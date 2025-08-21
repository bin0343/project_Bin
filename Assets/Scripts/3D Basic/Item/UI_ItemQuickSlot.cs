using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemQuickSlot : MonoBehaviour
{
    public Image ItemIcon;
    public Image CooldownMask;
    public Text CooldownText;
    public Text QuantityText;

    private ItemHolder assignedItemHolder;

    public bool IsEmpty => assignedItemHolder == null;

    void Awake()
    {
        Clear();
    }

    public void Setup(ItemHolder itemHolder)
    {
        assignedItemHolder = itemHolder;
        ItemIcon.sprite = itemHolder.ItemData.itemIcon;
        ItemIcon.enabled = true;
        QuantityText.text = $"{itemHolder.ItemData.Quantity}";
        QuantityText.enabled = true;
        UpdateCooldownUI();
    }

    public void Clear()
    {
        assignedItemHolder = null;
        ItemIcon.sprite = null;
        ItemIcon.enabled = false;
        QuantityText.text = "";
        QuantityText.enabled = false;
        CooldownMask.fillAmount = 0;
        CooldownText.enabled = false;
    }

    void Update()
    {
        if (IsEmpty) return;
        UpdateCooldownUI();
    }

    private void UpdateCooldownUI()
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
    }
}
