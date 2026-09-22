using UnityEngine;

public class VehicleInteractable : MonoBehaviour
{
    [Header("차량")]
    [SerializeField] private Transform vehicleRoot;

    [Header("탑승 위치")]
    [SerializeField] private Transform driverSeat;

    [Header("하차 위치")]
    [SerializeField] private Transform leftExitPoint;
    [SerializeField] private Transform rightExitPoint;

    [Header("하차 공간 검사")]
    [SerializeField] private LayerMask exitObstacleLayer;

    [SerializeField] private float exitCheckRadius = 0.35f;
    [SerializeField] private float exitCheckHeight = 1.8f;

    [Header("하차 설정")]
    [Tooltip("이 속도 이하일 때만 하차 가능 (m/s)")]
    [SerializeField] private float maxExitSpeed = 1f;

    [SerializeField] private KeyCode interactKey = KeyCode.F;


    private GameObject nearbyPlayer;
    private GameObject currentDriver;

    private Player_Move driverMove;
    private Player_Action driverAction;
    private Rigidbody vehicleRigidbody;
    private Rigidbody driverRigidbody;
    private Transform driverBody;

    private Collider[] driverColliders;
    private bool[] savedColliderStates;

    private bool savedMoveEnabled;
    private bool savedActionEnabled;
    private bool savedIsKinematic;
    private bool savedBodyActive;

    public bool IsOccupied
    {
        get { return currentDriver != null; }
    }

    private PlayerVehicleController vehicleController;

    private void Awake()
    {
        if (vehicleRoot != null)
        {
            vehicleRigidbody = vehicleRoot.GetComponent<Rigidbody>();

            vehicleController = vehicleRoot.GetComponent<PlayerVehicleController>();
        }

        SetVehicleParked(true);
    }

    private void Update()
    {
        // 이미 탑승한 상태
        if (currentDriver != null)
        {
            if (Input.GetKeyDown(interactKey))
            {
                // 1. 먼저 속도 검사
                if (!CanExitVehicle())
                {
                    if (UI_Manager.Instance != null)
                    {
                        UI_Manager.Instance.ShowMessage("차량이 충분히 느려진 후 하차할 수 있습니다.");
                    }

                    return;
                }

                // 2. 하차할 공간 검사
                if (!TryGetSafeExitPoint(out Transform safeExitPoint))
                {
                    if (UI_Manager.Instance != null)
                    {
                        UI_Manager.Instance.ShowMessage("하차할 공간이 없습니다.");
                    }

                    return;
                }

                // 3. 실제 하차
                ExitVehicle(safeExitPoint);
            }

            return;
        }

        // 아직 탑승하지 않은 상태
        if (nearbyPlayer != null && Input.GetKeyDown(interactKey))
        {
            EnterVehicle(nearbyPlayer);
        }
    }


    private void LateUpdate()
    {
        if (currentDriver == null) return;

        if (driverSeat == null) return;

        // 숨겨진 플레이어 Root를 차량 위치에 계속 유지
        currentDriver.transform.SetPositionAndRotation(driverSeat.position, driverSeat.rotation);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (currentDriver != null) return;

        Player_Action playerAction = other.GetComponentInParent<Player_Action>();

        if (playerAction == null) return;

        if (BattleManager.instance == null) return;

        GameObject activePlayer = BattleManager.instance.GetActiveCharacter();

        if (activePlayer == null) return;

        if (playerAction.gameObject != activePlayer) return;

        nearbyPlayer = activePlayer;

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.ShowInteractionPrompt($"{interactKey} : 탑승하기");
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (nearbyPlayer == null) return;

        Player_Action playerAction = other.GetComponentInParent<Player_Action>();

        if (playerAction == null) return;

        if (playerAction.gameObject != nearbyPlayer) return;

        nearbyPlayer = null;

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.HideInteractionPrompt();
        }
    }


    private void EnterVehicle(GameObject player)
    {
        if (player == null) return;

        if (vehicleRoot == null || driverSeat == null)
        {
            Debug.LogWarning("[Vehicle] Vehicle Root 또는 Driver Seat가 없습니다.");

            return;
        }

        currentDriver = player;
        nearbyPlayer = null;

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.HideInteractionPrompt();
        }

        driverMove = player.GetComponent<Player_Move>();
        driverAction = player.GetComponent<Player_Action>();
        driverRigidbody = player.GetComponent<Rigidbody>();

        if (driverMove != null)
        {
            savedMoveEnabled = driverMove.enabled;
            driverBody = driverMove.CharacterBody;
        }

        if (driverAction != null)
        {
            savedActionEnabled = driverAction.enabled;
        }

        if (driverBody != null)
        {
            savedBodyActive = driverBody.gameObject.activeSelf;
        }

        CacheAndDisableColliders(player);

        if (driverRigidbody != null)
        {
            savedIsKinematic = driverRigidbody.isKinematic;

            if (!driverRigidbody.isKinematic)
            {
                driverRigidbody.velocity = Vector3.zero;
                driverRigidbody.angularVelocity = Vector3.zero;
            }

            driverRigidbody.isKinematic = true;
        }

        if (driverMove != null)
        {
            driverMove.enabled = false;
        }

        if (driverAction != null)
        {
            driverAction.enabled = false;
        }

        if (driverBody != null)
        {
            driverBody.gameObject.SetActive(false);
        }

        player.transform.SetPositionAndRotation(driverSeat.position, driverSeat.rotation);

        if (BattleManager.instance != null)
        {
            // 캐릭터 스위칭 등 플레이어 조작 잠금
            BattleManager.instance.SetPlayerControlLocked(true);
            // 차량에서는 FreeLook 카메라 회전은 계속 가능해야 함
            BattleManager.instance.SetFreeLookInputLocked(false);
            BattleManager.instance.ChangeCameraTarget(vehicleRoot);
            BattleManager.instance.SetMapTrackingTarget(vehicleRoot);
        }

        SetVehicleParked(false);

        if (vehicleController != null)
        {
            vehicleController.SetDriving(true);
        }

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.HideInteractionPrompt();
            UI_Manager.Instance.SetVehicleHUD(true);
            UI_Manager.Instance.ShowMessage("차량에 탑승했습니다.");
        }
    }


    private void ExitVehicle(Transform selectedExitPoint)
    {
        if (currentDriver == null) return;

        if (selectedExitPoint == null)
        {
            Debug.LogWarning("[Vehicle] 선택된 하차 위치가 없습니다.");
            return;
        }

        GameObject player = currentDriver;

        player.transform.SetPositionAndRotation(selectedExitPoint.position, selectedExitPoint.rotation);

        if (vehicleController != null)
        {
            vehicleController.SetDriving(false);
        }

        SetVehicleParked(true);

        if (driverRigidbody != null)
        {
            driverRigidbody.position = selectedExitPoint.position;
            driverRigidbody.rotation = selectedExitPoint.rotation;
            // 플레이어 Rigidbody를 원래 물리 상태로 복구
            driverRigidbody.isKinematic = savedIsKinematic;

            // Dynamic 상태에서만 속도 초기화
            if (!driverRigidbody.isKinematic)
            {
                driverRigidbody.velocity = Vector3.zero;
                driverRigidbody.angularVelocity = Vector3.zero;
            }
        }

        RestoreColliders();

        if (driverBody != null)
        {
            driverBody.gameObject.SetActive(savedBodyActive);
        }

        if (driverMove != null)
        {
            driverMove.enabled = savedMoveEnabled;
        }

        if (driverAction != null)
        {
            driverAction.enabled = savedActionEnabled;
        }

        Physics.SyncTransforms();

        if (BattleManager.instance != null)
        {
            BattleManager.instance.SetPlayerControlLocked(false);
            BattleManager.instance.ChangeCameraTarget(player.transform);
            BattleManager.instance.SetMapTrackingTarget(player.transform);
        }

        currentDriver = null;
        driverMove = null;
        driverAction = null;
        driverRigidbody = null;
        driverBody = null;

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.SetVehicleHUD(false);
            UI_Manager.Instance.ShowMessage("차량에서 하차했습니다.");
        }
    }

    private bool IsExitPointClear(Transform exitPoint)
    {
        if (exitPoint == null) return false;

        Vector3 bottom = exitPoint.position + Vector3.up * exitCheckRadius;

        Vector3 top = exitPoint.position + Vector3.up * (exitCheckHeight - exitCheckRadius);

        Collider[] hits = Physics.OverlapCapsule(bottom, top, exitCheckRadius, exitObstacleLayer, QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;

            // 자기 차량 Collider는 무시
            if (vehicleRoot != null && hit.transform.IsChildOf(vehicleRoot)) continue;

            if (vehicleRoot != null && hit.transform == vehicleRoot) continue;

            // 탑승 중인 플레이어 자신의 Collider도 무시
            if (currentDriver != null && hit.transform.IsChildOf(currentDriver.transform)) continue;

            // 자기 자신 외에 뭔가 있다.
            return false;
        }

        return true;
    }

    private bool TryGetSafeExitPoint(out Transform safeExitPoint)
    {
        safeExitPoint = null;

        // 먼저 운전석 쪽, 왼쪽
        if (IsExitPointClear(leftExitPoint))
        {
            safeExitPoint = leftExitPoint;
            return true;
        }

        // 왼쪽이 막혔다면 오른쪽
        if (IsExitPointClear(rightExitPoint))
        {
            safeExitPoint = rightExitPoint;
            return true;
        }

        return false;
    }

    private void CacheAndDisableColliders(GameObject player)
    {
        driverColliders = player.GetComponentsInChildren<Collider>(true);

        savedColliderStates = new bool[driverColliders.Length];


        for (int i = 0; i < driverColliders.Length; i++)
        {
            savedColliderStates[i] = driverColliders[i].enabled;

            driverColliders[i].enabled = false;
        }
    }


    private void RestoreColliders()
    {
        if (driverColliders == null || savedColliderStates == null) return;


        for (int i = 0; i < driverColliders.Length; i++)
        {
            if (driverColliders[i] == null) continue;

            driverColliders[i].enabled = savedColliderStates[i];
        }
    }

    private void SetVehicleParked(bool parked)
    {
        if (vehicleRigidbody == null) return;

        if (parked)
        {
            if (!vehicleRigidbody.isKinematic)
            {
                vehicleRigidbody.velocity = Vector3.zero;

                vehicleRigidbody.angularVelocity = Vector3.zero;
            }

            vehicleRigidbody.isKinematic = true;
        }
        else
        {
            vehicleRigidbody.isKinematic = false;
        }
    }

    private float GetVehicleHorizontalSpeed()
    {
        if (vehicleRigidbody == null) return 0f;

        Vector3 velocity = vehicleRigidbody.velocity;

        velocity.y = 0f;

        return velocity.magnitude;
    }

    private bool CanExitVehicle()
    {
        float currentSpeed = GetVehicleHorizontalSpeed();

        return currentSpeed <= maxExitSpeed;
    }
}