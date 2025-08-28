using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlotType { INVENTORY, QUICKSLOT }

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

    // ... AddItem 메서드는 그대로 ...

    #region New Helper Methods
    // --- 슬롯 타입과 인덱스로 ItemHolder를 안전하게 가져오는 헬퍼 함수 ---
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

    // --- 슬롯 타입과 인덱스로 ItemHolder를 안전하게 설정하는 헬퍼 함수 ---
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

    // CHANGED: 헬퍼 함수를 사용하여 더 안전하고 명확하게 재작성
    public void HandleSlotDrop(SlotType sourceType, int sourceIndex, SlotType destType, int destIndex)
    {
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
}
