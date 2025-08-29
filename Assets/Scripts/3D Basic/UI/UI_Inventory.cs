using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory : MonoBehaviour
{
    public enum InventoryTabType { ALL, EQUIPMENT, CONSUMABLE, ETC }
    private InventoryTabType currentTab;

    [Header("UI 구성 요소")]
    public GameObject slotPrefab;
    public Transform slotGridParent;

    [Header("탭 버튼")]
    public Button allTabButton;
    public Button equipmentTabButton;
    public Button consumableTabButton;
    public Button etcTabButton;

    // 생성된 슬롯들을 담아둘 리스트
    private List<UI_ItemSlot> slots = new List<UI_ItemSlot>();
    private Player_Inventory playerInventory;

    void Start()
    {
        playerInventory = Player_Inventory.Instance;
        InitializeSlots(); // 슬롯을 미리 생성하여 풀(Pool)을 만듭니다.

        allTabButton.onClick.AddListener(() => ChangeTab(InventoryTabType.ALL));
        equipmentTabButton.onClick.AddListener(() => ChangeTab(InventoryTabType.EQUIPMENT));
        consumableTabButton.onClick.AddListener(() => ChangeTab(InventoryTabType.CONSUMABLE));
        etcTabButton.onClick.AddListener(() => ChangeTab(InventoryTabType.ETC));

        ChangeTab(InventoryTabType.ALL);
        gameObject.SetActive(false);
    }

    // 슬롯 UI들을 미리 생성하여 '풀(Pool)'로 만들어 둡니다.
    private void InitializeSlots()
    {
        for (int i = 0; i < playerInventory.inventorySlots.Count; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotGridParent);
            UI_ItemSlot slot = slotGO.GetComponent<UI_ItemSlot>();
            slots.Add(slot);
        }
    }

    public void ChangeTab(InventoryTabType newTab)
    {
        currentTab = newTab;
        RefreshUI();
    }

    // CHANGED: RefreshUI 로직을 '재활용' 방식으로 수정
    public void RefreshUI()
    {
        // 1. 필터링 로직 (기존과 동일)
        List<ItemHolder> filteredItems = new List<ItemHolder>();
        List<int> originalIndices = new List<int>();

        for (int i = 0; i < playerInventory.inventorySlots.Count; i++)
        {
            ItemHolder item = playerInventory.inventorySlots[i];

            // '전체' 탭에서는 빈 슬롯도 표시
            if (currentTab == InventoryTabType.ALL)
            {
                filteredItems.Add(item);
                originalIndices.Add(i);
            }
            // 다른 탭에서는 아이템이 있는 칸만 필터링
            else if (item != null && item.ItemData != null)
            {
                bool shouldShow = false;
                switch (currentTab)
                {
                    case InventoryTabType.EQUIPMENT:
                        if (item.ItemData.itemType == ITEMTYPE.Equipment) shouldShow = true;
                        break;
                    case InventoryTabType.CONSUMABLE:
                        if (item.ItemData.itemType == ITEMTYPE.Consumable) shouldShow = true;
                        break;
                }
                if (shouldShow)
                {
                    filteredItems.Add(item);
                    originalIndices.Add(i);
                }
            }
        }

        // 2. 미리 만들어 둔 슬롯들을 '재활용'하여 UI 업데이트
        for (int i = 0; i < slots.Count; i++)
        {
            // 표시할 아이템이 있다면
            if (i < filteredItems.Count)
            {
                slots[i].gameObject.SetActive(true); // 슬롯을 켜고
                slots[i].Initialize(SlotType.INVENTORY, originalIndices[i]); // 원본 인덱스 부여

                // 아이템이 있다면 Setup, 없다면 Clear
                if (filteredItems[i] != null)
                {
                    slots[i].Setup(filteredItems[i]);
                }
                else
                {
                    slots[i].Clear();
                }
            }
            // 표시할 아이템이 없다면
            else
            {
                slots[i].gameObject.SetActive(false); // 슬롯을 끈다
            }
        }
    }
}
