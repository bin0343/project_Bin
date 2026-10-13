using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponCategory
{
    OneHandedSword, // 한손검
    Greatsword,     // 대검
    Arrow,         // 권총
    MagicCatalyst   //마법
}

[System.Serializable]
public class WeaponBreakthroughMaterialRequirement
{
    [Tooltip("필요한 돌파 재료")]
    public Item_Material material;

    [Min(1)]
    [Tooltip("필요한 개수")]
    public int requiredCount = 1;
}

[System.Serializable]
public class WeaponBreakthroughRequirement
{
    [Min(1)]
    [Tooltip("돌파 후 도달하는 단계. 1은 Lv.20 → 40 돌파")]
    public int targetBreakthroughStage = 1;

    [Tooltip("이번 돌파에 필요한 재료 목록")]
    public List<WeaponBreakthroughMaterialRequirement> materials = new List<WeaponBreakthroughMaterialRequirement>();
}

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Item/Equipment")]
public class Item_Equipment : Item_Base
{
    [Header("장비 정보")]
    public EquipmentType equipmentType = EquipmentType.Weapon;
    public WeaponCategory weaponCategory = WeaponCategory.OneHandedSword;   //무기 종류
    public int attackBonus;

    [Header("성장 능력치")]
    [Tooltip("1레벨 오를 때마다 증가하는 공격력")]
    public float attackGrowthPerLevel = 5.0f;
    [Tooltip("동일 무기 1번 합성할 때마다 증가하는 공격력 비율 (0.1 = 10% 증가)")]
    public float refinementBonusPercent = 0.1f;

    [Header("무기 돌파 재료")]
    [Tooltip("무기 돌파 단계별 필요한 재료")]
    public List<WeaponBreakthroughRequirement> breakthroughRequirements = new List<WeaponBreakthroughRequirement>();

    public WeaponBreakthroughRequirement GetBreakthroughRequirement(int targetStage)
    {
        if (breakthroughRequirements == null) return null;

        return breakthroughRequirements.Find(requirement => requirement != null && requirement.targetBreakthroughStage == targetStage);
    }

    [Header("장착 시 외형")]
    [Tooltip("이 장비를 착용했을 때 캐릭터 손에 생성될 무기 프리팹")]
    public GameObject weaponPrefab;

    [Header("UI 프리뷰")]
    [Tooltip("무기 탭에서 보여줄 전용 프리뷰 프리팹. 비어 있으면 weaponPrefab을 사용합니다.")]
    public GameObject weaponPreviewPrefab;

    private void OnValidate()
    {
        isStackable = false;
        maxStackSize = 1;
        itemType = ITEMTYPE.Equipment;
    }

    public override bool Use(GameObject user)
    {
        return false;
    }
}
