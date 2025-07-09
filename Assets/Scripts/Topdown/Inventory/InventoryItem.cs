using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemData itemData;
    public int count;

    public InventoryItem(ItemData data, int count = 1)
    {
        itemData = data;
        this.count = count;
    }
}
