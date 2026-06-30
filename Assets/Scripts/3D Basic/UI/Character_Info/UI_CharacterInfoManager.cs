using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_CharacterInfoManager : MonoBehaviour
{
    public static UI_CharacterInfoManager instance;

    [Header("탭")]
    public GameObject characterTabPanel;
    public GameObject weaponTabPanel;

    [Header("사이드 바 버튼")]
    public Button characterTabButton;
    public Button weaponTabButton;

    [Header("보유 캐릭터 버튼 동적 생성")]
    public Transform characterListParent; 
    public GameObject characterSlotPrefab; 

    public Character_Data currentlySelectedCharacter { get; private set; }
    private int currentTabIndex = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void OnEnable()
    {
        // 최초 진입 시 필드에 나와 있는 액티브 플레이어를 기본 선택으로 설정
        if (UI_Manager.instance != null && UI_Manager.instance.activeCharacterStat != null)
        {
            currentlySelectedCharacter = UI_Manager.instance.activeCharacterStat.characterData;
        }

        // 부모 단에서 우측 초상화 버튼 리스트 일괄 생성
        RefreshCharacterButtons();

        // 기본 캐릭터 탭 오픈
        SelectTab(0);
    }

    public void RefreshCharacterButtons()
    {
        if (characterListParent == null || characterSlotPrefab == null) return;

        foreach (Transform child in characterListParent)
        {
            Destroy(child.gameObject);
        }

        if (Character_Manager.instance == null) return;
        List<Character_Data> ownedCharacters = Character_Manager.instance.GetOwnedCharacters();

        foreach (Character_Data charData in ownedCharacters)
        {
            if (charData == null) continue;

            GameObject slotObj = Instantiate(characterSlotPrefab, characterListParent);

            Image slotImg = null;
            Image[] allImages = slotObj.GetComponentsInChildren<Image>(true);
            foreach (Image img in allImages)
            {
                if (img.gameObject.name == "Portrait")
                {
                    slotImg = img;
                    break;
                }
            }

            if (slotImg == null) slotImg = slotObj.GetComponent<Image>();

            if (slotImg != null && charData.characterPortrait != null)
            {
                slotImg.sprite = charData.characterPortrait;
            }

            Button btn = slotObj.GetComponent<Button>();
            if (btn == null) btn = slotObj.GetComponentInChildren<Button>();

            if (btn != null)
            {
                Character_Data targetData = charData;
                btn.onClick.AddListener(() => SelectCharacter(targetData));
            }
        }
    }

    public void SelectCharacter(Character_Data characterData)
    {
        if (characterData == null) return;
        currentlySelectedCharacter = characterData;

        if (currentTabIndex == 0 && characterTabPanel.activeSelf)
        {
            characterTabPanel.GetComponent<UI_CharacterTab>()?.DisplayCharacterInfo(characterData);
        }
        else if (currentTabIndex == 1 && weaponTabPanel.activeSelf)
        {
            weaponTabPanel.GetComponent<UI_WeaponTab>()?.RefreshTab(characterData);
        }
    }

    public void SelectTab(int tabIndex)
    {
        currentTabIndex = tabIndex;
        characterTabPanel.SetActive(false);
        weaponTabPanel.SetActive(false);

        if (tabIndex == 0)
        {
            characterTabPanel.SetActive(true);
            characterTabPanel.GetComponent<UI_CharacterTab>()?.RefreshTab(currentlySelectedCharacter);
        }
        else if (tabIndex == 1)
        {
            weaponTabPanel.SetActive(true);
            weaponTabPanel.GetComponent<UI_WeaponTab>()?.RefreshTab(currentlySelectedCharacter);
        }
    }
}