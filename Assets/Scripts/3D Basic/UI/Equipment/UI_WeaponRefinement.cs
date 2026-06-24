using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UI_WeaponRefinement : MonoBehaviour
{
    public static UI_WeaponRefinement instance;

    [Header("공통 데이터")]
    private ItemHolder targetWeaponHolder;
    private List<ItemHolder> selectedMaterials = new List<ItemHolder>();
    private int maxRefinementStage = 5;

    [Header("재료 가방")]
    public Transform materialGridParent;
    public GameObject materialSlotPrefab;

    [Header("아이템 상세 정보")]
    public Image centerWeaponIcon;
    public Text centerWeaponName;
    public Text centerWeaponLevel;
    public Text centerWeaponCurrentStat;
    public Text centerWeaponCurrentPassive;

    [Header("합성")]
    public Text rightWeaponStep;
    public Text rightWeaponName;
    public Text rightWeaponNextPassive;
    public List<UI_EnhancementMaterialSlot> inputSlots;
    public Button refineButton;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < inputSlots.Count; i++)
        {
            if (inputSlots[i] != null)
            {
                inputSlots[i].Initialize(1, true);
            }
        }

        if (refineButton != null)
        {
            refineButton.onClick.RemoveAllListeners();
            refineButton.onClick.AddListener(ExecuteRefinement);
        }
    }

    public void OpenRefinementScreen(ItemHolder weapon)
    {
        targetWeaponHolder = weapon;
        selectedMaterials.Clear();

        RefreshAllUI();
    }

    private void RefreshAllUI()
    {
        RefreshLeftInventory();
        RefreshCenterPanel();
        RefreshRightPanel();
    }

    private void RefreshLeftInventory()
    {
        foreach (Transform child in materialGridParent) Destroy(child.gameObject);

        var dupes = Player_Inventory.instance.inventorySlots
            .Where(slot => slot != null
                        && slot.ItemData == targetWeaponHolder.ItemData
                        && slot != targetWeaponHolder)
            .ToList();

        foreach (var dupe in dupes)
        {
            GameObject go = Instantiate(materialSlotPrefab, materialGridParent);
            UI_EnhancementMaterialSlot slot = go.GetComponent<UI_EnhancementMaterialSlot>();
            slot.Initialize(-1, false);
            int placed = selectedMaterials.Contains(dupe) ? 1 : 0;
            slot.Setup(dupe, placed);
        }
    }

    private void RefreshCenterPanel()
    {
        if (targetWeaponHolder == null || targetWeaponHolder.ItemData == null) return;
        Item_Equipment eqData = targetWeaponHolder.ItemData as Item_Equipment;

        centerWeaponIcon.sprite = eqData.itemIcon;
        centerWeaponName.text = eqData.itemName;
        centerWeaponLevel.text = $"Lv.{targetWeaponHolder.weaponLevel}";

        centerWeaponCurrentStat.text = $"공격력 : {targetWeaponHolder.GetTotalWeaponAttack()}";
        //패시브는 임시.. 나중에 데이터 구조에 맞게 변경
        centerWeaponCurrentPassive.text = $"[재련 {targetWeaponHolder.refinementStage}단계]\n패시브 능력치 설명...";
    }

    private void RefreshRightPanel()
    {
        if (targetWeaponHolder == null || targetWeaponHolder.ItemData == null) return;
        Item_Equipment eqData = targetWeaponHolder.ItemData as Item_Equipment;

        int currentStep = targetWeaponHolder.refinementStage;
        int nextStep = currentStep + selectedMaterials.Count;

        if (rightWeaponName != null) rightWeaponName.text = eqData.itemName;

        if (selectedMaterials.Count > 0)
        {
            if (rightWeaponStep != null)
                rightWeaponStep.text = $"{currentStep}단계  ->  <color=green>{nextStep}단계</color>";

            float currentLevelBonus = (targetWeaponHolder.weaponLevel - 1) * eqData.attackGrowthPerLevel;
            float nextRefineMultiplier = 1f + ((nextStep - 1) * eqData.refinementBonusPercent);
            int previewAttack = Mathf.RoundToInt((eqData.attackBonus + currentLevelBonus) * nextRefineMultiplier);

            if (rightWeaponNextPassive != null)
                rightWeaponNextPassive.text = $"\n공격력 : {targetWeaponHolder.GetTotalWeaponAttack()}  ->  <color=green>{previewAttack}</color>";
        }
        else
        {
            if (rightWeaponStep != null)
                rightWeaponStep.text = $"{currentStep}단계 (최고 단계: {maxRefinementStage})";
            if (rightWeaponNextPassive != null)
                rightWeaponNextPassive.text = "왼쪽의 동일한 재료 무기를 클릭하여 합성 결과를 확인하세요.";
        }

        for (int i = 0; i < inputSlots.Count; i++)
        {
            if (i < selectedMaterials.Count)
                inputSlots[i].Setup(selectedMaterials[i], 1);
            else
                inputSlots[i].Clear();
        }

        refineButton.interactable = (selectedMaterials.Count > 0 && nextStep <= maxRefinementStage);
    }

    public void AddMaterialToSlot(ItemHolder dupeWeapon)
    {
        int currentStage = targetWeaponHolder.refinementStage;
        int maxAddable = maxRefinementStage - currentStage;

        if (selectedMaterials.Count >= maxAddable) return;
        if (selectedMaterials.Contains(dupeWeapon)) return;

        selectedMaterials.Add(dupeWeapon);
        RefreshAllUI();
    }

    public void ExecuteRefinement()
    {
        if (selectedMaterials.Count == 0) return;

        int oldAttack = targetWeaponHolder.GetTotalWeaponAttack();

        foreach (var mat in selectedMaterials)
        {
            Player_Inventory.instance.inventorySlots.Remove(mat);
        }

        targetWeaponHolder.refinementStage += selectedMaterials.Count;
        int newAttack = targetWeaponHolder.GetTotalWeaponAttack();

        Player_Equipment activeEquip = null;
        if (BattleManager.instance != null)
        {
            GameObject activePlayer = BattleManager.instance.GetActiveCharacter();
            if (activePlayer != null) activeEquip = activePlayer.GetComponent<Player_Equipment>();
        }

        if (activeEquip != null && activeEquip.equipmentSlots[0] == targetWeaponHolder)
        {
            Character_Stat stat = activeEquip.GetComponent<Character_Stat>();
            if (stat != null)
            {
                stat.RemoveEquipmentStat(STAT.Attack, oldAttack);
                stat.AddEquipmentStat(STAT.Attack, newAttack);
            }
        }

        selectedMaterials.Clear();
        Player_Inventory.instance.CleanUpInventory();
        Player_Inventory.instance.RefreshAllUI();

        RefreshAllUI();

        if (UI_Manager.instance != null) UI_Manager.instance.ShowMessage("장비 재련 합성 완료!");

        UI_WeaponTab weaponTab = FindObjectOfType<UI_WeaponTab>();
        if (weaponTab != null) weaponTab.RefreshTab();
    }

    public void RemoveMaterialFromInput(ItemHolder item)
    {
        if (item == null) return;

        if (selectedMaterials.Contains(item))
        {
            selectedMaterials.Remove(item);
        }

        RefreshAllUI();
    }
}
