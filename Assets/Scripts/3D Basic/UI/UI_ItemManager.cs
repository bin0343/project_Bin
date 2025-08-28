using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemManager : MonoBehaviour
{
    public static UI_ItemManager Instance { get; private set; }
    public UI_ItemSlot[] itemSlots;

    public string[] hotkeys = { "1", "2", "3", "4" };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i].Initialize(SlotType.QUICKSLOT, i);
        }

        SetHotkeys();
    }

    private void SetHotkeys()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            // 자식 오브젝트 중에서 "HotkeyText"를 찾아 Text 컴포넌트를 가져옵니다.
            Transform hotkeyTransform = itemSlots[i].transform.Find("HotkeyText");
            if (hotkeyTransform != null)
            {
                Text hotkeyText = hotkeyTransform.GetComponent<Text>();
                if (hotkeyText != null && i < hotkeys.Length)
                {
                    hotkeyText.text = hotkeys[i];
                }
            }
        }
    }

    // ADDED: Player_Action에서 호출하여 스킬 슬롯 전체를 초기화하는 메서드
    public void SetupItemSlots(ItemHolder[] playerItems)
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i < playerItems.Length && playerItems[i] != null)
            {
                itemSlots[i].Setup(playerItems[i]);
            }
            else
            {
                itemSlots[i].Clear();
            }
        }
    }

    // 아이템 사용 후 특정 슬롯만 업데이트하는 메서드 (나중에 최적화를 위해 사용)
    public void UpdateSlotUI(int slotIndex, ItemHolder itemHolder)
    {
        if (slotIndex < 0 || slotIndex >= itemSlots.Length) return;

        if (itemHolder != null)
        {
            itemSlots[slotIndex].Setup(itemHolder);
        }
        else
        {
            itemSlots[slotIndex].Clear();
        }
    }
}
