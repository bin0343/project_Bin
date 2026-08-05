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
    public GameObject blurPanel;
    public GameObject namePanel;
    public Button renameButton;
    public Button confirmRenameButton;
    public Button[] presetButtons = new Button[3];


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

    private int currentPresetIndex = 0;

    private bool isQuitting = false;    //오류 방지

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void Start()
    {
        for (int i = 0; i < plusButtons.Length; i++)
        {
            int index = i;
            plusButtons[i].onClick.AddListener(() => OpenSubPanel(index));
        }

        for (int i = 0; i < presetButtons.Length; i++)
        {
            int index = i;
            presetButtons[i].onClick.AddListener( () => OnClickLoadPreset(index));
        }

        executeFormationButton.onClick.AddListener(ExecuteFormation);
        if (renameButton != null) renameButton.onClick.AddListener(OnClickRename);
        if (confirmRenameButton != null) confirmRenameButton.onClick.AddListener(OnClickConfirmRename);

        if (presetNameInput != null) presetNameInput.characterLimit = 15;
    }

    public void OpenFormationWindow()
    {
        gameObject.SetActive(true);
        if (formationCamera != null) formationCamera.gameObject.SetActive(true);
        subPanel_CharacterSelect.SetActive(false);
        CloseNamePanel();

        wasOpened = true;

        if (Character_Manager.Instance != null && presetNameText != null)
        {
            PartyPreset preset = Character_Manager.Instance.GetPreset(currentPresetIndex);
            if (preset != null && !string.IsNullOrEmpty(preset.presetName))
            {
                presetNameText.text = preset.presetName;
            }
            else
            {
                presetNameText.text = $"파티 {currentPresetIndex + 1}";
            }
        }

        List<Character_Data> currentData = Character_Manager.Instance.currentPartyData;
        for (int i = 0; i < 3; i++)
        {
            tempParty[i] = (currentData != null && i < currentData.Count) ? currentData[i] : null;
        }

        CompactParty();
        Refresh3DStage();
    }

    private void OnDisable()
    {
        if (isQuitting) return;

        CloseNamePanel();

        if (Character_Manager.Instance == null || !wasOpened) return;
        CompactParty();

        bool isEmptyParty = true;
        for (int i = 0; i < 3; i++)
        {
            if (tempParty[i] != null)
            {
                isEmptyParty = false;
                break;
            }
        }

        if (isEmptyParty)
        {
            currentPresetIndex = 0;
            LoadPresetToTemp(0); // 중복 로직을 함수 하나로 통합 처리
            CompactParty();
        }
        else
        {
            AutoSaveCurrentPreset();
        }

        List<Character_Data> finalData = new List<Character_Data>();
        List<string> finalIDs = new List<string>();

        for (int i = 0; i < 3; i++)
        {
            finalData.Add(tempParty[i]);
            finalIDs.Add(tempParty[i] != null ? tempParty[i].characterID : "");
        }

        Character_Manager.Instance.SaveParty(finalIDs, finalData);

        if (BattleManager.Instance != null) BattleManager.Instance.InitializeParty();
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
        CloseNamePanel();
        
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

    public void CloseNamePanel()
    {
        if (namePanel != null) namePanel.SetActive(false);
        if (blurPanel != null) blurPanel.SetActive(false);
        if (presetNameInput != null) presetNameInput.text = "";
    }

    private void RefreshRosterList()
    {
        roasterSlotUIList.Clear();
        foreach (Transform child in rosterListParent) Destroy(child.gameObject);

        List<Character_Data> ownedChars = Character_Manager.Instance.GetOwnedCharacters();

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
        CharacterStatus status = Character_Manager.Instance.GetCharacterStatus(data.characterID);
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
            currentPreviewModel.transform.localRotation = Quaternion.identity;

            // 캐싱 처리 등록
            cachedPreviewModels.Add(data.characterID, currentPreviewModel);
        }

        currentPreviewModel.transform.localPosition = new Vector3(0, data.uiPreviewYOffset, 0);
        currentPreviewModel.transform.localScale = Vector3.one * data.uiPreviewScale;

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
        CloseNamePanel();
        if (formationCamera != null) formationCamera.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void OnClickRename()
    {
        if (namePanel != null)
        {
            namePanel.SetActive(true);
            blurPanel.SetActive(true);

            if (presetNameInput != null && presetNameText != null)
            {
                presetNameInput.text = presetNameText.text;
            }
        }
    }

    public void OnClickConfirmRename()
    {
        if (presetNameInput != null && presetNameText != null)
        {
            if (!string.IsNullOrWhiteSpace(presetNameInput.text))
            {
                presetNameText.text = presetNameInput.text.Trim();
            }
        }
        CloseNamePanel();
    }

    public void OnClickSavePreset()
    {
        AutoSaveCurrentPreset();
    }

    public void OnClickLoadPreset(int index)
    {
        AutoSaveCurrentPreset();
        CloseNamePanel();

        if (presetNameText == null)
        {
            Debug.LogError("UI_PartyFormation 인스펙터 창에서 'Preset Name Text'를 연결했는지 확인하세요!");
            return;
        }

        currentPresetIndex = index;
        PartyPreset preset = Character_Manager.Instance.GetPreset(index);

        if (preset == null || string.IsNullOrEmpty(preset.presetName) || preset.characterIDs == null)
        {
            presetNameText.text = $"파티 {index + 1}";
            for (int i = 0; i < 3; i++) tempParty[i] = null;
            Refresh3DStage();
            return;
        }

        presetNameText.text = preset.presetName;
        LoadPresetToTemp(index);
        Refresh3DStage();
    }

    private void AutoSaveCurrentPreset()
    {
        if (Character_Manager.Instance == null || presetNameText == null) return;

        List<string> ids = new List<string>();
        foreach (var charData in tempParty)
        {
            ids.Add(charData != null ? charData.characterID : "");
        }

        Character_Manager.Instance.SavePreset(currentPresetIndex, presetNameText.text, ids);
    }

    private void LoadPresetToTemp(int index)
    {
        PartyPreset preset = Character_Manager.Instance.GetPreset(index);
        List<Character_Data> allData = Character_Manager.Instance.allcharacterDataList;

        if (preset == null || preset.characterIDs == null || allData == null) return;

        for (int i = 0; i < 3; i++)
        {
            string id = (i < preset.characterIDs.Count) ? preset.characterIDs[i] : "";
            tempParty[i] = allData.Find(d => d != null && d.characterID == id);
        }
    }
}