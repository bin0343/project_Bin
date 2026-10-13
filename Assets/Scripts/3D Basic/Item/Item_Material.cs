using UnityEngine;

public enum MaterialUsageType
{
    None,
    CommonEnhancement,      // 캐릭터 + 무기 공용 강화석
    CharacterEnhancement,   // 캐릭터 경험치 재료
    WeaponEnhancement,      // 무기 경험치 재료
    CharacterAscension,     // 캐릭터 돌파 재료
    WeaponBreakthrough      // 무기 돌파 재료
}

[CreateAssetMenu(fileName = "NewMaterial", menuName = "Item/Material (강화 재료)")]
public class Item_Material : Item_Base
{
    [Header("재료 용도")]
    public MaterialUsageType materialUsage = MaterialUsageType.None;

    public override bool Use(GameObject user)
    {
        // UI 매니저가 있다면 화면에 알림을 띄워줘도 좋습니다.
        if (UI_Manager.Instance != null)
            UI_Manager.Instance.ShowMessage("이 아이템은 강화 화면에서 사용할 수 있습니다.");

        return false;
    }
}