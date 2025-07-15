using UnityEngine;

public enum ItemType
{
    Potion,
    Coin,
    Weapon,
    Armor,
    Material
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int itemID;
    public Sprite icon;
    public ItemType itemType;
    public int amount; // 예: 회복량, 공격력, 가격 등
    public int BuyPrice;
    public int SellPrice;
    public bool isConsumable;     // true면 사용 시 개수 감소

    [TextArea]
    public string description;
}