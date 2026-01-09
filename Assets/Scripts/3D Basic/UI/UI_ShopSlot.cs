using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_ShopSlot : MonoBehaviour
{
    [Header("UI 연결")]
    public Image iconImage;
    public Text nameText;
    public Text priceText;
    public GameObject selectHighlight; // 선택 강조 효과

    private Item_Base myItem;
    private int myPrice;
    private Action<Item_Base, int> onClickCallback; // 클릭 시 실행할 함수 (아이템, 가격 전달)

    public void Setup(Item_Base item, int price, Action<Item_Base, int> onClick)
    {
        myItem = item;
        myPrice = price;
        onClickCallback = onClick;

        if (iconImage != null) iconImage.sprite = item.itemIcon;
        if (nameText != null) nameText.text = item.itemName;
        if (priceText != null) priceText.text = $"{price} G";

        if (selectHighlight != null) selectHighlight.SetActive(false);
    }

    public void OnClickSlot()
    {
        onClickCallback?.Invoke(myItem, myPrice);
        if (selectHighlight != null) selectHighlight.SetActive(true);
        if (nameText != null && priceText != null) 
        {
            nameText.color = Color.black;
            priceText.color = Color.black;
        }
        
    }

    public void Deselect()
    {
        if (selectHighlight != null) selectHighlight.SetActive(false);
        if (nameText != null && priceText != null)
        {
            nameText.color = Color.white;
            priceText.color = Color.white;
        }
    }
}