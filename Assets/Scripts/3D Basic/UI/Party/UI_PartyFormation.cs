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

    private GameObject currentPreviewModel;
    private Character_Data selectedDataForSlot; 
    private int currentEditingSlotIndex = -1;  

    private Character_Data[] tempParty = new Character_Data[3];

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

        List<Character_Data> currentData = Character_Manager.instance.currentPartyData;
        for (int i = 0; i < 3; i++)
        {
            tempParty[i] = (i < currentData.Count) ? currentData[i] : null;
        }

        Refresh3DStage();
    }

    private void Refresh3DStage()
    {
        for (int i = 0; i < 3; i++)
        {
            if (spawnedModels[i] != null) Destroy(spawnedModels[i]);

            if (tempParty[i] != null)
            {
                plusButtons[i].gameObject.SetActive(false);

                spawnedModels[i] = Instantiate(tempParty[i].uiPrefab, padTransforms[i].position, padTransforms[i].rotation);

                Animator anim = spawnedModels[i].GetComponentInChildren<Animator>();
                if (anim != null) anim.Play("First Idle");
            }
            else
            {
                plusButtons[i].gameObject.SetActive(true);
            }
        }
    }

    private void OpenSubPanel(int slotIndex)
    {
        currentEditingSlotIndex = slotIndex;
        subPanel_CharacterSelect.SetActive(true);
        selectedDataForSlot = null;

        // 미리보기 초기화
        if (currentPreviewModel != null) Destroy(currentPreviewModel);
        previewNameText.text = "캐릭터를 선택하세요";
        previewLevelText.text = "";
        executeFormationButton.interactable = false;

        RefreshRosterList();
    }

    private void RefreshRosterList()
    {
        foreach (Transform child in rosterListParent) Destroy(child.gameObject);

        List<Character_Data> ownedChars = Character_Manager.instance.GetOwnedCharacters();

        foreach (var charData in ownedChars)
        {
            GameObject slot = Instantiate(rosterSlotPrefab, rosterListParent);

            UI_PartySlot slotLogic = slot.GetComponent<UI_PartySlot>();
            if (slotLogic != null) slotLogic.Setup(charData);

            Button btn = slot.GetComponent<Button>();
            btn.onClick.AddListener(() => SelectCharacterInSubPanel(charData));
        }
    }

    private void SelectCharacterInSubPanel(Character_Data data)
    {
        selectedDataForSlot = data;

        // 오른쪽 정보 갱신
        previewNameText.text = data.characterName;
        CharacterStatus status = Character_Manager.instance.GetCharacterStatus(data.characterID);
        previewLevelText.text = $"Lv.{status.level}";
        executeFormationButton.interactable = true;

        // 3D 모델 갱신
        if (currentPreviewModel != null) Destroy(currentPreviewModel);
        currentPreviewModel = Instantiate(data.uiPrefab, previewSpawnPoint.position, previewSpawnPoint.rotation);
        Animator anim = currentPreviewModel.GetComponentInChildren<Animator>();
        if (anim != null) anim.Play("First Idle");
    }

    private void ExecuteFormation()
    {
        if (selectedDataForSlot == null || currentEditingSlotIndex < 0) return;

        // 중복 편성 방지 (다른 자리에 이 캐릭터가 있으면 그 자리를 비움)
        for (int i = 0; i < 3; i++)
        {
            if (tempParty[i] == selectedDataForSlot) tempParty[i] = null;
        }

        tempParty[currentEditingSlotIndex] = selectedDataForSlot;

        subPanel_CharacterSelect.SetActive(false);
        Refresh3DStage();
    }

    public void SaveAndClose()
    {
        List<Character_Data> finalData = new List<Character_Data>();
        List<string> finalIDs = new List<string>();

        for (int i = 0; i < 3; i++)
        {
            if (tempParty[i] != null)
            {
                finalData.Add(tempParty[i]);
                finalIDs.Add(tempParty[i].characterID);
            }
        }

        Character_Manager.instance.SaveParty(finalIDs, finalData);

        if (formationCamera != null) formationCamera.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}