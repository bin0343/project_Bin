using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [Header("카메라 세팅")]
    public CinemachineFreeLook mainFreeLookCamera;

    [Header("캐릭터 태그")]
    public Transform startSpawnPoint;

    private GameObject[] spawnedCharacters = new GameObject[3];
    private int currentActiveIndex = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InitializeParty();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) TryTag(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryTag(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryTag(2);
    }

    public void InitializeParty()
    {
        if (Character_Manager.instance == null) return;

        List<Character_Data> party = Character_Manager.instance.currentPartyData;
        Debug.Log($"초기화 시도 - 파티원 수: {party.Count}");

        for (int i = 0; i < 3; i++)
        {
            if (i < party.Count && party[i] != null)
            {
                if (spawnedCharacters[i] == null)
                {
                    Vector3 spawnPos = (startSpawnPoint != null) ? startSpawnPoint.position : Vector3.zero;
                    GameObject charObj = Instantiate(party[i].characterPrefab, spawnPos, Quaternion.identity);
                    spawnedCharacters[i] = charObj;

                    if (currentActiveIndex == i || (currentActiveIndex == 0 && i == 0))
                    {
                        charObj.SetActive(true);
                        ChangeCameraTarget(charObj.transform);
                        UpdateSystemsWithActiveCharacter(charObj);
                    }
                    else
                    {
                        charObj.SetActive(false);
                    }
                }
            }
            else
            {
                if (spawnedCharacters[i] != null)
                {
                    Destroy(spawnedCharacters[i]);
                    spawnedCharacters[i] = null;
                }
            }
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

        currentActiveObj.SetActive(false);

        targetObj.transform.position = currentPos;
        targetObj.transform.rotation = currentRot;
        targetObj.SetActive(true);

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

        Transform targetPoint = newCharacterRoot.Find("Camera_Target");

        if (targetPoint == null)
        {
            targetPoint = newCharacterRoot;
        }

        mainFreeLookCamera.Follow = targetPoint;
        mainFreeLookCamera.LookAt = targetPoint;
    }

    private void UpdateSystemsWithActiveCharacter(GameObject activeCharacter)
    {
        Character_Stat newStat = activeCharacter.GetComponent<Character_Stat>();
        if (newStat != null && UI_Manager.instance != null)
        {
            UI_Manager.instance.UpdatePlayerStatus(newStat);
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
}