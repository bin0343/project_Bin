using UnityEngine;

public class VehicleInteractable : MonoBehaviour
{
    [Header("차량")]
    [SerializeField] private Transform vehicleRoot;

    [Header("탑승 위치")]
    [SerializeField] private Transform driverSeat;

    [Header("하차 위치")]
    [SerializeField] private Transform exitPoint;

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
                ExitVehicle();
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

        Debug.Log("[Vehicle] F 키를 눌러 탑승할 수 있습니다.");
    }


    private void OnTriggerExit(Collider other)
    {
        if (nearbyPlayer == null) return;

        Player_Action playerAction = other.GetComponentInParent<Player_Action>();

        if (playerAction == null) return;

        if (playerAction.gameObject != nearbyPlayer) return;

        nearbyPlayer = null;
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
        }

        SetVehicleParked(false);

        if (vehicleController != null)
        {
            vehicleController.SetDriving(true);
        }

        Debug.Log("[Vehicle] 차량 탑승 완료");
    }


    private void ExitVehicle()
    {
        if (currentDriver == null) return;

        if (exitPoint == null)
        {
            Debug.LogWarning("[Vehicle] Exit Point가 없습니다.");

            return;
        }

        GameObject player =currentDriver;

        player.transform.SetPositionAndRotation(exitPoint.position, exitPoint.rotation);

        if (vehicleController != null)
        {
            vehicleController.SetDriving(false);
        }

        SetVehicleParked(true);

        if (driverRigidbody != null)
        {
            driverRigidbody.position = exitPoint.position;
            driverRigidbody.rotation = exitPoint.rotation;
            // 먼저 원래 물리 상태로 되돌린다.
            driverRigidbody.isKinematic = savedIsKinematic;
            // Dynamic 상태일 때만 Velocity를 건드린다.
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
        }

        currentDriver = null;

        driverMove = null;
        driverAction = null;
        driverRigidbody = null;
        driverBody = null;

        Debug.Log("[Vehicle] 차량 하차 완료");
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
}