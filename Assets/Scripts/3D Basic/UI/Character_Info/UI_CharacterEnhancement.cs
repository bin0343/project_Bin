using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UI_CharacterEnhancement : MonoBehaviour
{
    public static UI_CharacterEnhancement instance;

    [Header("스탯 시뮬레이션 UI")]
    public Text previewLevelText;
    public Text previewExpText;
    public Transform statSlotParent;
    public GameObject statSlotPrefab; 
    private List<UI_WeaponStatSlot> instantiatedStatSlots = new List<UI_WeaponStatSlot>();

    [Header("재료 투입 UI")]
    public Transform materialGridParent;
    public GameObject expMaterialSlotPrefab; 
    private List<UI_ExpMaterialSlot> activeSlots = new List<UI_ExpMaterialSlot>();

    [Header("조작 버튼")]
    public Button autoToggleBtn;
    public Text autoToggleBtnText;
    public Button enhanceExecuteBtn;

    private Character_Stat targetStat;
    private bool isAutoAdded = false;

    private void Awake()
    {
        if (instance == null) instance = this;

        if (autoToggleBtn != null) autoToggleBtn.onClick.AddListener(OnClickAutoToggle);
        if (enhanceExecuteBtn != null) enhanceExecuteBtn.onClick.AddListener(ExecuteLevelUp);
    }

    public void OpenEnhancement()
    {
        targetStat = UI_Manager.instance.activeCharacterStat;
        isAutoAdded = false;

        RefreshMaterialInventory();
        CalculatePreview();
    }

    private void RefreshMaterialInventory()
    {
        foreach (Transform child in materialGridParent) Destroy(child.gameObject);
        activeSlots.Clear();

        if (Player_Inventory.instance == null) return;

        var expBooks = Player_Inventory.instance.inventorySlots
            .Where(slot => slot != null && slot.ItemData != null && slot.ItemData.expValue > 0)
            .OrderBy(slot => slot.ItemData.rarity) 
            .ToList();

        foreach (var book in expBooks)
        {
            GameObject go = Instantiate(expMaterialSlotPrefab, materialGridParent);
            UI_ExpMaterialSlot slot = go.GetComponent<UI_ExpMaterialSlot>();
            slot.Setup(book, 0);
            activeSlots.Add(slot);
        }
    }

    public void CalculatePreview()
    {
        if (targetStat == null) return;

        CharacterStatus cStatus = Character_Manager.instance.GetCharacterStatus(targetStat.characterData.characterID);

        int totalAddedExp = 0;
        int currentUsedBooks = 0;

        foreach (var slot in activeSlots)
        {
            totalAddedExp += slot.GetExpYield();
            currentUsedBooks += slot.usedCount;
        }

        isAutoAdded = currentUsedBooks > 0;
        if (autoToggleBtnText != null)
        {
            autoToggleBtnText.text = isAutoAdded ? "모두 취소" : "일괄 추가";
        }

        // 가상 레벨업 계산
        int previewLevel = cStatus.level;
        int previewExp = cStatus.currentExp + totalAddedExp;
        int currentRequiredExp = cStatus.maxExp; // Player_Stat의 임시 공식 적용

        while (previewExp >= currentRequiredExp)
        {
            previewExp -= currentRequiredExp;
            previewLevel++;
            currentRequiredExp *= 2;
        }

        // 텍스트 출력
        if (totalAddedExp == 0)
        {
            previewLevelText.text = previewLevel > cStatus.level ? $"Lv. {cStatus.level}  ->  <color=green>{previewLevel}</color>" : $"Lv. {cStatus.level}";
            previewExpText.text = previewExp + " / " + currentRequiredExp;
        }
        else
        {
            previewLevelText.text = $"Lv. {cStatus.level}  ->  <color=green>{previewLevel}</color>";
            previewExpText.text = $"{cStatus.currentExp} <color=green>+{totalAddedExp}</color> / {cStatus.maxExp}";
        }

        int leveledCount = previewLevel - cStatus.level;

        Character_Data data = targetStat.characterData;
        int hpGain = leveledCount * (int)data.levelUpStatGains[(int)STAT.HP];
        int attackGain = leveledCount * (int)data.levelUpStatGains[(int)STAT.Attack];
        int defenseGain = leveledCount * (int)data.levelUpStatGains[(int)STAT.Defense];

        RenderStatSlot(0, "HP", targetStat.maxHP, targetStat.maxHP + hpGain, totalAddedExp > 0);
        RenderStatSlot(1, "공격력", targetStat.attackPower, targetStat.attackPower + attackGain, totalAddedExp > 0);
        RenderStatSlot(2, "방어력", targetStat.defensePower, targetStat.defensePower + defenseGain, totalAddedExp > 0);

        enhanceExecuteBtn.interactable = totalAddedExp > 0;
    }

    private void RenderStatSlot(int index, string statName, int currentVal, int previewVal, bool isPreviewing)
    {
        if (index >= instantiatedStatSlots.Count)
        {
            GameObject go = Instantiate(statSlotPrefab, statSlotParent);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            instantiatedStatSlots.Add(go.GetComponent<UI_WeaponStatSlot>());
        }

        instantiatedStatSlots[index].gameObject.SetActive(true);
        bool showArrow = isPreviewing && (previewVal > currentVal);
        instantiatedStatSlots[index].Setup(statName, currentVal, previewVal, showArrow);
    }

    private void OnClickAutoToggle()
    {
        if (isAutoAdded)
        {
            foreach (var slot in activeSlots) slot.SetUsedCount(0);
        }
        else 
        {
            foreach (var slot in activeSlots)
            {
                slot.SetUsedCount(slot.GetItem().Quantity);
            }
        }
        CalculatePreview();
    }

    private void ExecuteLevelUp()
    {
        int totalAddedExp = 0;

        foreach (var slot in activeSlots)
        {
            if (slot.usedCount > 0)
            {
                totalAddedExp += slot.GetExpYield();
                Player_Inventory.instance.RemoveItem(slot.GetItem().ItemData, slot.usedCount);
            }
        }

        if (totalAddedExp > 0)
        {
            targetStat.GainExp(totalAddedExp);

            targetStat.RefreshStatsFromManager();

            Player_Inventory.instance.CleanUpInventory();
            Player_Inventory.instance.RefreshAllUI();

            if (UI_Manager.instance != null)
            {
                UI_Manager.instance.UpdatePlayerStatus(targetStat);
            }

            UI_Manager.instance.ShowMessage("캐릭터 레벨업 완료!");

            OpenEnhancement();
        }
    }
}