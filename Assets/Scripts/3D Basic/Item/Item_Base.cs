using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentType { Weapon }

[CreateAssetMenu(fileName = "NewItemData", menuName = "Item/Item Data")]
public abstract class Item_Base : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;
    public Sprite itemIcon;
    public ITEMTYPE itemType;
    public int Quantity;
    public int cooldownTime;
    public string itemID;

    [TextArea]
    public string itemDescription;

    [Header("아이템 속성")]
    public bool isStackable = true; // 겹치는게 가능한가?
    public int maxStackSize = 99;   // 최대겹치기 갯수

    [Header("강화 재료 설정")]
    [Tooltip("아이템의 등급 (예: 1~5성) - 일괄 추가 필터링에 사용됩니다.")] 
    public int rarity = 1;
    [Tooltip("이 아이템을 무기 강화 재료로 먹였을 때 오르는 경험치 양")]
    public int expValue = 0;

    public abstract bool Use(GameObject user);
}
