using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemQuickSlot : MonoBehaviour
{
    public Image ItemIcon;
    //public Image CooldownMask;
    //public Text CooldownText;
    public Text QuantityText;

    private ItemHolder assignedItemHolder;

    public bool IsEmpty => assignedItemHolder == null;

    void Awake()
    {
        Clear();
    }

    public void Setup(ItemHolder itemHolder)
    {
        /*assignedItemHolder = itemHolder;
        ItemIcon.sprite = itemHolder.ItemData.itemIcon;
        ItemIcon.enabled = true;

        // CHANGED: ItemData가 아닌 itemHolder에서 직접 quantity를 가져옵니다.
        QuantityText.text = $"{itemHolder.Quantity}";
        QuantityText.enabled = true;
        // UpdateCooldownUI(); // 쿨타임 기능 없으므로 주석 처리*/

        if (itemHolder == null || itemHolder.ItemData == null)
        {
            Clear();
            return;
        }

        assignedItemHolder = itemHolder;
        ItemIcon.sprite = itemHolder.ItemData.itemIcon;
        ItemIcon.enabled = true;

        if (itemHolder.Quantity > 1)
        {
            QuantityText.text = $"{itemHolder.Quantity}";
            QuantityText.enabled = true;
        }
        else
        {
            QuantityText.text = "";
            QuantityText.enabled = false;
        }
    }

    public void Clear()
    {
        assignedItemHolder = null;
        ItemIcon.sprite = null;
        ItemIcon.enabled = false;
        QuantityText.text = "";
        QuantityText.enabled = false;
        // CooldownMask.fillAmount = 0; // 쿨타임 기능 없으므로 주석 처리
        // CooldownText.enabled = false;
    }

    void Update()
    {
        
    }

    /*private void UpdateCooldownUI()
    {
        float remaining = assignedItemHolder.GetRemainingCooldown();
        float totalCooldown = assignedItemHolder.ItemData.cooldownTime;

        if (remaining > 0)
        {
            CooldownMask.fillAmount = remaining / totalCooldown;
            CooldownText.enabled = true;
            CooldownText.text = remaining.ToString("F1");
        }
        else
        {
            CooldownMask.fillAmount = 0f;
            CooldownText.enabled = false;
        }
    }*/
}
