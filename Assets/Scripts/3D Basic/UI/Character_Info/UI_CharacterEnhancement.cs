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

    private Player_Stat targetStat;
    private bool isAutoAdded = false;

    private void Awake()
    {
        if (instance == null) instance = this;

        if (autoToggleBtn != null) autoToggleBtn.onClick.AddListener(OnClickAutoToggle);
        if (enhanceExecuteBtn != null) enhanceExecuteBtn.onClick.AddListener(ExecuteLevelUp);
    }

    public void OpenEnhancement()
    {
        targetStat = Player_Stat.globalInstance != null ? Player_Stat.globalInstance : UI_Manager.instance.playerStat;
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
        int previewLevel = targetStat.level;
        int previewExp = targetStat.exp + totalAddedExp;
        int tempRequiredExp = previewLevel * 100; // Player_Stat의 임시 공식 적용

        while (previewExp >= tempRequiredExp)
        {
            previewExp -= tempRequiredExp;
            previewLevel++;
            tempRequiredExp = previewLevel * 100;
        }

        // 텍스트 출력
        if (totalAddedExp == 0)
        {
            previewLevelText.text = $"Lv. {targetStat.level}";
            previewExpText.text = $"{targetStat.exp} / {targetStat.level * 100}";
        }
        else
        {
            previewLevelText.text = $"Lv. {targetStat.level}  ->  <color=green>{previewLevel}</color>";
            previewExpText.text = $"<color=green>+{totalAddedExp} EXP</color>";
        }

        int hpGain = (previewLevel - targetStat.level) * 10; // 레벨업당 체력 10 증가 공식 (Player_Stat 기준)
        RenderStatSlot(0, "HP", targetStat.maxHP, targetStat.maxHP + hpGain, totalAddedExp > 0);
        RenderStatSlot(1, "공격력", targetStat.attackPower, targetStat.attackPower, totalAddedExp > 0);
        RenderStatSlot(2, "방어력", targetStat.defensePower, targetStat.defensePower, totalAddedExp > 0);

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

            Player_Inventory.instance.CleanUpInventory();
            Player_Inventory.instance.RefreshAllUI();

            UI_Manager.instance.ShowMessage("캐릭터 레벨업 완료!");

            OpenEnhancement();
        }
    }
}