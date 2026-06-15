using UnityEngine;

[CreateAssetMenu(fileName = "NewMaterial", menuName = "Item/Material (강화 재료)")]
public class Item_Material : Item_Base
{
    public override bool Use(GameObject user)
    {
        // UI 매니저가 있다면 화면에 알림을 띄워줘도 좋습니다.
        if (UI_Manager.instance != null)
            UI_Manager.instance.ShowMessage("이 아이템은 강화 화면에서 사용할 수 있습니다.");

        return false;
    }
}