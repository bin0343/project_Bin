using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlotType { INVENTORY, QUICKSLOT }

public class Player_Inventory : MonoBehaviour
{
    public static Player_Inventory Instance;

    public ItemHolder[] inventorySlots = new ItemHolder[20]; // 20칸짜리 인벤토리
    public ItemHolder[] quickSlots = new ItemHolder[4];   // 4칸짜리 퀵슬롯

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
        // 게임이 시작되면 UI_ItemManager에게 퀵슬롯 UI를 갱신하라고 즉시 요청합니다.
        if (UI_ItemManager.Instance != null)
        {
            UI_ItemManager.Instance.SetupItemSlots(quickSlots);
        }
    }

    // 아이템을 얻는 함수 (나중에 아이템 줍기 등에 사용)
    public bool AddItem(Item_Base item, int quantity = 1)
    {
        // 1. 겹칠 수 있는 아이템이고, 인벤토리에 이미 같은 아이템이 있는지 확인
        if (item.isStackable)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] != null && inventorySlots[i].ItemData == item && inventorySlots[i].Quantity < item.maxStackSize)
                {
                    inventorySlots[i].AddQuantity(quantity);
                    return true;
                }
            }
        }

        // 2. 비어있는 슬롯을 찾아 새로 추가
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
            {
                inventorySlots[i] = new ItemHolder(item, quantity);
                return true;
            }
        }

        // 3. 인벤토리가 가득 참
        Debug.Log("인벤토리가 가득 찼습니다.");
        return false;
    }

    // 슬롯 간 아이템 교환
    public void SwapSlots(SlotType typeA, int indexA, SlotType typeB, int indexB)
    {
        ItemHolder[] arrayA = (typeA == SlotType.INVENTORY) ? inventorySlots : quickSlots;
        ItemHolder[] arrayB = (typeB == SlotType.INVENTORY) ? inventorySlots : quickSlots;

        if (indexA < 0 || indexA >= arrayA.Length || indexB < 0 || indexB >= arrayB.Length) return;

        ItemHolder temp = arrayA[indexA];
        arrayA[indexA] = arrayB[indexB];
        arrayB[indexB] = temp;

        // 데이터 변경 후 UI 새로고침 요청
        UI_Manager.Instance.UI_Inventory.RefreshUI();
        // 퀵슬롯 UI도 새로고침 (UI_ItemManager가 있다면)
        if (UI_ItemManager.Instance != null)
        {
            UI_ItemManager.Instance.SetupItemSlots(quickSlots);
        }
    }
}
