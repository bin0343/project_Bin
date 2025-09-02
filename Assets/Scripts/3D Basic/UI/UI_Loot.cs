using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Loot : MonoBehaviour
{
    public static UI_Loot Instance;
    public GameObject lootPanel;

    [Header("Loot UI Setup")]
    public int maxLootSlots = 8; // 루팅 창에 표시될 최대 슬롯 개수
    public GameObject lootSlotPrefab;
    public Transform slotParent; // Grid Layout Group이 있는 Content 오브젝트

    private List<UI_LootSlot> slots = new List<UI_LootSlot>();
    private ItemDrop currentLootSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
        InitializeSlots();
        lootPanel.SetActive(false); // 처음엔 비활성화
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < maxLootSlots; i++)
        {
            GameObject slotGO = Instantiate(lootSlotPrefab, slotParent);
            UI_LootSlot slot = slotGO.GetComponent<UI_LootSlot>();
            slots.Add(slot);
        }
    }

    public void OpenLootPanel(ItemDrop lootSource)
    {
        this.currentLootSource = lootSource;
        RefreshLootUI(); // UI 갱신

        if (!lootPanel.activeSelf)
        {
            UI_Manager.Instance.OpenUI(lootPanel);
        }
    }

    public void CloseLootPanel()
    {
        if (lootPanel.activeSelf)
        {
            UI_Manager.Instance.CloseSpecificUI(lootPanel);
        }
    }

    private void RefreshLootUI()
    {
        if (currentLootSource == null) return;

        List<Item_Base> drops = currentLootSource.GetLoot();

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].gameObject.SetActive(true);

            if (i < drops.Count)
            {
                slots[i].Setup(drops[i], this);
            }
            else
            {
                slots[i].Clear();
            }
        }
    }

    public void OnItemLooted(Item_Base item)
    {
        currentLootSource.RemoveLootedItem(item);

        RefreshLootUI();
    }
}
