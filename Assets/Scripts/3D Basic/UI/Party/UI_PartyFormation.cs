using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PartyFormation : MonoBehaviour
{
    [Header("3D 무대 세팅")]
    public Camera formationCamera;
    public Transform[] padTransforms = new Transform[3];
    private GameObject[] spawnedModels = new GameObject[3];

    [Header("메인 UI 세팅")]
    public Button[] plusButtons = new Button[3];

    [Header("모델 보정(높이, 크기")]
    public float stageModelScale = 1.0f;
    public float stageModelYOffset = 0.0f;

    public float previewModelScale = 1.0f;
    public float previewModelYOffset = 0.0f;

    [Header("프리셋 및 이름 UI")]
    public Text presetNameText;
    public InputField presetNameInput;
    public Button renameButton;

    [Header("서브 탭 (캐릭터 선택창)")]
    public GameObject subPanel_CharacterSelect;
    public Transform rosterListParent;
    public GameObject rosterSlotPrefab;
    public Transform previewSpawnPoint;
    public Text previewNameText;
    public Text previewLevelText;
    public Button executeFormationButton;

    public GameObject previewStageRoot;
    private GameObject currentPreviewModel;
    private Character_Data selectedDataForSlot;
    private int currentEditingSlotIndex = -1;
    private Dictionary<string, GameObject> cachedPreviewModels = new Dictionary<string, GameObject>();

    private Character_Data[] tempParty = new Character_Data[3];
    private List<UI_PartySlot> roasterSlotUIList = new List<UI_PartySlot>();

    private Character_Data[] spawnedData = new Character_Data[3];
    private HashSet<string> playedFirstIdleSet = new HashSet<string>();
    private bool wasOpened = false; // 게임 실행 시 처음에 비활성화 돼서 저장된 데이터 초기화 되는 현상 방지

    private void Start()
    {
        for (int i = 0; i < plusButtons.Length; i++)
        {
            int index = i;
            plusButtons[i].onClick.AddListener(() => OpenSubPanel(index));
        }

        executeFormationButton.onClick.AddListener(ExecuteFormation);
    }

    public void OpenFormationWindow()
    {
        gameObject.SetActive(true);
        if (formationCamera != null) formationCamera.gameObject.SetActive(true);
        subPanel_CharacterSelect.SetActive(false);

        wasOpened = true;

        List<Character_Data> currentData = Character_Manager.instance.currentPartyData;
        for (int i = 0; i < 3; i++)
        {
            tempParty[i] = (currentData != null && i < currentData.Count) ? currentData[i] : null;
        }

        CompactParty();
        Refresh3DStage();
    }

    private void OnDisable()
    {
        if (Character_Manager.instance == null || !wasOpened) return;
        CompactParty();

        List<Character_Data> finalData = new List<Character_Data>();
        List<string> finalIDs = new List<string>();

        for (int i = 0; i < 3; i++)
        {
            finalData.Add(tempParty[i]);
            finalIDs.Add(tempParty[i] != null ? tempParty[i].characterID : "");
        }

        Character_Manager.instance.SaveParty(finalIDs, finalData);

        if (BattleManager.instance != null) BattleManager.instance.InitializeParty();
        if (currentPreviewModel != null) currentPreviewModel.gameObject.SetActive(false);
    }

    private void Refresh3DStage()
    {
        for (int i = 0; i < 3; i++)
        {
            if (spawnedModels[i] != null && spawnedData[i] != tempParty[i])
            {
                Destroy(spawnedModels[i]);
                spawnedModels[i] = null;
                spawnedData[i] = null;
            }

            if (tempParty[i] != null)
            {
                if (spawnedModels[i] == null)
                {
                    Vector3 spawnPos = padTransforms[i].position + new Vector3(0, stageModelYOffset, 0);
                    spawnedModels[i] = Instantiate(tempParty[i].uiPrefab, spawnPos, padTransforms[i].rotation);
                    spawnedModels[i].transform.localScale = Vector3.one * stageModelScale;

                    spawnedData[i] = tempParty[i];

                    Animator anim = spawnedModels[i].GetComponentInChildren<Animator>();
                    if (anim != null)
                    {
                        anim.Rebind();

                        if (!playedFirstIdleSet.Contains(tempParty[i].characterID))
                        {
                            anim.Play("First Idle", 0, 0f);
                            playedFirstIdleSet.Add(tempParty[i].characterID);
                        }
                        else
                        {
                            anim.Play("Idle", 0, 0f);
                        }
                        anim.Update(0f);
                    }
                }
                else
                {
                    bool wasHidden = !spawnedModels[i].activeSelf;
                    spawnedModels[i].SetActive(true);

                    if (wasHidden)
                    {
                        Animator anim = spawnedModels[i].GetComponent<Animator>();
                        if (anim != null) anim.Play("Idle");
                    }
                }

                plusButtons[i].gameObject.SetActive(true);
                Image btnImg = plusButtons[i].GetComponent<Image>();
                if (btnImg != null) btnImg.color = new Color(1, 1, 1, 0);
            }
            else
            {
                plusButtons[i].gameObject.SetActive(true);
                Image btnImg = plusButtons[i].GetComponent<Image>();
                if (btnImg != null) btnImg.color = new Color(1, 1, 1, 1);
            }
        }
    }

    private void OpenSubPanel(int slotIndex)
    {
        if (previewStageRoot != null) previewStageRoot.SetActive(true);

        int targetIndex = slotIndex;
        if (tempParty[slotIndex] == null)
        {
            for (int i = 0; i < 3; i++)
            {
                if (tempParty[i] == null)
                {
                    targetIndex = i;
                    break;
                }
            }
        }
        currentEditingSlotIndex = slotIndex;
        subPanel_CharacterSelect.SetActive(true);

        for (int i = 0; i < 3; i++)
        {
            if (spawnedModels[i] != null) spawnedModels[i].SetActive(false);
            if (plusButtons[i] != null) plusButtons[i].gameObject.SetActive(false);
        }

        RefreshRosterList();

        if (tempParty[currentEditingSlotIndex] != null)
        {
            SelectCharacterInSubPanel(tempParty[currentEditingSlotIndex]);
        }
        else
        {
            selectedDataForSlot = null;
            if (currentPreviewModel != null) currentPreviewModel.SetActive(false);
            previewNameText.text = "캐릭터를 선택하세요";
            previewLevelText.text = "";
            executeFormationButton.interactable = false;
        }
    }

    public void CloseSubPanel()
    {
        if (currentPreviewModel != null) currentPreviewModel.SetActive(false);

        if (previewStageRoot != null) previewStageRoot.SetActive(false);
        subPanel_CharacterSelect.SetActive(false);

        Refresh3DStage();
    }

    private void RefreshRosterList()
    {
        roasterSlotUIList.Clear();
        foreach (Transform child in rosterListParent) Destroy(child.gameObject);

        List<Character_Data> ownedChars = Character_Manager.instance.GetOwnedCharacters();

        foreach (var charData in ownedChars)
        {
            GameObject slot = Instantiate(rosterSlotPrefab, rosterListParent);

            UI_PartySlot slotLogic = slot.GetComponent<UI_PartySlot>();
            if (slotLogic != null)
            {
                int assignedIndex = 0; // 기본값: 0 (미편성)

                for (int i = 0; i < 3; i++)
                {
                    if (tempParty[i] == charData)
                    {
                        assignedIndex = i + 1; // 0번 배열 -> 1번 자리
                        break;
                    }
                }

                slotLogic.Setup(charData, assignedIndex, false);
                roasterSlotUIList.Add(slotLogic);
            }

            Button btn = slot.GetComponent<Button>();
            btn.onClick.AddListener(() => SelectCharacterInSubPanel(charData));
        }
    }

    private void SelectCharacterInSubPanel(Character_Data data)
    {
        selectedDataForSlot = data;

        foreach (var slotLogic in roasterSlotUIList)
        {
            if (slotLogic == null) continue;

            bool isSelected = (slotLogic.data == selectedDataForSlot);
            int displayIndex = 0;

            if (isSelected)
            {
                displayIndex = currentEditingSlotIndex + 1;
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i == currentEditingSlotIndex) continue;

                    if (tempParty[i] == slotLogic.data)
                    {
                        displayIndex = i + 1;
                        break;
                    }
                }
            }

            slotLogic.Setup(slotLogic.data, displayIndex, isSelected);
        }

        previewNameText.text = data.characterName;
        CharacterStatus status = Character_Manager.instance.GetCharacterStatus(data.characterID);
        previewLevelText.text = $"Lv.{status.level}";
        executeFormationButton.interactable = true;

        if (currentPreviewModel != null) currentPreviewModel.SetActive(false);

        if (cachedPreviewModels.ContainsKey(data.characterID))
        {
            currentPreviewModel = cachedPreviewModels[data.characterID];
            currentPreviewModel.SetActive(true);
        }
        else
        {
            currentPreviewModel = Instantiate(data.uiPrefab, previewSpawnPoint.position, previewSpawnPoint.rotation);
            currentPreviewModel.transform.SetParent(previewSpawnPoint);
            currentPreviewModel.transform.localPosition = Vector3.zero;
            currentPreviewModel.transform.localRotation = Quaternion.identity;
            currentPreviewModel.transform.localScale = Vector3.one * previewModelScale;

            cachedPreviewModels.Add(data.characterID, currentPreviewModel);
        }

        Animator anim = currentPreviewModel.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.Rebind();       // 뼈대 및 상태 구조 태초의 상태로 강제 초기화
            anim.Play("First Idle", 0, 0f); // 애니메이션 재생 지시
            anim.Update(0f);
        }
    }

    private void ExecuteFormation()
    {
        if (selectedDataForSlot == null || currentEditingSlotIndex < 0) return;

        // 중복 편성 방지 (다른 자리에 이 캐릭터가 있으면 그 자리를 비움)
        for (int i = 0; i < 3; i++)
        {
            if (tempParty[i] == selectedDataForSlot) tempParty[i] = null;
        }

        if (currentPreviewModel != null) currentPreviewModel.SetActive(false);

        if (previewStageRoot != null) previewStageRoot.SetActive(false);

        tempParty[currentEditingSlotIndex] = selectedDataForSlot;

        CompactParty();

        subPanel_CharacterSelect.SetActive(false);
        Refresh3DStage();
    }

    // 파티의 빈 공간을 없애고 1번 슬롯부터 차례대로 당겨서 정렬하는 함수
    private void CompactParty()
    {
        List<Character_Data> compacted = new List<Character_Data>();

        for (int i = 0; i < 3; i++)
        {
            if (tempParty[i] != null) compacted.Add(tempParty[i]);
        }

        for (int i = 0; i < 3; i++)
        {
            tempParty[i] = (i < compacted.Count ? compacted[i] : null);
        }
    }

    public void SaveAndClose()
    {
        if (formationCamera != null) formationCamera.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}