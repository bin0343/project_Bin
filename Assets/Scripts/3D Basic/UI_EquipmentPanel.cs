using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_EquipmentPanel : MonoBehaviour
{
    public UI_ItemSlot weaponSlot;

    private UI_ItemSlot[] equipmentSlotsUI;

    private void Start()
    {
        equipmentSlotsUI = new UI_ItemSlot[] { weaponSlot };

        weaponSlot.Initialize(SlotType.EQUIPMENT, (int)EquipmentType.Weapon);

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (Player_Equipment.instance == null) return;

        if (equipmentSlotsUI == null) return;

        for (int i = 0; i < equipmentSlotsUI.Length; i++)
        {
            if (equipmentSlotsUI[i] != null)
            {
                equipmentSlotsUI[i].gameObject.SetActive(true);
            }

            ItemHolder equippedItem = Player_Equipment.instance.equipmentSlots[i];
            if (equippedItem != null)
            {
                equipmentSlotsUI[i].Setup(equippedItem);
            }
            else
            {
                equipmentSlotsUI[i].Clear();
            }
        }
    }
}
