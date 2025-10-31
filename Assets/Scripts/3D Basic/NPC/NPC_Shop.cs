using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[System.Serializable]
public class ShopItem
{
    public Item_Base itemData;
    public int price;
}

public class NPC_Shop : Interactable
{
    [Header("상점 NPC 전용")]
    public GameObject shopMenuPanel;
    public GameObject buyMenuPanel;
    public GameObject sellMenuPanel;

    [Header("판매 아이템 목록")]
    public List<ShopItem> sellingItems;

    [Header("UI 프리팹 및 부모")]
    [Tooltip("구매 UI에 생성될 아이템 슬롯 프리팹")]
    public GameObject buySlotPrefab;
    [Tooltip("구매 슬롯들이 생성될 부모 Transform (Vertical Layout Group이 있는 곳)")]
    public Transform buySlotParent;

    [Header("상점 버튼")]
    public Button buyButton;
    public Button sellButton;
    public Button closeButton;

    void Start()
    {
        if (shopMenuPanel != null)
        {
            shopMenuPanel.SetActive(false);
        }

        if (buyButton != null) buyButton.onClick.AddListener(OnBuy);
        if (sellButton != null) sellButton.onClick.AddListener(OnSell);
        if (closeButton != null) closeButton.onClick.AddListener(Closemenu);

        InitializeShopUI();
    }

    // 상점 아이템 리스트를 UI에 생성하는 새 메소드
    private void InitializeShopUI()
    {
        // 혹시 모를 기존 슬롯들 삭제 (필요에 따라)
        foreach (Transform child in buySlotParent)
        {
            Destroy(child.gameObject);
        }

        // 판매 목록(sellingItems)을 순회하며 UI 슬롯 생성
        foreach (ShopItem item in sellingItems)
        {
            // 1. 프리팹 생성
            GameObject slotInstance = Instantiate(buySlotPrefab, buySlotParent);

            // 2. 스크립트 가져오기
            var slotUI = slotInstance.GetComponent<UI_ShopSlot>();
            if (slotUI != null)
            {
                // 3. 슬롯 데이터 설정
                slotUI.Setup(item);

                // 4. 슬롯의 구매 버튼 클릭 이벤트에 'HandleBuyRequest' 메소드를 구독(연결)
                slotUI.OnBuyButtonClicked += HandleBuyRequest;
            }
        }
    }

    // 아이템 구매 요청 처리 (ShopSlot_UI가 호출)
    private void HandleBuyRequest(ShopItem itemToBuy)
    {
        Debug.Log($"플레이어가 {itemToBuy.itemData.itemName}을(를) {itemToBuy.price} G에 구매하려고 합니다.");
    }

    protected override void OpenMenu()
    {
        base.OpenMenu(shopMenuPanel);
        EventSystem.current.SetSelectedGameObject(buyButton.gameObject);
    }

    public void OnBuy()
    {
        Debug.Log("구매 창을 엽니다.");
        //PopulateBuyList();
        UI_Manager.Instance.OpenUI(buyMenuPanel);
        //buyMenuPanel.SetActive(true);
        shopMenuPanel.SetActive(false);
    }

    public void OnSell()
    {
        Debug.Log("판매 창을 엽니다.");
        UI_Manager.Instance.OpenUI(sellMenuPanel);
        //sellMenuPanel.SetActive(true);
        shopMenuPanel.SetActive(false);
    }
    
}
