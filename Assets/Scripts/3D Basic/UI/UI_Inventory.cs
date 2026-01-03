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

    [Header("아이템 상세 정보(UI패널)")]
    public GameObject detailPanel;
    public Image itemIcon;
    public Text itemName;
    public Text itemCount;
    public Text itemDescription;

    [Header("설정")]
    public bool enableDrag = true;  //drag허용, 비허용
    public bool showDetailOnHover = false;  //마우스 올리면 표시

    [Header("기타 UI")]
    public Dropdown sortDropdown;

    // 생성된 모든 UI 슬롯을 담아두는 리스트
    private List<UI_ItemSlot> slots = new List<UI_ItemSlot>();
    private Player_Inventory playerInventory;

    public InventoryTabType CurrentTab => currentTab;


    void Start()
    {
        playerInventory = Player_Inventory.instance;
        InitializeSlots();

        allTabButton.onClick.AddListener(() => ChangeTab(InventoryTabType.ALL));
        equipmentTabButton.onClick.AddListener(() => ChangeTab(InventoryTabType.EQUIPMENT));
        consumableTabButton.onClick.AddListener(() => ChangeTab(InventoryTabType.CONSUMABLE));
        etcTabButton.onClick.AddListener(() => ChangeTab(InventoryTabType.ETC));

        if (sortDropdown != null)
        {
            sortDropdown.onValueChanged.AddListener(OnSortChanged);
        }

        ChangeTab(InventoryTabType.ALL);
        //gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // 상세 정보창 끄기 (초기화)
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

        // 인벤토리 목록 새로고침 (데이터 동기화)
        if (playerInventory != null)
        {
            RefreshUI();
        }
    }

    // 처음에 인벤토리 크기만큼 슬롯을 미리 생성하고 리스트에 담아둠
    private void InitializeSlots()
    {
        for (int i = 0; i < playerInventory.inventorySlots.Count; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotGridParent);
            UI_ItemSlot slot = slotGO.GetComponent<UI_ItemSlot>();
            slot.Initialize(SlotType.INVENTORY, i);
            slots.Add(slot);
        }
    }

    public void ChangeTab(InventoryTabType newTab)
    {
        currentTab = newTab;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (currentTab == InventoryTabType.ALL)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].SetDraggable(enableDrag);

                ItemHolder item = playerInventory.inventorySlots[i];
                slots[i].Initialize(SlotType.INVENTORY, i);

                if (item != null)
                {
                    slots[i].Setup(item);
                }
                else
                {
                    slots[i].Clear();
                }
            }
        }
        // '장비', '소비' 등 필터링된 탭일 경우
        else
        {
            List<ItemHolder> filteredItems = new List<ItemHolder>();
            List<int> originalIndices = new List<int>();

            for (int i = 0; i < playerInventory.inventorySlots.Count; i++)
            {
                ItemHolder item = playerInventory.inventorySlots[i];
                if (item != null && item.ItemData != null)
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

            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].SetDraggable(enableDrag);

                // 표시할 필터링된 아이템이 있다면
                if (i < filteredItems.Count)
                {
                    slots[i].Initialize(SlotType.INVENTORY, originalIndices[i]);
                    slots[i].Setup(filteredItems[i]);
                }
                // 표시할 아이템이 없다면, 나머지 슬롯은 모두 빈 칸으로 처리
                else
                {
                    slots[i].Initialize(SlotType.INVENTORY, -1);
                    slots[i].Clear();
                }
            }
        }
    }

    private void OnSortChanged(int index)
    {
        switch (index)
        {
            case 0: 
                playerInventory.SortItems(ItemSortMethod.NAME);
                break;
        }
    }

    public void CloseDetailPanel()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }

    public void UpdateDetailView(ItemHolder itemHolder)
    {
        if (itemHolder == null || itemHolder.ItemData == null)
        {
            //아이템 없는 슬롯 누르면 상세창 끄기
            if (detailPanel != null) detailPanel.SetActive(false);
            return;
        }
        //상세창 켜기
        if (detailPanel != null) detailPanel.SetActive(true);

        //이미지 설정
        /*if (detailPanel != null)
        {
            itemIcon.sprite = itemHolder.ItemData.itemIcon;
            itemIcon.gameObject.SetActive(true);
        }

        //이름 설정
        if (detailPanel != null)
        {
            itemName.text = itemHolder.ItemData.itemName;
        }

        //수량 설정
        if (detailPanel != null)
        {
            itemCount.text = $"보유 수량\n <b><color=blue>×{itemHolder.Quantity}</color></b>";
        }

        //설명 설정
        if (detailPanel != null)
        {
            itemDescription.text = itemHolder.ItemData.description;
        }*/
        if (detailPanel != null)
        {
            if (itemIcon != null) { itemIcon.sprite = itemHolder.ItemData.itemIcon; itemIcon.gameObject.SetActive(true); }
            if (itemName != null) itemName.text = itemHolder.ItemData.itemName;
            if (itemCount != null) itemCount.text = $"보유 수량\n <b><color=blue>×{itemHolder.Quantity}</color></b>";
            if (itemDescription != null) itemDescription.text = itemHolder.ItemData.description;
        }

        if (showDetailOnHover && detailPanel != null)
        {
            // 마우스 위치에서 오른쪽(+15), 아래(-15)로 살짝 띄우기
            Vector3 offset = new Vector3(15, -15, 0);
            detailPanel.transform.position = Input.mousePosition + offset;
        }
    }
}
