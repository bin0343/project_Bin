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
    private bool isDeathHandling;

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
        if (isDeathHandling) return;

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

    private void TryTag(int targetIndex, bool force = false)
    {
        if (targetIndex == currentActiveIndex) return;

        if (spawnedCharacters[targetIndex] == null)
        {
            return;
        }

        GameObject currentActiveObj = spawnedCharacters[currentActiveIndex];
        GameObject targetObj = spawnedCharacters[targetIndex];

        Character_Stat targetStat = targetObj.GetComponent<Character_Stat>();

        if (targetStat != null && (targetStat.isDead || targetStat.currentHP <= 0)) return;

        Player_Equipment currentEquipment = currentActiveObj.GetComponent<Player_Equipment>();

        bool wasInCombat = currentEquipment != null && currentEquipment.IsInCombat;

        float remainingCombatTime = currentEquipment != null ? currentEquipment.RemainingCombatTime : 0f;

        Player_Action currentAction = currentActiveObj.GetComponent<Player_Action>();

        if (!force && currentAction != null && !currentAction.IsGrounded) return;

        Vector3 currentPos = currentActiveObj.transform.position;
        Quaternion currentRot = currentActiveObj.transform.rotation;

        Transform currentBody = GetCharacterBody(currentActiveObj);

        Quaternion currentBodyRotation = currentBody != null ? currentBody.rotation : currentRot;

        Rigidbody currentRb = currentActiveObj.GetComponent<Rigidbody>();

        Vector3 savedVelocity = (!force && currentRb != null) ? currentRb.velocity : Vector3.zero;
        bool wasGrounded = currentAction != null ? currentAction.IsGrounded : true;

        if (currentEquipment != null && wasInCombat)
        {
            currentEquipment.ClearCombatState();
        }

        currentActiveObj.SetActive(false);

        targetObj.transform.position = currentPos;
        targetObj.transform.rotation = currentRot;
        targetObj.SetActive(true);

        Transform targetBody = GetCharacterBody(targetObj);

        if (targetBody != null)
        {
            targetBody.rotation = currentBodyRotation;
        }

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
            if (force)
            {
                targetAction.IsGrounded = true;
                targetAction.ChangeState(new PlayerIdleState());
            }
            else
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

        if (spawnPoint.GetComponent<TeleportPoint3D>() != null)
        {
            spawnPosition = spawnPoint.position - (spawnPoint.forward * 2.0f);
            spawnRotation = Quaternion.LookRotation(-spawnPoint.forward);
        }

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

    private int FindNextAliveCharacter()
    {
        for (int offset = 1; offset < spawnedCharacters.Length; offset++)
        {
            int index = (currentActiveIndex + offset) % spawnedCharacters.Length;

            GameObject character = spawnedCharacters[index];

            if (character == null) continue;

            Character_Stat stat = character.GetComponent<Character_Stat>();

            if (stat == null) continue;

            if (stat.isDead || stat.currentHP <= 0) continue;

            return index;
        }

        return -1;
    }

    public void OnCharacterDead(Player_Action deadPlayer)
    {
        isDeathHandling = true;
    }

    public void OnDeadAnimationFinished(Player_Action deadPlayer)
    {
        int aliveIndex = FindNextAliveCharacter();

        if (aliveIndex >= 0)
        {
            TryTag(aliveIndex, true);

            isDeathHandling = false;
            return;
        }

        RespawnPartyAtNearestTeleport();

        isDeathHandling = false;
    }

    private TeleportPoint3D FindNearestActivatedTeleportPoint(Vector3 position)
    {
        TeleportPoint3D[] teleportPoints = FindObjectsOfType<TeleportPoint3D>(true);
        TeleportPoint3D nearestPoint = null;

        float nearestSqrDistance = float.MaxValue;

        foreach (TeleportPoint3D point in teleportPoints)
        {
            if (point == null) continue;

            if (!point.isActivated) continue;

            Vector3 difference = point.transform.position - position;

            float sqrDistance = difference.sqrMagnitude;

            if (sqrDistance >= nearestSqrDistance) continue;

            nearestSqrDistance = sqrDistance;
            nearestPoint = point;
        }

        return nearestPoint;
    }

    private void RespawnPartyAtNearestTeleport()
    {
        GameObject activeCharacter = GetActiveCharacter();

        if (activeCharacter == null) return;

        Vector3 deathPosition = activeCharacter.transform.position;

        TeleportPoint3D nearestPoint = FindNearestActivatedTeleportPoint(deathPosition);

        Transform respawnPoint;

        if (nearestPoint != null)
        {
            respawnPoint = nearestPoint.transform;
        }
        else
        {
            respawnPoint = startSpawnPoint;
        }

        if (respawnPoint == null)
        {
            Debug.LogError("[BattleManager] 부활할 위치가 없습니다.");

            return;
        }

        for (int i = 0; i < spawnedCharacters.Length; i++)
        {
            GameObject character = spawnedCharacters[i];

            if (character == null) continue;

            Character_Stat stat = character.GetComponent<Character_Stat>();

            Player_Action action = character.GetComponent<Player_Action>();

            stat?.Revive();
            action?.Revive();
        }

        MovePartyToSpawnPoint(respawnPoint);

        Debug.Log($"[BattleManager] 파티 전멸 → " + $"[{respawnPoint.name}]에서 부활");
    }

    public void RestorePartyAtTeleport()
    {
        for (int i = 0; i < spawnedCharacters.Length; i++)
        {
            GameObject character = spawnedCharacters[i];

            if (character == null) continue;

            Character_Stat stat = character.GetComponent<Character_Stat>();
            Player_Action action = character.GetComponent<Player_Action>();

            if (stat == null) continue;

            bool wasDead = stat.isDead || stat.currentHP <= 0 || (action != null && action.IsDead);

            stat.Revive();

            if (wasDead && action != null)
            {
                action.Revive();
            }
        }

        GameObject activeCharacter = GetActiveCharacter();

        if (activeCharacter != null)
        {
            UpdateSystemsWithActiveCharacter(activeCharacter);
        }

        Debug.Log("[BattleManager] 텔레포트 포인트에서 파티 HP를 회복했습니다.");
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