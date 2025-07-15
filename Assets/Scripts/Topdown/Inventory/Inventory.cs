using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int maxSlots = 10;
    public List<InventoryItem> items = new List<InventoryItem>();

    public delegate void OnInventoryChanged();
    public event OnInventoryChanged onInventoryChanged;

    public void AddItem(ItemData newItem)
    {
        InventoryItem existing = items.Find(i => i.itemData == newItem);
        if (existing != null)
        {
            existing.count++;
            onInventoryChanged?.Invoke();
        }
        else
        {
            if (items.Count < maxSlots)
            {
                items.Add(new InventoryItem(newItem));
                onInventoryChanged?.Invoke();
            }
            else
            {
                Debug.Log("인벤토리가 가득 찼습니다!");
                return;
            }
        }

        Debug.Log($"{newItem.itemName} 인벤토리에 추가됨!");
        onInventoryChanged?.Invoke();
    }

    public int GetItemCount(ItemData item)
    {
        foreach (var i in items)
        {
            if (i.itemData == item)
                return i.count;
        }
        return 0;
    }

    public void RemoveItem(ItemData item, int amount)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemData == item)
            {
                items[i].count -= amount;
                if (items[i].count <= 0)
                    items.RemoveAt(i);

                onInventoryChanged?.Invoke();
                return;
            }
        }
    }
}
