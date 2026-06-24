using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum SlotType { INVENTORY, QUICKSLOT, EQUIPMENT }
public enum ItemSortMethod { NAME, TYPE, QUANTITY }

[System.Serializable]
public class StartingItem
{
    public Item_Base itemData;
    public int quantity = 1;
}

public class Player_Inventory : MonoBehaviour
{
    public static Player_Inventory instance;

    public List<ItemHolder> inventorySlots = new List<ItemHolder>();
    public ItemHolder[] quickSlots = new ItemHolder[4];

    [Header("최초 시작 시 지급할 기본 아이템 목록")]
    public List<StartingItem> startingItems = new List<StartingItem>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    private void Start()
    {
        if (inventorySlots.Count == 0 && startingItems.Count > 0)
        {
            foreach (var defaultItem in startingItems)
            {
                if (defaultItem.itemData != null && defaultItem.quantity > 0)
                {
                    AddItem(defaultItem.itemData, defaultItem.quantity, false); 
                }
            }
        }
        if (UI_ItemManager.Instance != null)
        {
            UI_ItemManager.Instance.SetupItemSlots(quickSlots);
        }
    }

    #region Add Item
    public bool AddItem(Item_Base item, int quantity = 1, bool showToast = true)
    {
        if (!item.isStackable)
        {
            for (int i = 0; i < quantity; i++)
            {
                inventorySlots.Add(new ItemHolder(item, 1));
                if (showToast && UI_ItemToastManager.instance != null)
                    UI_ItemToastManager.instance.ShowToast(item, 1);
            }
            RefreshAllUI();
            return true;
        }

        int remainingQuantity = quantity;

        List<ItemHolder> existingStacks = inventorySlots.Where(slot =>
            slot != null && slot.ItemData == item && slot.Quantity < item.maxStackSize).ToList();

        foreach (var stack in existingStacks)
        {
            int spaceAvailable = item.maxStackSize - stack.Quantity;
            int amountToAdd = Mathf.Min(spaceAvailable, remainingQuantity);

            stack.AddQuantity(amountToAdd);
            remainingQuantity -= amountToAdd;

            if (amountToAdd > 0 && showToast && UI_ItemToastManager.instance != null)
                UI_ItemToastManager.instance.ShowToast(item, amountToAdd);

            if (remainingQuantity <= 0)
            {
                RefreshAllUI();
                return true;
            }
        }

        while (remainingQuantity > 0)
        {
            int amountForNewStack = Mathf.Min(item.maxStackSize, remainingQuantity);
            inventorySlots.Add(new ItemHolder(item, amountForNewStack));
            remainingQuantity -= amountForNewStack;

            if (amountForNewStack > 0 && showToast && UI_ItemToastManager.instance != null)
            {
                UI_ItemToastManager.instance.ShowToast(item, amountForNewStack);
            }

            RefreshAllUI();
            return true;
        }

        RefreshAllUI();
        return true;
    }
    #endregion

    #region Remove Item
    public bool RemoveItem(Item_Base item, int quantityToRemove)
    {
        int totalOwned = inventorySlots.Where(slot => slot.ItemData == item).Sum(slot => slot.Quantity);
        if (totalOwned < quantityToRemove) return false;

        int remainingToRemove = quantityToRemove;

        for (int i = inventorySlots.Count - 1; i >= 0; i--)
        {
            ItemHolder slot = inventorySlots[i];
            if (slot == null || slot.ItemData != item) continue;

            int amountToRemoveFromSlot = Mathf.Min(remainingToRemove, slot.Quantity);
            slot.Quantity -= amountToRemoveFromSlot;
            remainingToRemove -= amountToRemoveFromSlot;

            if (remainingToRemove <= 0) break;
        }

        CleanUpInventory(); 
        RefreshAllUI();
        return true;
    }
    #endregion

    public void CleanUpInventory()
    {
        inventorySlots.RemoveAll(slot => slot == null || slot.ItemData == null || slot.Quantity <= 0);
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
        if (destType == SlotType.EQUIPMENT)
        {
            if (sourceType == SlotType.EQUIPMENT) return;

            ItemHolder sourceItem = GetItemHolderAt(sourceType, sourceIndex);
            if (sourceItem != null)
            {
                if (BattleManager.instance != null)
                {
                    GameObject activePlayer = BattleManager.instance.GetActiveCharacter();
                    if (activePlayer != null)
                    {
                        Player_Equipment equip = activePlayer.GetComponent<Player_Equipment>();
                        if (equip != null) equip.Equip(sourceItem, sourceType, sourceIndex);
                    }
                }
                return;
            }
        }

        if (sourceType == SlotType.EQUIPMENT || (currentTab != UI_Inventory.InventoryTabType.ALL && destType == SlotType.INVENTORY))
        {
            return;
        }

        //인벤토리/퀵슬롯 간의 일반적인 이동
        ItemHolder sourceItemGeneral = GetItemHolderAt(sourceType, sourceIndex);
        ItemHolder destItemGeneral = GetItemHolderAt(destType, destIndex);

        // 겹치기 로직
        if (sourceItemGeneral != null && destItemGeneral != null && sourceItemGeneral.ItemData == destItemGeneral.ItemData && destItemGeneral.ItemData.isStackable && destItemGeneral.Quantity < destItemGeneral.ItemData.maxStackSize)
        {
            int spaceAvailable = destItemGeneral.ItemData.maxStackSize - destItemGeneral.Quantity;
            int amountToMove = Mathf.Min(spaceAvailable, sourceItemGeneral.Quantity);

            destItemGeneral.AddQuantity(amountToMove);
            sourceItemGeneral.Quantity -= amountToMove;
        }
        // 교환 로직
        else
        {
            SetItemHolderAt(destType, destIndex, sourceItemGeneral);
            SetItemHolderAt(sourceType, sourceIndex, destItemGeneral);
        }

        CleanUpInventory();
        RefreshAllUI();
    }

    public void RefreshAllUI()
    {
        if (UI_Manager.instance != null && UI_Manager.instance.UI_Inventory != null)
        {
            UI_Manager.instance.UI_Inventory.RefreshUI();
        }

        // 퀵슬롯 UI도 마찬가지로 체크
        if (UI_ItemManager.Instance != null)
        {
            UI_ItemManager.Instance.SetupItemSlots(quickSlots);
        }
    }

    #region Sorting
    public void SortItems(ItemSortMethod method)
    {
        CleanUpInventory();
        switch (method)
        {
            case ItemSortMethod.NAME:
                inventorySlots = inventorySlots.OrderBy(holder => holder.ItemData.itemName).ToList();
                break;
        }
        RefreshAllUI();
    }
    #endregion
}
