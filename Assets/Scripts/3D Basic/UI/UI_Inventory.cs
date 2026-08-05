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
    public Image itemCountImage;
    public Text itemName;
    public Text itemCount;
    public Text itemStatText;
    public Text itemTypeText;
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
        playerInventory = Player_Inventory.Instance;

        allTabButton.onClick.AddListener(() => ChangeTab(InventoryTabType.ALL));
        equipmentTabButton.onClick.AddListener(() => ChangeTab(InventoryTabType.EQUIPMENT));
        consumableTabButton.onClick.AddListener(() => ChangeTab(InventoryTabType.CONSUMABLE));
        etcTabButton.onClick.AddListener(() => ChangeTab(InventoryTabType.ETC));

        if (sortDropdown != null)
        {
            sortDropdown.onValueChanged.AddListener(OnSortChanged);
        }

        ChangeTab(InventoryTabType.ALL);
    }

    private void OnEnable()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

        if (playerInventory != null)
        {
            RefreshUI();
        }
    }

    public void ChangeTab(InventoryTabType newTab)
    {
        currentTab = newTab;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (playerInventory == null) return;

        List<ItemHolder> filteredItems = new List<ItemHolder>();
        List<int> originalIndices = new List<int>();

        for (int i = 0; i < playerInventory.inventorySlots.Count; i++)
        {
            ItemHolder item = playerInventory.inventorySlots[i];
            bool shouldShow = currentTab == InventoryTabType.ALL;

            if (!shouldShow && item.ItemData != null)
            {
                switch (currentTab)
                {
                    case InventoryTabType.EQUIPMENT: if (item.ItemData.itemType == ITEMTYPE.Equipment) shouldShow = true; break;
                    case InventoryTabType.CONSUMABLE: if (item.ItemData.itemType == ITEMTYPE.Consumable) shouldShow = true; break;
                    case InventoryTabType.ETC: if (item.ItemData.itemType == ITEMTYPE.ETC) shouldShow = true; break;
                }
            }

            if (shouldShow)
            {
                filteredItems.Add(item);
                originalIndices.Add(i); // 드래그 앤 드롭을 위해 원본 위치 기억
            }
        }

        //슬롯 부족하면 부족한만큼 생성
        while (slots.Count < filteredItems.Count)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotGridParent);
            UI_ItemSlot slot = slotGO.GetComponent<UI_ItemSlot>();
            slots.Add(slot);
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < filteredItems.Count)
            {
                // 가지고 있는 아이템이면 슬롯을 켜고 데이터를 넣음
                slots[i].gameObject.SetActive(true);
                slots[i].SetDraggable(enableDrag);
                slots[i].Initialize(SlotType.INVENTORY, originalIndices[i]);
                slots[i].Setup(filteredItems[i]);
            }
            else
            {
                // 안 쓰는 빈 슬롯은 화면에서 끔
                slots[i].gameObject.SetActive(false);
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
        
        if (detailPanel != null)
        {
            if (itemIcon != null) { itemIcon.sprite = itemHolder.ItemData.itemIcon; itemIcon.gameObject.SetActive(true); }
            if (itemName != null) itemName.text = itemHolder.ItemData.itemName;

            if (itemCount != null && itemCountImage != null)
            {
                if (itemHolder.ItemData.itemType == ITEMTYPE.Equipment)
                {
                    itemCount.gameObject.SetActive(false);
                    itemCountImage.gameObject.SetActive(false);
                }
                else
                {
                    itemCount.gameObject.SetActive(true);
                    itemCountImage.gameObject.SetActive(true);
                    itemCount.text = $"<b><color=white>×{itemHolder.Quantity}</color></b>";
                }
            }

            if (itemTypeText != null)
            {
                switch (itemHolder.ItemData.itemType)
                {
                    case ITEMTYPE.Equipment: itemTypeText.text = "장비"; break;
                    case ITEMTYPE.Consumable: itemTypeText.text = "소비품"; break;
                    case ITEMTYPE.Material: itemTypeText.text = "재료"; break;
                    case ITEMTYPE.Quest: itemTypeText.text = "퀘스트 아이템"; break;
                    case ITEMTYPE.ETC: itemTypeText.text = "기타"; break;
                    default: itemTypeText.text = "아이템"; break;
                }
            }

            if (itemHolder.ItemData.itemType == ITEMTYPE.Equipment)
            {
                if (itemStatText != null)
                {
                    itemStatText.gameObject.SetActive(true);

                    string statString = "";

                    int attack = itemHolder.GetTotalWeaponAttack();
                    if (attack > 0) statString += $"공격력 : {attack}\n";
                    // int defense = itemHolder.GetTotalWeaponDefense();
                    // if (defense > 0) statString += $"방어력 : {defense}\n";
                    // if (critical > 0) statString += $"치명타 : {critical}%\n";
                    itemStatText.text = statString.TrimEnd();
                }
            }
            else
            {
                if (itemStatText != null) itemStatText.gameObject.SetActive(false);
            }

            if (itemDescription != null) itemDescription.text = itemHolder.ItemData.itemDescription;
        }

        if (showDetailOnHover && detailPanel != null)
        {
            // 마우스 위치에서 오른쪽(+15), 아래(-15)로 살짝 띄우기
            Vector3 offset = new Vector3(15, -15, 0);
            detailPanel.transform.position = Input.mousePosition + offset;
        }
    }
}
