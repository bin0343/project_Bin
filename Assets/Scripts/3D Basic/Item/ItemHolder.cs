using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemHolder
{
    public Item_Base ItemData;// 아이템 원본 데이터
    public int Quantity;                            // 아이템 개수
    private float LastUseTime;

    public ItemHolder(Item_Base itemData, int quantity)
    {
        ItemData = itemData;
        this.Quantity = quantity;
    }

    public void AddQuantity(int amount)     //개수 추가
    {
        Quantity += amount;
    }

    // 아이템 사용 로직은 이제 Player_Action에서 직접 처리하므로 이 메서드는 제거해도 무방합니다.
    // public void RemoveQuantity(int amount)
    // {
    //     Quantity -= amount;
    // }

    // 아이템 사용
    // CHANGED: 반환 타입을 bool로 변경하고, 내부의 수량 감소 로직을 제거
    public bool Use(GameObject user)
    {
        if (ItemData == null) return false;

        // ItemData의 Use 결과를 그대로 반환해주는 역할만 수행
        return ItemData.Use(user);
    }

    public float GetRemainingCooldown()
    {
        // ... (이 부분은 아이템 쿨타임 기능이 필요할 때 구현) ...
        return 0f;
    }
}
