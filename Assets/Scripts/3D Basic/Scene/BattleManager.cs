using Cinemachine;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("카메라 세팅")]
    public CinemachineFreeLook mainFreeLookCamera;

    [Header("캐릭터 태그")]
    public Transform startSpawnPoint;

    private bool hasCachedDefaultCameraOrbit;
    private float defaultFreeLookXAxis;
    private float defaultFreeLookYAxis;
    private float defaultCharacterBodyYaw;

    private GameObject[] spawnedCharacters = new GameObject[3];
    private int currentActiveIndex = 0;

    public GameObject[] SpawnedCharacters => spawnedCharacters;

    private bool isPlayerControlLocked;

    private bool isFreeLookInputCached;
    private string savedFreeLookXAxisName;
    private string savedFreeLookYAxisName;

    public bool IsPlayerControlLocked
    {
        get { return isPlayerControlLocked; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) return;
        Instance = this;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InitializeParty();

        CacheDefaultCameraOrbit();
    }

    private void Update()
    {
        if (isPlayerControlLocked) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) TryTag(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryTag(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryTag(2);
    }

    public void InitializeParty()
    {
        if (Character_Manager.Instance == null) return;

        List<Character_Data> party = Character_Manager.Instance.currentPartyData;

        Vector3 spawnPos = (startSpawnPoint != null) ? startSpawnPoint.position : Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        string savedActiveCharacterID = "";
        GameObject activeChar = GetActiveCharacter();
        if (activeChar != null)
        {
            spawnPos = activeChar.transform.position;
            spawnRot = activeChar.transform.rotation;
            Character_Stat stat = activeChar.GetComponent<Character_Stat>();
            if (stat != null && stat.characterData != null)
            {
                savedActiveCharacterID = stat.characterData.characterID;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (spawnedCharacters[i] != null)
            {
                Destroy(spawnedCharacters[i]);
                spawnedCharacters[i] = null;
            }
        }

        currentActiveIndex = 0;

        for (int i = 0; i < 3; i++)
        {
            if (i < party.Count && party[i] != null)
            {
                GameObject charObj = Instantiate(party[i].characterPrefab, spawnPos, spawnRot);
                spawnedCharacters[i] = charObj;

                if (!string.IsNullOrEmpty(savedActiveCharacterID) && party[i].characterID == savedActiveCharacterID)
                {
                    currentActiveIndex = i;
                }

                charObj.SetActive(false);
            }
        }

        if (spawnedCharacters[currentActiveIndex] != null)
        {
            spawnedCharacters[currentActiveIndex].SetActive(true);
            ChangeCameraTarget(spawnedCharacters[currentActiveIndex].transform);
            UpdateSystemsWithActiveCharacter(spawnedCharacters[currentActiveIndex]);
        }
    }

    private void TryTag(int targetIndex)
    {
        if (targetIndex == currentActiveIndex) return;

        if (spawnedCharacters[targetIndex] == null)
        {
            Debug.Log($"슬롯 {targetIndex}에 캐릭터가 없어 소환을 시도합니다.");
            InitializeParty();
            if (spawnedCharacters[targetIndex] == null) return;
        }

        GameObject currentActiveObj = spawnedCharacters[currentActiveIndex];
        GameObject targetObj = spawnedCharacters[targetIndex];

        Player_Equipment currentEquipment = currentActiveObj.GetComponent<Player_Equipment>();

        bool wasInCombat = currentEquipment != null && currentEquipment.IsInCombat;

        float remainingCombatTime = currentEquipment != null ? currentEquipment.RemainingCombatTime : 0f;

        Player_Action currentAction = currentActiveObj.GetComponent<Player_Action>();

        if (currentAction != null && !currentAction.IsGrounded)
        {
            return;
        }

        Vector3 currentPos = currentActiveObj.transform.position;
        Quaternion currentRot = currentActiveObj.transform.rotation;

        Rigidbody currentRb = currentActiveObj.GetComponent<Rigidbody>();

        Vector3 savedVelocity = currentRb != null ? currentRb.velocity : Vector3.zero;
        bool wasGrounded = currentAction != null ? currentAction.IsGrounded : true;

        int currentAnimHash = 0;
        float currentAnimTime = 0f;
        if (currentAction != null && currentAction.animator != null)
        {
            AnimatorStateInfo stateInfo = currentAction.animator.GetCurrentAnimatorStateInfo(0);
            currentAnimHash = stateInfo.fullPathHash;
            currentAnimTime = stateInfo.normalizedTime;
        }

        if (currentEquipment != null && wasInCombat)
        {
            currentEquipment.ClearCombatState();
        }

        currentActiveObj.SetActive(false);

        targetObj.transform.position = currentPos;
        targetObj.transform.rotation = currentRot;
        targetObj.SetActive(true);

        if (wasInCombat && remainingCombatTime > 0f)
        {
            Player_Equipment targetEquipment = targetObj.GetComponent<Player_Equipment>();

            if (targetEquipment != null)
            {
                targetEquipment.EnterCombatState(remainingCombatTime);
            }
        }

        Rigidbody targetRb = targetObj.GetComponent<Rigidbody>();
        Player_Action targetAction = targetObj.GetComponent<Player_Action>();

        if (targetRb != null)
        {
            targetRb.velocity = savedVelocity;
        }

        if (targetAction != null)
        {
            targetAction.IsGrounded = wasGrounded;

            if (wasGrounded)
            {
                if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                {
                    targetAction.ChangeState(new PlayerMoveState());
                }
                else
                {
                    targetAction.ChangeState(new PlayerIdleState());
                }
            }
            else
            {
                targetAction.ForceJumpAirborne();
            }

            if (targetAction.animator != null && currentAnimHash != 0)
            {
                targetAction.animator.Play(currentAnimHash, 0, currentAnimTime % 1f);

                targetAction.animator.Update(0f);
            }
        }

        ChangeCameraTarget(targetObj.transform);

        currentActiveIndex = targetIndex;

        UpdateSystemsWithActiveCharacter(targetObj);
    }

    //캐릭터 교체마다 불러오는 함수
    public void ChangeCameraTarget(Transform newCharacterRoot)
    {
        if (mainFreeLookCamera == null || newCharacterRoot == null) return;

        Transform targetPoint = GetCameraTarget(newCharacterRoot);

        mainFreeLookCamera.Follow = targetPoint;
        mainFreeLookCamera.LookAt = targetPoint;
    }

    public Transform GetCameraTarget(Transform characterRoot)
    {
        if (characterRoot == null) return null;

        Transform targetPoint = characterRoot.Find("Camera_Target");

        if (targetPoint != null) return targetPoint;

        return characterRoot;
    }

    private Transform GetCharacterBody(GameObject character)
    {
        if (character == null)
        {
            return null;
        }

        Player_Move playerMove = character.GetComponent<Player_Move>();

        if (playerMove != null && playerMove.CharacterBody != null)
        {
            return playerMove.CharacterBody;
        }

        Player_Action playerAction = character.GetComponent<Player_Action>();

        if (playerAction != null && playerAction.animator != null)
        {
            return playerAction.animator.transform;
        }

        return character.transform;
    }

    private void UpdateSystemsWithActiveCharacter(GameObject activeCharacter)
    {
        Character_Stat newStat = activeCharacter.GetComponent<Character_Stat>();
        if (newStat != null && UI_Manager.Instance != null)
        {
            UI_Manager.Instance.UpdatePlayerStatus(newStat);
        }

        MiniMapController miniMap = FindObjectOfType<MiniMapController>();
        if (miniMap != null)
        {
            miniMap.SetTarget(activeCharacter.transform);
        }
    }

    public GameObject GetActiveCharacter()
    {
        return spawnedCharacters[currentActiveIndex];
    }

    public void PreparePartyForSceneTransition()
    {
        GameObject activeCharacter = GetActiveCharacter();

        if (activeCharacter == null) return; 

        Player_Action playerAction = activeCharacter.GetComponent<Player_Action>();

        if (playerAction != null && !playerAction.IsDead)
        {
            playerAction.currentWeapon?.ForceStopTrail();
            playerAction.currentWeapon?.DisableHitbox();

            playerAction.ChangeState(new PlayerIdleState());
        }

        Rigidbody activeRigidbody = activeCharacter.GetComponent<Rigidbody>();

        if (activeRigidbody != null)
        {
            activeRigidbody.velocity = Vector3.zero;
            activeRigidbody.angularVelocity = Vector3.zero;
        }

        PlayerAttackState.ResetCombo();
    }

    public void MovePartyToSpawnPoint(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogError("[BattleManager] 이동할 SpawnPoint가 없습니다.");
            return;
        }

        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        GameObject activeCharacterBeforeMove = GetActiveCharacter();

        Transform cameraTarget = null;
        Vector3 oldCameraTargetPosition = Vector3.zero;

        if (activeCharacterBeforeMove != null)
        {
            cameraTarget = GetCameraTarget(activeCharacterBeforeMove.transform);

            if (cameraTarget != null)
            {
                oldCameraTargetPosition = cameraTarget.position;
            }
        }

        for (int i = 0; i < spawnedCharacters.Length; i++)
        {
            GameObject character = spawnedCharacters[i];

            if (character == null) continue;

            character.transform.SetPositionAndRotation(spawnPosition,Quaternion.identity);
            Rigidbody characterRigidbody = character.GetComponent<Rigidbody>();

            if (characterRigidbody != null)
            {
                characterRigidbody.position = spawnPosition;
                characterRigidbody.rotation = Quaternion.identity;
                characterRigidbody.velocity = Vector3.zero;
                characterRigidbody.angularVelocity = Vector3.zero;
            }

            Player_Move playerMove = character.GetComponent<Player_Move>();

            if (playerMove != null && playerMove.CharacterBody != null)
            {
                playerMove.CharacterBody.rotation = spawnRotation;
            }
            else
            {
                Player_Action playerAction = character.GetComponent<Player_Action>();

                if (playerAction != null && playerAction.animator != null)
                {
                    playerAction.animator.transform.rotation = spawnRotation;
                }
            }
        }

        Physics.SyncTransforms();

        if (mainFreeLookCamera != null && cameraTarget != null)
        {
            Vector3 warpDelta = cameraTarget.position - oldCameraTargetPosition;

            mainFreeLookCamera.OnTargetObjectWarped(cameraTarget, warpDelta);
        }

        GameObject activeCharacter = GetActiveCharacter();

        if (activeCharacter != null)
        {
            ResetFreeLookToDefaultBehind(activeCharacter);

            UpdateSystemsWithActiveCharacter(activeCharacter);
        }
    }

    private void CacheDefaultCameraOrbit()
    {
        if (mainFreeLookCamera == null) return;

        GameObject activeCharacter = GetActiveCharacter();

        if (activeCharacter == null) return;

        Transform characterBody = GetCharacterBody(activeCharacter);

        if (characterBody == null) return;

        defaultFreeLookXAxis = mainFreeLookCamera.m_XAxis.Value;

        defaultFreeLookYAxis = mainFreeLookCamera.m_YAxis.Value;

        defaultCharacterBodyYaw = characterBody.eulerAngles.y;

        hasCachedDefaultCameraOrbit = true;

        Debug.Log($"[BattleManager] 기본 카메라 구도 저장 " + $"X: {defaultFreeLookXAxis:F1}, " + $"Y: {defaultFreeLookYAxis:F2}, " + $"Player Yaw: {defaultCharacterBodyYaw:F1}");
    }

    private void ResetFreeLookToDefaultBehind(GameObject activeCharacter)
    {
        if (mainFreeLookCamera == null || activeCharacter == null) return;

        if (!hasCachedDefaultCameraOrbit)
        {
            CacheDefaultCameraOrbit();

            if (!hasCachedDefaultCameraOrbit) return;
        }

        Transform characterBody = GetCharacterBody(activeCharacter);

        if (characterBody == null) return;

        float characterYawDelta = Mathf.DeltaAngle(defaultCharacterBodyYaw, characterBody.eulerAngles.y);
        
        float targetXAxis = defaultFreeLookXAxis + characterYawDelta;

        targetXAxis = Mathf.DeltaAngle(0f, targetXAxis);

        mainFreeLookCamera.m_XAxis.Value = targetXAxis;
        mainFreeLookCamera.m_YAxis.Value = defaultFreeLookYAxis;

        mainFreeLookCamera.m_XAxis.m_InputAxisValue = 0f;
        mainFreeLookCamera.m_YAxis.m_InputAxisValue = 0f;

        mainFreeLookCamera.PreviousStateIsValid = false;
    }

    #region ControlLocked
    public void SetPlayerControlLocked(bool locked)
    {
        isPlayerControlLocked = locked;

        GameObject activeCharacter = GetActiveCharacter();

        if (activeCharacter != null)
        {
            Player_Action playerAction = activeCharacter.GetComponent<Player_Action>();

            if (playerAction != null)
            {
                playerAction.SetControlLocked(locked);
            }
        }

        SetFreeLookInputLocked(locked);
    }

    public void SetFreeLookInputLocked(bool locked)
    {
        if (mainFreeLookCamera == null) return;

        if (!isFreeLookInputCached)
        {
            savedFreeLookXAxisName = mainFreeLookCamera.m_XAxis.m_InputAxisName;
            savedFreeLookYAxisName = mainFreeLookCamera.m_YAxis.m_InputAxisName;

            isFreeLookInputCached = true;
        }

        if (locked)
        {
            mainFreeLookCamera.m_XAxis.m_InputAxisName = string.Empty;
            mainFreeLookCamera.m_YAxis.m_InputAxisName = string.Empty;

            mainFreeLookCamera.m_XAxis.m_InputAxisValue = 0f;
            mainFreeLookCamera.m_YAxis.m_InputAxisValue = 0f;
        }
        else
        {
            mainFreeLookCamera.m_XAxis.m_InputAxisName = savedFreeLookXAxisName;
            mainFreeLookCamera.m_YAxis.m_InputAxisName = savedFreeLookYAxisName;
        }
    }

    #endregion
}