using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.PlayerLoop;

public class UI_WeaponEnhancement : MonoBehaviour
{
    public static UI_WeaponEnhancement instance;

    [Header("서브 탭 메뉴")]
    public GameObject enhancementSubPanel;
    public GameObject refinementSubPanel; 
    public Button tabEnhanceButton;
    public Button tabRefineButton;

    [Header("좌측 상단: 스탯 프리뷰")]
    public Text weaponNameText;
    public Text levelText;          // 현재레벨 >> 다음레벨 [MAX]
    public Text expProgressText;    // +경험치(재료 넣은만큼)

    [Header("스탯 슬롯 설정")]
    public GameObject statSlotPrefab;
    public Transform statSlotParent;
    private List<UI_WeaponStatSlot> instantiatedStatSlots = new List<UI_WeaponStatSlot>();

    private struct StatPreviewData
    {
        public string statName;
        public int currentValue;
        public int previewValue;
        public StatPreviewData(string name, int current, int preview)
        {
            statName = name;
            currentValue = current;
            previewValue = preview;
        }
    }

    [Header("좌측 하단: 재료 투입 영역")]
    public Dropdown rarityFilterDropdown; 
    public Button autoAddButton;          
    public ScrollRect materialScrollRect; 
    public Transform materialGridParent; 
    public GameObject materialSlotPrefab;
    public Button enhanceExecuteButton; //강화 버튼


    [Header("우측 영역: 보유 재료 패널")]
    public GameObject myMaterialInventoryPanel; 
    public Transform myMaterialGridParent;
    public GameObject myMaterialSlotPrefab;
    public Transform uiWeaponMountPoint;        

    private ItemHolder targetWeaponHolder;

    private GameObject spawnedWeaponModel;  //3d모델 추적

    private List<ItemHolder> selectedMaterials = new List<ItemHolder>();
    private List<UI_EnhancementMaterialSlot> inputSlots = new List<UI_EnhancementMaterialSlot>();
    private List<UI_EnhancementMaterialSlot> myInventorySlots = new List<UI_EnhancementMaterialSlot>();

    private int previewLevel;
    private int previewExp;

    [SerializeField] private GameObject breakthroughMaterialSlotPrefab;
    [SerializeField] private Text executeButtonText;

    private enum WeaponGrowthMode
    {
        LevelUp,
        Breakthrough,
        MaxLevel
    }

    private WeaponGrowthMode currentGrowthMode;

    private void Awake()
    {
        instance = this;
        tabEnhanceButton.onClick.AddListener(() => SwitchSubTab(true));
        tabRefineButton.onClick.AddListener(() => SwitchSubTab(false));
        autoAddButton.onClick.AddListener(OnClickAutoAdd);
        if (enhanceExecuteButton != null) enhanceExecuteButton.onClick.AddListener(OnClickExecute);

        gameObject.SetActive(false); // 처음엔 꺼둠
    }

    public void OpenEnhancementScreen(ItemHolder weapon)
    {
        if (weapon.weaponLevel < 1) weapon.weaponLevel = 1;
        if (weapon.refinementStage < 1) weapon.refinementStage = 1;

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.OpenUI(gameObject);
        }
        else
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling(); 
        }
        targetWeaponHolder = weapon;
        selectedMaterials.Clear();
        myMaterialInventoryPanel.SetActive(false);

        SwitchSubTab(true);

        SpawnWeaponModel(weapon);
    }

    private void SpawnWeaponModel(ItemHolder weapon)
    {
        if (spawnedWeaponModel != null)
        {
            Destroy(spawnedWeaponModel);
        }

        if (weapon == null || weapon.ItemData == null) return;

        Item_Equipment eqData = weapon.ItemData as Item_Equipment;
        if (eqData == null) return;

        if (eqData.weaponPrefab != null && uiWeaponMountPoint != null)
        {
            spawnedWeaponModel = Instantiate(eqData.weaponPrefab, uiWeaponMountPoint);

            spawnedWeaponModel.transform.localPosition = Vector3.zero;
            spawnedWeaponModel.transform.localRotation = Quaternion.identity;
            spawnedWeaponModel.transform.localScale = Vector3.one;
        } 
    }

    private void InitializeInputSlots()
    {
        foreach (Transform child in materialGridParent) Destroy(child.gameObject);
        inputSlots.Clear();

        for (int i = 0; i < 20; i++)
        {
            GameObject go = Instantiate(materialSlotPrefab, materialGridParent, false);
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localPosition = Vector3.zero;
            UI_EnhancementMaterialSlot slot = go.GetComponent<UI_EnhancementMaterialSlot>();
            slot.Initialize(i, true); // 투입구 슬롯임을 명시
            inputSlots.Add(slot);
        }
    }

    private void SwitchSubTab(bool isEnhance)
    {
        enhancementSubPanel.SetActive(isEnhance);
        refinementSubPanel.SetActive(!isEnhance);
        myMaterialInventoryPanel.SetActive(false);

        if (isEnhance)
        {
            RefreshGrowthMode();
        }
        else
        {
            if (UI_WeaponRefinement.instance != null && targetWeaponHolder != null)
            {
                UI_WeaponRefinement.instance.OpenRefinementScreen(targetWeaponHolder);
            }
        }
    }

    // 재료가 들어올 때마다 가상으로 레벨과 스탯 상승치를 실시간 계산
    public void CalculatePreviewStats()
    {
        if (targetWeaponHolder == null) return;

        Item_Equipment eqData = targetWeaponHolder.ItemData as Item_Equipment;
        int totalAddedExp = 0;

        // 투입된 모든 재료의 경험치 합산
        foreach (var mat in selectedMaterials)
        {
            if (mat.ItemData is Item_Equipment)
            {
                totalAddedExp += GetSingleMaterialExpYield(targetWeaponHolder, mat);
            }
            else
            {
                totalAddedExp += (mat.ItemData.expValue * mat.Quantity);
            }
        }

        previewLevel = targetWeaponHolder.weaponLevel;
        previewExp = targetWeaponHolder.weaponExp + totalAddedExp;
        int maxLevel = targetWeaponHolder.GetMaxLevel();

        // 임시 가상 레벨업 루프
        while (previewLevel < maxLevel)
        {
            int requiredExp = previewLevel * 100; // ItemHolder와 동일한 공식 유지
            if (previewExp >= requiredExp)
            {
                previewExp -= requiredExp;
                previewLevel++;
            }
            else break;
        }

        if (previewLevel >= maxLevel)
        {
            previewLevel = maxLevel;
            previewExp = 0;
        }

        // 스탯 텍스트 갱신
        weaponNameText.text = eqData.itemName;

        if (totalAddedExp == 0)
        {
            levelText.text = (targetWeaponHolder.weaponLevel == maxLevel) ? $"Lv.{targetWeaponHolder.weaponLevel} <color=red>[MAX]</color>" : $"Lv.{targetWeaponHolder.weaponLevel}";
            expProgressText.text = "";
        }
        else
        {
            if (previewLevel >= maxLevel)
                levelText.text = $"Lv.{targetWeaponHolder.weaponLevel} -> <color=yellow>{previewLevel}</color> <color=red>[MAX]</color>";
            else if (previewLevel > targetWeaponHolder.weaponLevel)
                levelText.text = $"Lv.{targetWeaponHolder.weaponLevel} -> <color=green>{previewLevel}</color>";
            else
                levelText.text = $"Lv.{targetWeaponHolder.weaponLevel} -> <color=white>{previewLevel}</color>";

            expProgressText.text = $"+{totalAddedExp} EXP";
        }

        // 공격력 프리뷰 계산
        float currentLevelBonus = (targetWeaponHolder.weaponLevel - 1) * eqData.attackGrowthPerLevel;
        float previewLevelBonus = (previewLevel - 1) * eqData.attackGrowthPerLevel;
        float refineMultiplier = 1f + ((targetWeaponHolder.refinementStage - 1) * eqData.refinementBonusPercent);

        int currentAttack = Mathf.RoundToInt((eqData.attackBonus + currentLevelBonus) * refineMultiplier);
        int previewAttack = Mathf.RoundToInt((eqData.attackBonus + previewLevelBonus) * refineMultiplier);

        List<StatPreviewData> statDisplayList = new List<StatPreviewData>();

        statDisplayList.Add(new StatPreviewData("공격력", currentAttack, previewAttack));
        // 나중에 확장할때
        // statDisplayList.Add(new StatPreviewData("방어력", currentDef, previewDef));

        while (instantiatedStatSlots.Count < statDisplayList.Count)
        {
            GameObject go = Instantiate(statSlotPrefab, statSlotParent, false);

            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localPosition = Vector3.zero;

            UI_WeaponStatSlot slot = go.GetComponent<UI_WeaponStatSlot>();
            instantiatedStatSlots.Add(slot);
        }

        for (int i = 0; i < instantiatedStatSlots.Count; i++)
        {
            if (i < statDisplayList.Count)
            {
                instantiatedStatSlots[i].gameObject.SetActive(true);
                instantiatedStatSlots[i].Setup(
                    statDisplayList[i].statName,
                    statDisplayList[i].currentValue,
                    statDisplayList[i].previewValue,
                    totalAddedExp > 0
                );
            }
            else
            {
                instantiatedStatSlots[i].gameObject.SetActive(false);
            }
        }

        // 투입구 UI 동기화
        RefreshInputSlotsUI();

        if (myMaterialInventoryPanel.activeSelf)
        {
            UpdateInventorySlotsUI();
        }
    }

    private void RefreshInputSlotsUI()
    {
        for (int i = 0; i < inputSlots.Count; i++)
        {
            if (i < selectedMaterials.Count)
                inputSlots[i].Setup(selectedMaterials[i], 0);
            else
                inputSlots[i].Clear();
        }
    }

    // 효율적인 경험치 계산 및 투입
    private void OnClickAutoAdd()
    {
        if (targetWeaponHolder == null || Player_Inventory.instance == null) return;

        selectedMaterials.Clear();
        int maxLevel = targetWeaponHolder.GetMaxLevel();
        if (targetWeaponHolder.weaponLevel >= maxLevel) return;

        // 현재 무기의 만렙까지 필요한 총 경험치 잔여량 계산
        int neededExp = 0;
        int tempLevel = targetWeaponHolder.weaponLevel;
        neededExp += (tempLevel * 100) - targetWeaponHolder.weaponExp;
        tempLevel++;
        while (tempLevel < maxLevel)
        {
            neededExp += (tempLevel * 100);
            tempLevel++;
        }

        int maxRarityFilter = rarityFilterDropdown.value == 0 ? 3 : 4;

        var availableMaterials = Player_Inventory.instance.inventorySlots
             .Where(slot =>
                IsValidWeaponEnhancementMaterial(slot)
                && slot.ItemData.rarity <= maxRarityFilter)
             .OrderBy(slot => slot.ItemData.expValue)
             .ToList();

        int currentAccumulatedExp = 0;

        foreach (var invItem in availableMaterials)
        {
            if (currentAccumulatedExp >= neededExp) break;

            int itemExp = invItem.ItemData.expValue;
            int countNeeded = Mathf.CeilToInt((neededExp - currentAccumulatedExp) / (float)itemExp);
            int actualCountToTake = Mathf.Min(countNeeded, invItem.Quantity);

            if (actualCountToTake <= 0) continue;

            selectedMaterials.Add(new ItemHolder(invItem.ItemData, actualCountToTake));

            currentAccumulatedExp += itemExp * actualCountToTake;

            if (selectedMaterials.Count >= 20) break;
        }

        CalculatePreviewStats();
    }

    // 투입 슬롯에서 X 버튼 눌렀을 때 재료를 차감하는 기능
    public void RemoveMaterialFromInput(ItemHolder item)
    {
        if (item == null) return;

        if (item.ItemData is Item_Equipment)
        {
            selectedMaterials.Remove(item); // 장비는 리스트에서 즉시 제거
        }
        else
        {
            ItemHolder existing = selectedMaterials.Find(m => m.ItemData == item.ItemData);
            if (existing != null)
            {
                existing.Quantity--;
                if (existing.Quantity <= 0)
                {
                    selectedMaterials.Remove(existing);
                }
            }
        }

        CalculatePreviewStats();
    }

    private bool IsValidWeaponEnhancementMaterial(ItemHolder holder)
    {
        if (holder == null || holder.ItemData == null) return false;

        Item_Material material = holder.ItemData as Item_Material;

        if (material == null) return false;

        bool validUsage = material.materialUsage == MaterialUsageType.CommonEnhancement || material.materialUsage == MaterialUsageType.WeaponEnhancement;

        if (!validUsage) return false;

        return material.expValue > 0;
    }

    // 실제 재료를 소모하고 스탯을 증가시키는 강화 실행 로직
    private void ExecuteEnhancement()
    {
        if (targetWeaponHolder == null || selectedMaterials.Count == 0) return;

        int maxLevel = targetWeaponHolder.GetMaxLevel();
        if (targetWeaponHolder.weaponLevel >= maxLevel) return;

        int totalAddedExp = 0;
        int oldAttack = targetWeaponHolder.GetTotalWeaponAttack();

        foreach (var mat in selectedMaterials)
        {
            if (mat.ItemData is Item_Equipment)
            {
                totalAddedExp += GetSingleMaterialExpYield(targetWeaponHolder, mat);
                Player_Inventory.instance.inventorySlots.Remove(mat); // 제물 장비 삭제
            }
            else
            {
                totalAddedExp += (mat.ItemData.expValue * mat.Quantity);
                Player_Inventory.instance.RemoveItem(mat.ItemData, mat.Quantity); // 사용한 수량만큼 광석 삭제
            }
        }

        // 본체 무기에 최종 누적 경험치 주입
        targetWeaponHolder.AddWeaponExp(totalAddedExp);

        int newAttack = targetWeaponHolder.GetTotalWeaponAttack();

        RefreshEquippedWeaponAttack(oldAttack, newAttack);

        // 바구니 비우기 및 인벤토리/UI 완전 갱신
        selectedMaterials.Clear();
        Player_Inventory.instance.CleanUpInventory();
        Player_Inventory.instance.RefreshAllUI();
        RefreshGrowthMode();

        if (myMaterialInventoryPanel.activeSelf)
        {
            RefreshMyInventoryUI();
        }

        UI_WeaponTab weaponTab = FindObjectOfType<UI_WeaponTab>();
        if (weaponTab != null) weaponTab.RefreshTab();
    }

    private void ExecuteBreakthrough()
    {
        if (targetWeaponHolder == null) return;

        Item_Equipment weaponData = targetWeaponHolder.ItemData as Item_Equipment;

        if (weaponData == null) return;

        if (targetWeaponHolder.weaponLevel < targetWeaponHolder.GetMaxLevel()) return;

        int targetStage = targetWeaponHolder.breakthroughStage + 1;

        WeaponBreakthroughRequirement requirement = weaponData.GetBreakthroughRequirement(targetStage);

        if (requirement == null || requirement.materials == null || requirement.materials.Count == 0) return;

        // 먼저 전부 있는지 확인
        foreach (WeaponBreakthroughMaterialRequirement mat in requirement.materials)
        {
            if (mat == null || mat.material == null) return;

            if (GetOwnedItemCount(mat.material) < mat.requiredCount)
            {
                UI_Manager.Instance?.ShowMessage("돌파 재료가 부족합니다.");

                return;
            }
        }

        // 검증 통과 후에만 소비
        foreach (WeaponBreakthroughMaterialRequirement mat in requirement.materials)
        {
            Player_Inventory.instance.RemoveItem(mat.material, mat.requiredCount);
        }

        bool success = targetWeaponHolder.TryBreakthrough();

        if (!success)
        {
            Debug.LogError("무기 돌파 처리에 실패했습니다.");

            return;
        }

        Player_Inventory.instance.CleanUpInventory();
        Player_Inventory.instance.RefreshAllUI();

        UI_Manager.Instance?.ShowMessage($"{weaponData.itemName} 돌파 완료! " + $"레벨 상한이 {targetWeaponHolder.GetMaxLevel()}까지 열렸습니다.");

        RefreshGrowthMode();
    }

    public void OpenMyMaterialInventory()
    {
        myMaterialInventoryPanel.SetActive(true);
        RefreshMyInventoryUI();
    }

    private void UpdateInventorySlotsUI()
    {
        foreach (var slot in myInventorySlots)
        {
            ItemHolder invItem = slot.GetAssignedItem();
            if (invItem != null)
            {
                int placed = 0;
                if (invItem.ItemData.itemType == ITEMTYPE.Equipment)
                {
                    placed = selectedMaterials.Contains(invItem) ? 1 : 0;
                }
                else
                {
                    ItemHolder basketItem = selectedMaterials.Find(m => m.ItemData == invItem.ItemData);
                    if (basketItem != null) placed = basketItem.Quantity;
                }

                slot.Setup(invItem, placed);
            }
        }
    }

    private void RefreshMyInventoryUI()
    {
        foreach (Transform child in myMaterialGridParent) Destroy(child.gameObject);
        myInventorySlots.Clear();

        if (Player_Inventory.instance == null) return;

        var materials = Player_Inventory.instance.inventorySlots
          .Where(IsValidWeaponEnhancementMaterial)
          .OrderBy(slot => slot.ItemData.rarity);

        foreach (var mat in materials)
        {
            GameObject go = Instantiate(myMaterialSlotPrefab, myMaterialGridParent);
            UI_EnhancementMaterialSlot slot = go.GetComponent<UI_EnhancementMaterialSlot>();
            slot.Initialize(-1, false);

            //무기일때는 주소비교, 광석일 때는 설계도 비교
            int placed = 0;
            if (mat.ItemData.itemType == ITEMTYPE.Equipment)
            {
                placed = selectedMaterials.Contains(mat) ? 1 : 0;
            }
            else
            {
                ItemHolder baseketItem = selectedMaterials.Find(m => m.ItemData == mat.ItemData);
                if (baseketItem != null) placed = baseketItem.Quantity;
            }

            slot.Setup(mat);
            myInventorySlots.Add(slot);
        }
    }

    private void RefreshEquippedWeaponAttack(int oldAttack, int newAttack)
    {
        if (BattleManager.instance == null) return;

        GameObject activePlayer = BattleManager.instance.GetActiveCharacter();

        if (activePlayer == null) return;

        Player_Equipment equipment = activePlayer.GetComponent<Player_Equipment>();

        Character_Stat stat = activePlayer.GetComponent<Character_Stat>();

        if (equipment == null || stat == null) return;
        if (equipment.equipmentSlots.Length == 0) return;
        if (equipment.equipmentSlots[0] != targetWeaponHolder) return;

        stat.RemoveEquipmentStat(STAT.Attack, oldAttack);

        stat.AddEquipmentStat(STAT.Attack, newAttack);
    }

    //보유량 확인
    private int GetOwnedItemCount(Item_Base targetItem)
    {
        if (targetItem == null || Player_Inventory.instance == null) return 0;

        int totalCount = 0;

        foreach (ItemHolder holder in Player_Inventory.instance.inventorySlots)
        {
            if (holder == null || holder.ItemData == null) continue;

            bool sameReference = holder.ItemData == targetItem;

            bool sameItemID = !string.IsNullOrEmpty(targetItem.itemID) && holder.ItemData.itemID == targetItem.itemID;

            if (sameReference || sameItemID)
            {
                totalCount += holder.Quantity;
            }
        }

        return totalCount;
    }

    public void AddMaterialFromInventory(ItemHolder clickedItem)
    {
        if (!IsValidWeaponEnhancementMaterial(clickedItem)) return;
        if (targetWeaponHolder != null && previewLevel >= targetWeaponHolder.GetMaxLevel()) return;

        ItemHolder existing = selectedMaterials.Find(m => m.ItemData == clickedItem.ItemData);

        if (existing != null)
        {
            if (existing.Quantity < clickedItem.Quantity)
            {
                existing.Quantity++;
            }
        }
        else
        {
            if (selectedMaterials.Count >= 20) return;

            selectedMaterials.Add(new ItemHolder(clickedItem.ItemData, 1));
        }

        CalculatePreviewStats();
    }

    public void RefreshEnhancementUI()
    {
        CalculatePreviewStats();
    }

    private void RefreshGrowthMode()
    {
        if (targetWeaponHolder == null) return;

        int currentCap = targetWeaponHolder.GetMaxLevel();

        if (targetWeaponHolder.weaponLevel < currentCap)
        {
            currentGrowthMode = WeaponGrowthMode.LevelUp;

            ShowLevelUpMode();
        }
        else if (targetWeaponHolder.breakthroughStage < 2)
        {
            currentGrowthMode = WeaponGrowthMode.Breakthrough;

            ShowBreakthroughMode();
        }
        else
        {
            currentGrowthMode = WeaponGrowthMode.MaxLevel;

            ShowMaxLevelMode();
        }
    }

    private void RefreshCurrentWeaponStat()
    {
        int currentAttack = targetWeaponHolder.GetTotalWeaponAttack();

        while (instantiatedStatSlots.Count < 1)
        {
            GameObject go = Instantiate(statSlotPrefab, statSlotParent, false);

            UI_WeaponStatSlot slot = go.GetComponent<UI_WeaponStatSlot>();

            instantiatedStatSlots.Add(slot);
        }

        instantiatedStatSlots[0].gameObject.SetActive(true);

        instantiatedStatSlots[0].Setup("공격력", currentAttack, currentAttack, false);

        for (int i = 1; i < instantiatedStatSlots.Count; i++)
        {
            instantiatedStatSlots[i].gameObject.SetActive(false);
        }
    }

    private void RefreshBreakthroughMaterialGrid()
    {
        Item_Equipment weaponData = targetWeaponHolder.ItemData as Item_Equipment;

        if (weaponData == null) return;

        int targetStage = targetWeaponHolder.breakthroughStage + 1;

        WeaponBreakthroughRequirement requirement = weaponData.GetBreakthroughRequirement(targetStage);

        if (requirement == null || requirement.materials == null || requirement.materials.Count == 0)
        {
            enhanceExecuteButton.interactable = false;

            Debug.LogWarning($"{weaponData.itemName}의 " + $"{targetStage}단계 돌파 재료가 없습니다.");

            return;
        }

        bool hasAllMaterials = true;

        foreach (WeaponBreakthroughMaterialRequirement materialRequirement in requirement.materials)
        {
            if (materialRequirement == null || materialRequirement.material == null)
            {
                hasAllMaterials = false;
                continue;
            }

            int ownedCount = GetOwnedItemCount(materialRequirement.material);

            GameObject slotObject = Instantiate(breakthroughMaterialSlotPrefab, materialGridParent);

            UI_AscensionMaterialSlot slot = slotObject.GetComponent<UI_AscensionMaterialSlot>();

            if (slot != null)
            {
                slot.Setup(materialRequirement.material, ownedCount, materialRequirement.requiredCount);
            }

            if (ownedCount < materialRequirement.requiredCount)
            {
                hasAllMaterials = false;
            }
        }

        enhanceExecuteButton.interactable = hasAllMaterials;
    }

    // 재료가 장비일 경우 페이백 경험치를 포함한 최종 제공 경험치를 계산
    private int GetSingleMaterialExpYield(ItemHolder targetWeapon, ItemHolder materialSlot)
    {
        int baseExp = materialSlot.ItemData.expValue; // 명함 상태의 기본 재료 경험치

        if (materialSlot.ItemData is Item_Equipment matEq)
        {
            // 강화할 타겟 무기와 재료 무기의 등급(Rarity)이 같다면?
            if (matEq.rarity == targetWeapon.ItemData.rarity)
            {
                int accumulated = materialSlot.GetTotalAccumulatedExp();
                // 기본 경험치 + 누적 경험치의 90% 반환
                return baseExp + Mathf.RoundToInt(accumulated * 0.9f);
            }
        }
        return baseExp;
    }

    private void ShowLevelUpMode()
    {
        selectedMaterials.Clear();

        if (myMaterialInventoryPanel != null) myMaterialInventoryPanel.SetActive(false);
        if (autoAddButton != null) autoAddButton.gameObject.SetActive(true);
        if (rarityFilterDropdown != null) rarityFilterDropdown.gameObject.SetActive(true);
        if (executeButtonText != null) executeButtonText.text = "강화";

        InitializeInputSlots();
        RefreshEnhancementUI();
    }

    private void ShowBreakthroughMode()
    {
        selectedMaterials.Clear();
        ClearMaterialGrid();

        if (myMaterialInventoryPanel != null) myMaterialInventoryPanel.SetActive(false);
        if (autoAddButton != null) autoAddButton.gameObject.SetActive(false);
        if (rarityFilterDropdown != null) rarityFilterDropdown.gameObject.SetActive(false);
        if (executeButtonText != null) executeButtonText.text = "돌파";

        Item_Equipment weaponData = targetWeaponHolder.ItemData as Item_Equipment;

        if (weaponData == null) return;

        int currentCap = targetWeaponHolder.GetMaxLevel();

        int nextCap = targetWeaponHolder.breakthroughStage == 0 ? 40 : 50;

        weaponNameText.text = weaponData.itemName;

        levelText.text = $"Lv.{targetWeaponHolder.weaponLevel} / {currentCap}" + $"  →  <color=green>상한 Lv.{nextCap}</color>";

        expProgressText.text = $"돌파 단계 {targetWeaponHolder.breakthroughStage}" + $" → <color=green>{targetWeaponHolder.breakthroughStage + 1}</color>";

        RefreshCurrentWeaponStat();
        RefreshBreakthroughMaterialGrid();
    }

    private void ShowMaxLevelMode()
    {
        selectedMaterials.Clear();
        ClearMaterialGrid();

        if (myMaterialInventoryPanel != null) myMaterialInventoryPanel.SetActive(false);
        if (autoAddButton != null) autoAddButton.gameObject.SetActive(false);
        if (rarityFilterDropdown != null) rarityFilterDropdown.gameObject.SetActive(false);
        if (executeButtonText != null) executeButtonText.text = "최대";

        weaponNameText.text = targetWeaponHolder.ItemData.itemName;

        levelText.text = $"Lv.{targetWeaponHolder.weaponLevel} [MAX]";

        expProgressText.text = "최종 성장 완료";

        enhanceExecuteButton.interactable = false;

        RefreshCurrentWeaponStat();
    }

    private void ClearMaterialGrid()
    {
        foreach (Transform child in materialGridParent)
        {
            Destroy(child.gameObject);
        }

        inputSlots.Clear();
    }

    private void OnClickExecute()
    {
        switch (currentGrowthMode)
        {
            case WeaponGrowthMode.LevelUp:
                ExecuteEnhancement();
                break;

            case WeaponGrowthMode.Breakthrough:
                ExecuteBreakthrough();
                break;
        }
    }
}