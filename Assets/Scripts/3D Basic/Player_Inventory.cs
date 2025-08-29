using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum SlotType { INVENTORY, QUICKSLOT }
public enum ItemSortMethod
{
    NAME,
    TYPE,
    QUANTITY
}

public class Player_Inventory : MonoBehaviour
{
    public static Player_Inventory Instance;

    public List<ItemHolder> inventorySlots = new List<ItemHolder>(); // 리스트로 변경
    public ItemHolder[] quickSlots = new ItemHolder[4];

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
        if (UI_ItemManager.Instance != null)
        {
            UI_ItemManager.Instance.SetupItemSlots(quickSlots);
        }
    }


    #region New Helper Methods
    private ItemHolder GetItemHolderAt(SlotType type, int index)
    {
        if (type == SlotType.INVENTORY)
        {
            if (index < 0 || index >= inventorySlots.Count) return null;
            return inventorySlots[index];
        }
        else // QUICKSLOT
        {
            if (index < 0 || index >= quickSlots.Length) return null;
            return quickSlots[index];
        }
    }

    private void SetItemHolderAt(SlotType type, int index, ItemHolder itemHolder)
    {
        if (type == SlotType.INVENTORY)
        {
            if (index < 0 || index >= inventorySlots.Count) return;
            inventorySlots[index] = itemHolder;
        }
        else // QUICKSLOT
        {
            if (index < 0 || index >= quickSlots.Length) return;
            quickSlots[index] = itemHolder;
        }
    }
    #endregion

    public void HandleSlotDrop(SlotType sourceType, int sourceIndex, SlotType destType, int destIndex, UI_Inventory.InventoryTabType currentTab)
    {
        if (currentTab != UI_Inventory.InventoryTabType.ALL && destType == SlotType.INVENTORY)
        {
            return;
        }

        ItemHolder sourceItem = GetItemHolderAt(sourceType, sourceIndex);
        ItemHolder destItem = GetItemHolderAt(destType, destIndex);

        // 겹치기 로직
        if (sourceItem != null && destItem != null && sourceItem.ItemData == destItem.ItemData && destItem.ItemData.isStackable && destItem.Quantity < destItem.ItemData.maxStackSize)
        {
            int spaceAvailable = destItem.ItemData.maxStackSize - destItem.Quantity;
            int amountToMove = Mathf.Min(spaceAvailable, sourceItem.Quantity);

            destItem.AddQuantity(amountToMove);
            sourceItem.Quantity -= amountToMove;

            if (sourceItem.Quantity <= 0)
            {
                SetItemHolderAt(sourceType, sourceIndex, null);
            }
        }
        // 교환 로직
        else
        {
            SetItemHolderAt(destType, destIndex, sourceItem);
            SetItemHolderAt(sourceType, sourceIndex, destItem);
        }

        RefreshAllUI();
    }

    public void RefreshAllUI()
    {
        UI_Manager.Instance.UI_Inventory.RefreshUI();
        if (UI_ItemManager.Instance != null)
        {
            UI_ItemManager.Instance.SetupItemSlots(quickSlots);
        }
    }

    #region Sorting
    public void SortItems(ItemSortMethod method)
    {
        List<ItemHolder> itemsToSort = inventorySlots.Where(slot => slot != null && slot.ItemData != null).ToList();

        switch (method)
        {
            case ItemSortMethod.NAME:
                itemsToSort = itemsToSort.OrderBy(holder => holder.ItemData.itemName).ToList();
                break;
        }

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < itemsToSort.Count)
            {
                inventorySlots[i] = itemsToSort[i];
            }
            else
            {
                inventorySlots[i] = null;
            }
        }

        RefreshAllUI();
    }
    #endregion
}
