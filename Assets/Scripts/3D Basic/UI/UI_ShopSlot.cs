using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button))]
public class UI_ShopSlot : MonoBehaviour
{
    [Header("UI 요소")]
    public Image itemIcon;
    public Text itemNameText;
    public Text itemEffectText;
    public Text itemPriceText;

    public ShopItem currentShopItem { get; private set; }

    public Button selfButton;

    //public event Action<ShopItem> OnBuyButtonClicked;

    /*void Awake()
    {
        selfButton = GetComponent<Button>();
    }*/
    
    public void Setup(ShopItem shopItem, Action<ShopItem> onClickAction) 
    {
        currentShopItem = shopItem;

        // UI 업데이트
        itemIcon.sprite = shopItem.itemData.itemIcon;
        itemNameText.text = shopItem.itemData.itemName;
        itemPriceText.text = $"{shopItem.price} G"; // 가격 표시 (G는 예시)

        selfButton.onClick.RemoveAllListeners();
        selfButton.onClick.AddListener(() => onClickAction(currentShopItem));
    }

    /*private void OnClickBuy()
    {
        Debug.Log($"{currentShopItem.itemData.itemName} 구매 시도");

        // OnBuyButtonClicked 이벤트를 구독(Listen)하고 있는 
        // 모든 스크립트(NPC_Shop)에게 currentShopItem 정보를 전달
        OnBuyButtonClicked?.Invoke(currentShopItem);
    }*/
}
