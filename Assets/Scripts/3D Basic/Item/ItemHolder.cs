using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHolder
{
    public Item_Base ItemData { get; private set; } // 아이템 원본 데이터
    public int Quantity;                            // 아이템 개수
    public int LastUseTime;

    public ItemHolder(Item_Base itemData, int quantity)
    {
        ItemData = itemData;
        this.Quantity = quantity;
    }

    public void AddQuantity(int amount)     //개수 추가
    {
        Quantity += amount;
    }

    public void RemoveQuantity(int amount)      //개수 감소
    {
        Quantity -= amount;
    }

    // 아이템 사용
    public void Use(GameObject user)
    {
        ItemData.Use(user);
    }

    public float GetRemainingCooldown()
    {
        float remaining = (LastUseTime + ItemData.cooldownTime) - Time.time;
        return Mathf.Max(0f, remaining);
    }
}
