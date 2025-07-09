using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public GameObject slotGrid;
    public Text GoldText;
    public Transform slotParent;
    private List<InventorySlotUI> slots = new List<InventorySlotUI>();

    private Inventory playerInventory;
    private PlayerStat stat;

    private void Start()
    {
        stat = FindObjectOfType<PlayerStat>();
        foreach (Transform child in slotGrid.transform)
        {
            Destroy(child.gameObject);
        }
        playerInventory = FindObjectOfType<Inventory>();
        playerInventory.onInventoryChanged += UpdateUI;

        for (int i = 0; i < playerInventory.maxSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotParent);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            slots.Add(slotUI);
        }

        Debug.Log("InventoryUI Start called");
        UpdateUI();
        UpdateGold();
    }

    public void UpdateGold()
    {
        if (stat != null && GoldText != null)
        {
            GoldText.text = $"Gold: {stat.Gold}";
        }
    }

    public void UpdateUI()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < playerInventory.items.Count)
            {
                slots[i].SetItem(playerInventory.items[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }
}