using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum SlotType { INVENTORY, QUICKSLOT, EQUIPMENT }
public enum ItemSortMethod
{
    NAME,
    TYPE,
    QUANTITY
}

public class Player_Inventory : MonoBehaviour
{
    public static Player_Inventory instance;

    public List<ItemHolder> inventorySlots = new List<ItemHolder>();
    public ItemHolder[] quickSlots = new ItemHolder[4];

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
        if (UI_ItemManager.Instance != null)
        {
            UI_ItemManager.Instance.SetupItemSlots(quickSlots);
        }
    }

    #region Add Item
    public bool AddItem(Item_Base item, int quantity = 1)
    {
        if (!item.isStackable)
        {
            for (int i = 0; i < quantity; i++)
            {
                // 빈 슬롯을 찾아서 아이템을 1개씩 추가
                int emptySlotIndex = inventorySlots.FindIndex(slot => slot == null || slot.ItemData == null);
                if (emptySlotIndex != -1)
                {
                    inventorySlots[emptySlotIndex] = new ItemHolder(item, 1);

                    if (UI_ItemToastManager.instance != null)
                        UI_ItemToastManager.instance.ShowToast(item, 1);
                }
                else
                {
                    Debug.Log("인벤토리가 가득 찼습니다.");
                    RefreshAllUI();
                    return false; 
                }
            }
            RefreshAllUI();
            return true;
        }

        int remainingQuantity = quantity;

        // 1. 기존 스택에 최대한 채우기
        List<ItemHolder> existingStacks = inventorySlots.Where(slot =>
            slot != null && slot.ItemData == item && slot.Quantity < item.maxStackSize).ToList();

        foreach (var stack in existingStacks)
        {
            int spaceAvailable = item.maxStackSize - stack.Quantity;
            int amountToAdd = Mathf.Min(spaceAvailable, remainingQuantity);

            stack.AddQuantity(amountToAdd);
            remainingQuantity -= amountToAdd;

            if (amountToAdd > 0 && UI_ItemToastManager.instance != null)
                UI_ItemToastManager.instance.ShowToast(item, amountToAdd);

            if (remainingQuantity <= 0)
            {
                RefreshAllUI();
                return true;
            }
        }

        while (remainingQuantity > 0)
        {
            int emptySlotIndex = inventorySlots.FindIndex(slot => slot == null || slot.ItemData == null);

            if (emptySlotIndex != -1)
            {
                int amountForNewStack = Mathf.Min(item.maxStackSize, remainingQuantity);
                inventorySlots[emptySlotIndex] = new ItemHolder(item, amountForNewStack);
                remainingQuantity -= amountForNewStack;

                if (amountForNewStack > 0 && UI_ItemToastManager.instance != null)
                    UI_ItemToastManager.instance.ShowToast(item, amountForNewStack);
            }
            else
            {
                Debug.Log("인벤토리가 가득 찼습니다.");
                RefreshAllUI();
                return false; // 남은 아이템을 추가하지 못하고 실패
            }
        }

        RefreshAllUI();
        return true;
    }
    #endregion

    #region Remove Item
    public bool RemoveItem(Item_Base item, int quantityToRemove)
    {
        int totalOwned = 0;

        if (!item.isStackable)
        {
            totalOwned = inventorySlots.Count(slot => slot != null && slot.ItemData == item);
        }
        else
        {
            totalOwned = inventorySlots.Where(slot => slot != null && slot.ItemData == item).Sum(slot => slot.Quantity);
        }

        if (totalOwned < quantityToRemove)
        {
            return false;
        }

        int remainingToRemove = quantityToRemove;

        for (int i = inventorySlots.Count - 1; i >= 0; i--)
        {
            ItemHolder slot = inventorySlots[i];
            if (slot == null || slot.ItemData != item) continue;

            if (!item.isStackable)
            {
                inventorySlots[i] = null;
                remainingToRemove--;
            }
            else
            {
                int amountToRemoveFromSlot = Mathf.Min(remainingToRemove, slot.Quantity);
                slot.Quantity -= amountToRemoveFromSlot;
                remainingToRemove -= amountToRemoveFromSlot;

                if (slot.Quantity <= 0)
                {
                    inventorySlots[i] = null;
                }
            }

            if (remainingToRemove <= 0) break;
        }

        RefreshAllUI();
        return true;
    }
    #endregion

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
        //아이템 장착 시도 (인벤토리/퀵슬롯 -> 장비 슬롯)
        if (destType == SlotType.EQUIPMENT)
        {
            // 장비창에서 장비창으로 이동하는 것은 막음
            if (sourceType == SlotType.EQUIPMENT) return;

            ItemHolder sourceItem = GetItemHolderAt(sourceType, sourceIndex);
            if (sourceItem != null)
            {
                // 장착 시도
                Player_Equipment.instance.Equip(sourceItem, sourceType, sourceIndex);
                // Equip 함수가 모든 데이터 처리와 UI갱신을 하므로 여기서 종료
                return;
            }
        }

        // 아이템 장착 해제
        if (sourceType == SlotType.EQUIPMENT)
        {
            return;
        }


        //필터링된 탭에서 인벤토리 내부로 드롭 방지
        if (currentTab != UI_Inventory.InventoryTabType.ALL && destType == SlotType.INVENTORY)
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

            if (sourceItemGeneral.Quantity <= 0)
            {
                SetItemHolderAt(sourceType, sourceIndex, null);
            }
        }
        // 교환 로직
        else
        {
            SetItemHolderAt(destType, destIndex, sourceItemGeneral);
            SetItemHolderAt(sourceType, sourceIndex, destItemGeneral);
        }

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
