using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UI_CharacterEnhancement : MonoBehaviour
{
    public static UI_CharacterEnhancement Instance;

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

    [Header("돌파 재료 UI")]
    [SerializeField] private GameObject ascensionMaterialSlotPrefab;
    [SerializeField] private Text executeButtonText;

    private EnhancementMode currentMode;

    private Character_Data targetData;
    private CharacterStatus targetStatus;

    // 월드에 실제 생성된 캐릭터가 있을 때만 사용
    private Character_Stat targetStat;
    private bool isAutoAdded = false;

    private enum EnhancementMode
    {
        LevelUp,
        Ascension,
        MaxLevel
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        if (autoToggleBtn != null) autoToggleBtn.onClick.AddListener(OnClickAutoToggle);
        if (enhanceExecuteBtn != null) enhanceExecuteBtn.onClick.AddListener(OnClickExecute);
    }

    private void OnClickExecute()
    {
        switch (currentMode)
        {
            case EnhancementMode.LevelUp:
                ExecuteLevelUp();
                break;

            case EnhancementMode.Ascension:
                ExecuteAscension();
                break;
        }
    }

    public void OpenEnhancement(Character_Data selectedCharacter)
    {
        if (selectedCharacter == null) return;

        targetData = selectedCharacter;

        targetStatus = Character_Manager.Instance.GetCharacterStatus(targetData.characterID, targetData);

        targetStat = FindSpawnedCharacterStat(targetData);

        isAutoAdded = false;

        RefreshEnhancementMode();
    }

    private Character_Stat FindSpawnedCharacterStat(Character_Data data)
    {
        if (data == null) return null;

        if (BattleManager.Instance == null || BattleManager.Instance.SpawnedCharacters == null) return null;

        foreach (GameObject characterObject in BattleManager.Instance.SpawnedCharacters)
        {
            if (characterObject == null) continue;

            Character_Stat stat = characterObject.GetComponent<Character_Stat>();

            if (stat == null || stat.characterData == null) continue;

            if (stat.characterData.characterID == data.characterID) return stat;
        }

        return null;
    }

    private void RefreshMaterialInventory()
    {
        if (Player_Inventory.Instance == null) return;

        var expBooks = Player_Inventory.Instance.inventorySlots
            .Where(slot => slot != null && slot.ItemData != null && slot.ItemData.expValue > 0)
            .OrderBy(slot => slot.ItemData.rarity) 
            .ToList();

        foreach (var book in expBooks)
        {
            GameObject go = Instantiate(expMaterialSlotPrefab, materialGridParent);
            UI_ExpMaterialSlot slot = go.GetComponent<UI_ExpMaterialSlot>();
            if (slot == null)
            {
                Destroy(go);
                continue;
            }
            slot.Setup(book, 0);
            activeSlots.Add(slot);
        }
    }

    private void RefreshEnhancementMode()
    {
        ClearMaterialGrid();

        int currentLevelCap = Character_Manager.Instance.GetCurrentLevelCap(targetStatus);

        if (!Character_Manager.Instance.IsAtCurrentLevelCap(targetStatus))
        {
            currentMode = EnhancementMode.LevelUp;
            ShowLevelUpMode();
        }
        else if (Character_Manager.Instance.CanAscend(targetStatus))
        {
            currentMode = EnhancementMode.Ascension;
            ShowAscensionMode();
        }
        else
        {
            currentMode = EnhancementMode.MaxLevel;
            ShowMaxLevelMode();
        }
    }

    private void ClearMaterialGrid()
    {
        foreach (Transform child in materialGridParent)
        {
            Destroy(child.gameObject);
        }

        activeSlots.Clear();
    }

    private void ShowLevelUpMode()
    {
        if (autoToggleBtn != null)
        {
            autoToggleBtn.gameObject.SetActive(true);
        }

        if (executeButtonText != null)
        {
            executeButtonText.text = "강화";
        }

        RefreshMaterialInventory();
        CalculatePreview();
    }

    private void ShowAscensionMode()
    {
        if (autoToggleBtn != null)
        {
            autoToggleBtn.gameObject.SetActive(false);
        }

        if (executeButtonText != null)
        {
            executeButtonText.text = "돌파";
        }

        int currentCap =Character_Manager.Instance.GetCurrentLevelCap(targetStatus);

        int nextCap =Character_Manager.Instance.GetNextLevelCap(targetStatus);

        previewLevelText.text = $"Lv. {targetStatus.level} / {currentCap}  →  " + $"<color=green>상한 Lv. {nextCap}</color>";

        previewExpText.text = $"돌파 단계 {targetStatus.ascensionStage}  →  " + $"<color=green>{targetStatus.ascensionStage + 1}</color>";

        RefreshCurrentStatSlots();
        RefreshAscensionMaterialGrid();
    }

    private void RefreshAscensionMaterialGrid()
    {
        int targetStage = targetStatus.ascensionStage + 1;

        CharacterAscensionRequirement requirement = targetData.GetAscensionRequirement(targetStage);

        if (requirement == null)
        {
            enhanceExecuteBtn.interactable = false;

            Debug.LogWarning($"{targetData.characterName}의 " + $"돌파 {targetStage}단계 재료가 설정되지 않았습니다.");

            return;
        }

        if (requirement.materials == null || requirement.materials.Count == 0)
        {
            enhanceExecuteBtn.interactable = false;

            Debug.LogWarning($"{targetData.characterName}의 " + $"돌파 {targetStage}단계에 필요한 재료가 없습니다.");

            return;
        }

        bool hasAllMaterials = true;

        foreach (AscensionMaterialRequirement materialRequirement in requirement.materials)
        {
            if (materialRequirement == null || materialRequirement.material == null)
            {
                hasAllMaterials = false;
                continue;
            }

            int ownedCount = GetOwnedItemCount(materialRequirement.material);

            GameObject slotObject = Instantiate(ascensionMaterialSlotPrefab, materialGridParent);

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

        enhanceExecuteBtn.interactable = hasAllMaterials;
    }

    private int GetOwnedItemCount(Item_Base targetItem)
    {
        if (targetItem == null || Player_Inventory.Instance == null) return 0;

        int totalCount = 0;

        foreach (ItemHolder holder in Player_Inventory.Instance.inventorySlots)
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

    private void ExecuteAscension()
    {
        if (targetData == null || targetStatus == null) return;

        if (!Character_Manager.Instance.CanAscend(targetStatus)) return;

        int targetStage = targetStatus.ascensionStage + 1;

        CharacterAscensionRequirement requirement = targetData.GetAscensionRequirement(targetStage);

        if (requirement == null || requirement.materials == null || requirement.materials.Count == 0)
        {
            if (UI_Manager.Instance != null)
            {
                UI_Manager.Instance.ShowMessage("돌파 재료 설정이 없습니다.");
            }

            return;
        }

        // 먼저 전체 재료가 있는지 검사
        foreach (AscensionMaterialRequirement materialRequirement in requirement.materials)
        {
            if (materialRequirement == null || materialRequirement.material == null) return;

            int ownedCount = GetOwnedItemCount(materialRequirement.material);

            if (ownedCount < materialRequirement.requiredCount)
            {
                UI_Manager.Instance.ShowMessage("돌파 재료가 부족합니다.");

                return;
            }
        }

        // 검사를 모두 통과한 뒤에만 재료 소비
        foreach (AscensionMaterialRequirement materialRequirement in requirement.materials)
        {
            Player_Inventory.Instance.RemoveItem(materialRequirement.material, materialRequirement.requiredCount);
        }

        bool ascended = Character_Manager.Instance.AscendCharacter(targetData.characterID, targetData);

        if (!ascended)
        {
            Debug.LogError("재료를 소비했지만 돌파 처리에 실패했습니다.");

            return;
        }

        // 해당 캐릭터가 월드에 나와 있다면 즉시 갱신
        if (targetStat != null)
        {
            targetStat.RefreshStatsFromManager();
        }

        Player_Inventory.Instance.CleanUpInventory();
        Player_Inventory.Instance.RefreshAllUI();

        UI_Manager.Instance.ShowMessage($"{targetData.characterName} 돌파 완료! " + $"레벨 상한이 " + $"{Character_Manager.Instance.GetCurrentLevelCap(targetStatus)}" + $"까지 열렸습니다.");

        // 돌파 후 다시 열면 자동으로 레벨업 모드가 됩니다.
        OpenEnhancement(targetData);
    }

    private void ShowMaxLevelMode()
    {
        if (autoToggleBtn != null)
        {
            autoToggleBtn.gameObject.SetActive(false);
        }

        if (executeButtonText != null)
        {
            executeButtonText.text = "최대";
        }

        int levelCap = Character_Manager.Instance.GetCurrentLevelCap(targetStatus);

        previewLevelText.text = $"Lv. {targetStatus.level} / {levelCap}";

        previewExpText.text = "최종 성장 완료";

        RefreshCurrentStatSlots();
        enhanceExecuteBtn.interactable = false;
    }

    public void CalculateLevelUpPreview()
    {
        if (targetData == null || targetStatus == null) return;

        CharacterStatus cStatus = targetStatus;

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
        Character_Manager.Instance.SimulateExperience(cStatus, totalAddedExp, out int previewLevel, out int previewExp, out int previewRequiredExp, out int appliedExp);

        int levelCap = Character_Manager.Instance.GetCurrentLevelCap(cStatus);

        int cumulativePreviewExp;
        int cumulativeRequiredExp;

        if (previewLevel >= levelCap)
        {
            cumulativeRequiredExp = (levelCap - 1) * previewRequiredExp;

            cumulativePreviewExp = cumulativeRequiredExp;
        }
        else
        {
            cumulativePreviewExp = (previewLevel - 1) * previewRequiredExp + previewExp;

            cumulativeRequiredExp = previewLevel * previewRequiredExp;
        }

        if (totalAddedExp == 0)
        {
            previewLevelText.text = $"Lv. {cStatus.level} / {levelCap}";

            previewExpText.text = $"{cumulativePreviewExp} / " + $"{cumulativeRequiredExp}";
        }
        else
        {
            previewLevelText.text = $"Lv. {cStatus.level}  ->  " + $"<color=green>Lv. {previewLevel}</color> " + $"/ {levelCap}";

            previewExpText.text = $"<color=green>{cumulativePreviewExp}</color> / " + $"{cumulativeRequiredExp}";
        }

        int leveledCount = previewLevel - cStatus.level;

        Character_Data data = targetData;
        int hpGain = leveledCount * (int)data.levelUpStatGains[(int)STAT.HP];
        int attackGain = leveledCount * (int)data.levelUpStatGains[(int)STAT.Attack];
        int defenseGain = leveledCount * (int)data.levelUpStatGains[(int)STAT.Defense];

        bool hasValidPreview = appliedExp > 0;

        int currentHP = targetStat != null ? targetStat.maxHP : (int)targetStatus.currentStats[(int)STAT.HP];

        int currentAttack = targetStat != null ? targetStat.attackPower : (int)targetStatus.currentStats[(int)STAT.Attack];

        int currentDefense = targetStat != null ? targetStat.defensePower : (int)targetStatus.currentStats[(int)STAT.Defense];

        RenderStatSlot(0, "HP", currentHP, currentHP + hpGain, hasValidPreview);
        RenderStatSlot(1, "공격력", currentAttack, currentAttack + attackGain, hasValidPreview);
        RenderStatSlot(2, "방어력", currentDefense, currentDefense + defenseGain, hasValidPreview);

        enhanceExecuteBtn.interactable = appliedExp > 0;
    }

    public void CalculatePreview()
    {
        if (currentMode ==
            EnhancementMode.LevelUp)
        {
            CalculateLevelUpPreview();
        }
    }

    private void RefreshCurrentStatSlots()
    {
        if (targetStatus == null) return;

        int currentHP = targetStat != null ? targetStat.maxHP : (int)targetStatus.currentStats[(int)STAT.HP];
        int currentAttack = targetStat != null ? targetStat.attackPower : (int)targetStatus.currentStats[(int)STAT.Attack];
        int currentDefense = targetStat != null ? targetStat.defensePower : (int)targetStatus.currentStats[(int)STAT.Defense];

        RenderStatSlot(0, "HP", currentHP, currentHP, false);
        RenderStatSlot(1, "공격력", currentAttack, currentAttack, false);
        RenderStatSlot(2, "방어력", currentDefense, currentDefense, false);
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

    private void AutoAddRequiredMaterials()
    {
        if (targetData == null || targetStatus == null) return;

        CharacterStatus status = targetStatus;

        int remainingRequiredExp = Character_Manager.Instance.GetRequiredExpToCurrentCap(status);

        // 기존 선택 초기화
        foreach (UI_ExpMaterialSlot slot in activeSlots)
        {
            slot.SetUsedCount(0);
        }

        if (remainingRequiredExp <= 0) return;

        foreach (UI_ExpMaterialSlot slot in activeSlots)
        {
            ItemHolder holder = slot.GetItem();

            if (holder == null || holder.ItemData == null || holder.ItemData.expValue <= 0) continue;

            int expPerItem = holder.ItemData.expValue;
            int availableCount = holder.Quantity;

            int neededCount = Mathf.CeilToInt(remainingRequiredExp / (float)expPerItem);

            int useCount = Mathf.Min(availableCount, neededCount);

            slot.SetUsedCount(useCount);

            remainingRequiredExp -= useCount * expPerItem;

            if (remainingRequiredExp <= 0) break;
        }
    }

    private void OnClickAutoToggle()
    {
        if (isAutoAdded)
        {
            foreach (UI_ExpMaterialSlot slot in activeSlots)
            {
                slot.SetUsedCount(0);
            }
        }
        else
        {
            AutoAddRequiredMaterials();
        }

        CalculatePreview();
    }

    private void ExecuteLevelUp()
    {
        if (targetData == null || targetStatus == null) return;

        CharacterStatus status = targetStatus;

        int remainingRequiredExp = Character_Manager.Instance.GetRequiredExpToCurrentCap(status);

        if (remainingRequiredExp <= 0)
        {
            UI_Manager.Instance.ShowMessage("현재 돌파 단계의 레벨 상한입니다.");

            return;
        }

        int totalMaterialExp = 0;

        foreach (UI_ExpMaterialSlot slot in activeSlots)
        {
            if (slot.usedCount <= 0) continue;

            ItemHolder holder = slot.GetItem();

            if (holder == null || holder.ItemData == null || holder.ItemData.expValue <= 0) continue;

            int expPerItem = holder.ItemData.expValue;

            int maximumNeededCount = Mathf.CeilToInt(remainingRequiredExp / (float)expPerItem);

            int consumeCount = Mathf.Min(slot.usedCount, maximumNeededCount);

            if (consumeCount <= 0) continue;

            int materialExp = consumeCount * expPerItem;

            totalMaterialExp += materialExp;
            remainingRequiredExp -= materialExp;

            Player_Inventory.Instance.RemoveItem(holder.ItemData, consumeCount);

            if (remainingRequiredExp <= 0) break;
        }

        if (totalMaterialExp <= 0) return;

        int appliedExp = Character_Manager.Instance.AddExperience(targetData.characterID, totalMaterialExp, targetData);

        if (targetStat != null) targetStat.RefreshStatsFromManager();

        Player_Inventory.Instance.CleanUpInventory();
        Player_Inventory.Instance.RefreshAllUI();

        if (targetStat != null)
        {
            targetStat.RefreshStatsFromManager();

            if (UI_Manager.Instance != null)
            {
                UI_Manager.Instance.UpdatePlayerStatus(targetStat);
            }
        }

        if (UI_Manager.Instance != null)
        {
            if (appliedExp < totalMaterialExp)
            {
                UI_Manager.Instance.ShowMessage("레벨 상한에 도달했습니다. " + "마지막 재료의 초과 경험치는 소멸했습니다.");
            }
            else
            {
                UI_Manager.Instance.ShowMessage("캐릭터 강화 완료!");
            }
        }

        OpenEnhancement(targetData);
    }
}