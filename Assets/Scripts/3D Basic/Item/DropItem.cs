using UnityEngine;

[System.Serializable]
public class DropItem
{
    public ItemData itemData;   // 어떤 아이템인지
    [Range(0f, 1f)]
    public float dropChance;    // 드랍 확률
    public int minAmount;       // 최소 개수
    public int maxAmount;       // 최대 개수
}