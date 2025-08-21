using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Item/Item Data")]
public abstract class Item_Base : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;
    public Sprite itemIcon;
    public ITEMTYPE itemType;
    public int Quantity;
    public int cooldownTime;

    [TextArea]
    public string description;

    [Header("아이템 속성")]
    public bool isStackable = true; // 겹치는게 가능한가?
    public int maxStackSize = 99;   // 최대겹치기 갯수

    public abstract void Use(GameObject user);  //아이템 사용 시 효과 추상함수
}
