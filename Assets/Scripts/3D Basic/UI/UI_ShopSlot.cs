using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_ShopSlot : MonoBehaviour
{
    [Header("UI 요소")]
    public Image itemIcon;
    public Text itemNameText;
    public Text itemEffectText;
    public Text itemPriceText;

    private ShopItem currentShopItem;
    private NPC_Shop currentShop;
    private Button buyButton;

    public event Action<ShopItem> OnBuyButtonClicked;

    void Start()
    {
        // 구매 버튼이 클릭되면 OnClickBuy() 메소드 실행
        //buyButton.onClick.AddListener(OnClickBuy);
    }

    // NPC_Shop에서 호출하여 슬롯을 설정
    /*public void Setup(NPC_Shop shop, ShopItem item)
    {
        currentShop = shop;
        currentItem = item;

        if (item.itemData != null)
        {
            itemIcon.sprite = item.itemData.itemIcon;
            itemIcon.enabled = true;
            itemNameText.text = item.itemData.itemName;
            itemPriceText.text = $"{item.price} G"; // "G"는 골드 단위 예시
        }
        else
        {
            // (혹시 모를 빈 슬롯 처리)
            itemIcon.enabled = false;
            itemNameText.text = "";
            itemPriceText.text = "";
        }
    }*/
    public void Setup(ShopItem shopItem)
    {
        currentShopItem = shopItem;

        // UI 업데이트
        itemIcon.sprite = shopItem.itemData.itemIcon;
        itemNameText.text = shopItem.itemData.itemName;
        itemPriceText.text = $"{shopItem.price} G"; // 가격 표시 (G는 예시)
    }

    // 버튼이 클릭되거나 Enter 키로 선택되었을 때 호출
    /*private void OnSlotClicked()
    {
        if (currentItem != null && currentShop != null)
        {
            // 상점의 OnItemSelected 메서드 호출
            currentShop.OnItemSelected(currentItem);
        }
    }*/

    private void OnClickBuy()
    {
        Debug.Log($"{currentShopItem.itemData.itemName} 구매 시도");

        // OnBuyButtonClicked 이벤트를 구독(Listen)하고 있는 
        // 모든 스크립트(NPC_Shop)에게 currentShopItem 정보를 전달
        OnBuyButtonClicked?.Invoke(currentShopItem);
    }
}
