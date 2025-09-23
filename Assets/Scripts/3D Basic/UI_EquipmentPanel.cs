using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_EquipmentPanel : MonoBehaviour
{
    public UI_ItemSlot leftHandSlot;
    public UI_ItemSlot rightHandSlot;
    public UI_ItemSlot armorSlot;

    private UI_ItemSlot[] equipmentSlotsUI;

    private void Start()
    {
        equipmentSlotsUI = new UI_ItemSlot[] { leftHandSlot, rightHandSlot, armorSlot };

        leftHandSlot.Initialize(SlotType.EQUIPMENT, (int)EquipmentType.LeftHand);
        rightHandSlot.Initialize(SlotType.EQUIPMENT, (int)EquipmentType.RightHand);
        armorSlot.Initialize(SlotType.EQUIPMENT, (int)EquipmentType.Armor);

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (Player_Equipment.instance == null) return;

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
