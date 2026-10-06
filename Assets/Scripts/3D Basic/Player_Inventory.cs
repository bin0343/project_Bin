using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum SlotType { INVENTORY, EQUIPMENT }
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

    [Header("소비 아이템 퀵슬롯")]
    public Item_Base quickSlotItem;

    [Header("최초 시작 시 지급할 기본 아이템 목록")]
    public List<StartingItem> startingItems = new List<StartingItem>();

    private void Awake()
    {
        if (instance != null && instance != this) return;
        instance = this;
    }

    private void Start()
    {
        if (UI_ItemManager.instance != null)
        {
            UI_ItemManager.instance.RefreshQuickSlot();
        }
    }

    public void InitializeNewInventory()
    {
        inventorySlots.Clear();
        quickSlotItem = null;

        if (startingItems != null)
        {
            foreach (StartingItem defaultItem in startingItems)
            {
                if (defaultItem == null) continue;

                if (defaultItem.itemData == null || defaultItem.quantity <= 0) continue;

                AddItem(defaultItem.itemData, defaultItem.quantity, false);
            }
        }

        RefreshAllUI();

        Debug.Log("[Inventory] 신규 계정 기본 아이템 지급 완료");
    }

    #region Add Item
    public bool AddItem(Item_Base item, int quantity = 1, bool showToast = true)
    {
        if (!item.isStackable)
        {
            for (int i = 0; i < quantity; i++)
            {
                inventorySlots.Add(new ItemHolder(item, 1));
                if (showToast && UI_ItemToastManager.Instance != null)
                    UI_ItemToastManager.Instance.ShowToast(item, 1);
            }
            RefreshAllUI();
            GameDataManager.Instance?.RequestAutoSave();
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

            if (amountToAdd > 0 && showToast && UI_ItemToastManager.Instance != null)
                UI_ItemToastManager.Instance.ShowToast(item, amountToAdd);

            if (remainingQuantity <= 0)
            {
                RefreshAllUI();
                GameDataManager.Instance?.RequestAutoSave();
                return true;
            }
        }

        while (remainingQuantity > 0)
        {
            int amountForNewStack = Mathf.Min(item.maxStackSize, remainingQuantity);
            inventorySlots.Add(new ItemHolder(item, amountForNewStack));
            remainingQuantity -= amountForNewStack;

            if (amountForNewStack > 0 && showToast && UI_ItemToastManager.Instance != null)
            {
                UI_ItemToastManager.Instance.ShowToast(item, amountForNewStack);
            }

            RefreshAllUI();
            GameDataManager.Instance?.RequestAutoSave();
            return true;
        }

        RefreshAllUI();
        GameDataManager.Instance?.RequestAutoSave();
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

        GameDataManager.Instance?.RequestAutoSave();
        return true;
    }
    #endregion

    #region QuickSlot

    public void RegisterQuickSlot(ItemHolder itemHolder)
    {
        if (itemHolder == null || itemHolder.ItemData == null)
            return;

        if (itemHolder.ItemData.itemType != ITEMTYPE.Consumable)
            return;

        quickSlotItem = itemHolder.ItemData;

        RefreshAllUI();
        GameDataManager.Instance?.RequestAutoSave();

        Debug.Log($"{quickSlotItem.itemName}을 퀵슬롯에 등록했습니다.");
    }

    public int GetQuickSlotQuantity()
    {
        if (quickSlotItem == null) return 0;

        return inventorySlots.Where(slot => slot != null && slot.ItemData == quickSlotItem && slot.Quantity > 0).Sum(slot => slot.Quantity);
    }

    public bool TryUseQuickSlot(GameObject user)
    {
        if (quickSlotItem == null) return false;

        ItemHolder holder = inventorySlots.FirstOrDefault(slot => slot != null && slot.ItemData == quickSlotItem && slot.Quantity > 0);

        // 등록되어 있는데 실제 아이템은 하나도 없는 경우
        if (holder == null)
        {
            quickSlotItem = null;
            RefreshAllUI();
            return false;
        }

        bool success = quickSlotItem.Use(user);

        if (!success) return false;

        holder.Quantity--;

        CleanUpInventory();

        // 마지막 1개까지 사용했다면 퀵슬롯 등록도 해제
        if (GetQuickSlotQuantity() <= 0)
        {
            quickSlotItem = null;
        }

        RefreshAllUI();
        GameDataManager.Instance?.RequestAutoSave();
        return true;
    }

    #endregion

    public void CleanUpInventory()
    {
        inventorySlots.RemoveAll(slot => slot == null || slot.ItemData == null || slot.Quantity <= 0);

        if (quickSlotItem != null && GetQuickSlotQuantity() <= 0)
        {
            quickSlotItem = null;
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

        return null;
    }

    private void SetItemHolderAt(SlotType type, int index, ItemHolder itemHolder)
    {
        if (type == SlotType.INVENTORY)
        {
            if (index < 0 || index >= inventorySlots.Count) return;
            inventorySlots[index] = itemHolder;
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
        GameDataManager.Instance?.RequestAutoSave();
    }

    public void RefreshAllUI()
    {
        if (UI_Manager.Instance != null && UI_Manager.Instance.UI_Inventory != null)
        {
            UI_Manager.Instance.UI_Inventory.RefreshUI();
        }

        // 퀵슬롯 UI도 마찬가지로 체크
        if (UI_ItemManager.instance != null)
        {
            UI_ItemManager.instance.RefreshQuickSlot();
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
        GameDataManager.Instance?.RequestAutoSave();
    }
    #endregion

    #region Save & Load

    public InventorySaveData GetSaveData()
    {
        InventorySaveData data = new InventorySaveData();

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            ItemHolder holder = inventorySlots[i];

            if (holder == null || holder.ItemData == null) continue;

            ItemSaveData itemData = holder.GetSaveData(i);

            if (itemData != null)
            {
                data.items.Add(itemData);
            }
        }

        if (quickSlotItem != null)
        {
            data.quickSlotItemID = quickSlotItem.itemID;
        }

        return data;
    }

    public void LoadSaveData(InventorySaveData data)
    {
        inventorySlots.Clear();
        quickSlotItem = null;

        if (data == null)
        {
            RefreshAllUI();
            return;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("[Inventory] GameDataManager가 없어 " + "아이템 원본을 복구할 수 없습니다.");

            return;
        }

        if (data.items != null)
        {
            List<ItemSaveData> sortedItems = data.items.OrderBy(item => item.slotIndex).ToList();

            foreach (ItemSaveData savedItem in sortedItems)
            {
                if (savedItem == null || string.IsNullOrEmpty(savedItem.itemID)) continue;

                Item_Base original = GameDataManager.Instance.GetItemByID(savedItem.itemID);

                if (original == null)
                {
                    Debug.LogWarning($"[Inventory] 아이템 원본을 " + $"찾을 수 없습니다. " + $"ID: {savedItem.itemID}");

                    continue;
                }

                ItemHolder holder = ItemHolder.FromSaveData(original, savedItem);

                if (holder != null)
                {
                    inventorySlots.Add(holder);
                }
            }
        }

        RestoreQuickSlot(data.quickSlotItemID);

        RefreshAllUI();

        Debug.Log($"[Inventory] 인벤토리 " + $"{inventorySlots.Count}개 복구 완료");
    }

    #endregion

    //퀵슬롯 복구
    private void RestoreQuickSlot(string itemID)
    {
        quickSlotItem = null;

        if (string.IsNullOrEmpty(itemID)) return;

        ItemHolder holder = inventorySlots.FirstOrDefault(
                slot =>
                    slot != null &&
                    slot.ItemData != null &&
                    slot.ItemData.itemID == itemID &&
                    slot.Quantity > 0
            );

        if (holder == null) return;

        if (holder.ItemData.itemType != ITEMTYPE.Consumable) return;

        quickSlotItem = holder.ItemData;
    }
}
