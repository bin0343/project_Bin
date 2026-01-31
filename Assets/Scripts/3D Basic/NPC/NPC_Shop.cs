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
    [Tooltip("판매 UI에 생성될 아이템 슬롯 프리팹")]
    public GameObject sellSlotPrefab;
    [Tooltip("판매 슬롯들이 생성될 부모 Transform")]
    public Transform sellSlotParent;
    [Tooltip("아이템 판매 시 가격 비율 (0.5 = 50%)")]
    public float sellPriceRatio = 0.5f;

    [Header("상점 버튼")]
    public Button buyButton;
    public Button sellButton;
    public Button closeButton;

    [Header("팝업 UI")]
    [Tooltip("수량 선택 팝업 패널")]
    public GameObject quantityPopupPanel;
    public UI_QuantityPopup quantityPopup;
    /*[Tooltip("최종 구매 확인 팝업 패널")]
    public GameObject confirmPopupPanel;*/

    private Player_Stat playerStat;

    private List<UI_ShopSlot> createdSlots = new List<UI_ShopSlot>();   //생성 슬롯 관리 리스트
    private List<UI_ShopSlot> createdSellSlots = new List<UI_ShopSlot>();

    private enum ShopState { Browsing, SelectingQuantity}
    private ShopState currentState;

    private ShopItem currentSelectedItem;
    private ItemHolder currentSelectedItemHolder;
    private GameObject lastSelectedSlot;

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

        if (quantityPopupPanel != null) quantityPopupPanel.SetActive(false);

        if (Player_Inventory.instance != null)
        {
            playerStat = Player_Inventory.instance.GetComponent<Player_Stat>();
        }
    }

    /*void Update()
    {
        //if (!isMenuOpen) return;

        // 아이템 목록(Browsing)에서 Esc -> 구매/판매 메뉴로
        if (currentState == ShopState.Browsing)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (buyMenuPanel.activeSelf)
                {
                    ReturnToShopMenu(buyMenuPanel);
                }
                else if (sellMenuPanel.activeSelf)
                {
                    ReturnToShopMenu(sellMenuPanel);
                }
            }
        }
    }*/

    // 상점 아이템 리스트를 UI에 생성하는 새 메소드
    private void InitializeShopUI()
    {
        // 혹시 모를 기존 슬롯들 삭제 (필요에 따라)
        foreach (Transform child in buySlotParent)
        {
            Destroy(child.gameObject);
        }

        createdSlots.Clear();

        for (int i = 0; i < sellingItems.Count; i++)
        {
            GameObject slotInstance = Instantiate(buySlotPrefab, buySlotParent);
            var slotUI = slotInstance.GetComponent<UI_ShopSlot>();

            if (slotUI != null)
            {
                // 1. 슬롯 데이터 설정 및 클릭 이벤트 연결
                // "이 슬롯이 클릭되면, HandleSlotSelection 함수를 이 아이템 정보와 함께 호출해라"
                //slotUI.Setup(sellingItems[i], HandleSlotSelection);
                createdSlots.Add(slotUI);
            }
        }

        // 2. 슬롯 간 네비게이션 설정
        SetupSlotNavigation();
    }

    private void SetupSlotNavigation()
    {
        for (int i = 0; i < createdSlots.Count; i++)
        {
            Button button = createdSlots[i].GetComponent<Button>();
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit; // 네비게이션 수동 설정

            // 위쪽 (i-1), 아래쪽 (i+1)
            // (첫 번째 슬롯의 '위'는 마지막 슬롯으로 - Wrap-around)
            nav.selectOnUp = createdSlots[(i - 1 + createdSlots.Count) % createdSlots.Count].GetComponent<Button>();
            // (마지막 슬롯의 '아래'는 첫 번째 슬롯으로 - Wrap-around)
            nav.selectOnDown = createdSlots[(i + 1) % createdSlots.Count].GetComponent<Button>();

            button.navigation = nav;
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
        UI_Manager.instance.OpenUI(buyMenuPanel);
        //buyMenuPanel.SetActive(true);
        shopMenuPanel.SetActive(false);

        currentState = ShopState.Browsing;

        if (createdSlots.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(createdSlots[0].gameObject);
        }
    }

    public void OnSell()
    {
        Debug.Log("판매 창을 엽니다.");
        PopulateSellList();
        UI_Manager.instance.OpenUI(sellMenuPanel);
        shopMenuPanel.SetActive(false);

        currentState = ShopState.Browsing;

        if (createdSellSlots.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(createdSellSlots[0].gameObject);
        }
    }

    private void PopulateSellList()
    {
        foreach (Transform child in sellSlotParent)
        {
            Destroy(child.gameObject);
        }
        createdSellSlots.Clear();

        foreach (ItemHolder playerItemHolder in Player_Inventory.instance.inventorySlots)
        {
            if (playerItemHolder == null || playerItemHolder.ItemData == null) continue;

            ShopItem shopData = sellingItems.Find(s => s.itemData == playerItemHolder.ItemData);

            int sellPrice = (shopData != null) ? Mathf.FloorToInt(shopData.price * sellPriceRatio) : 0;
            if (sellPrice <= 0) continue;

            GameObject slotInstance = Instantiate(sellSlotPrefab, sellSlotParent);
            var slotUI = slotInstance.GetComponent<UI_ShopSlot>();

            ShopItem displayItem = new ShopItem { itemData = playerItemHolder.ItemData, price = sellPrice };

            if (slotUI != null)
            {
                /*slotUI.Setup(displayItem, (clickedShopItem) => {
                    // (clickedShopItem은 무시)
                    HandleSellSlotSelection(playerItemHolder);
                });*/
                createdSellSlots.Add(slotUI);
            }
        }

        SetupSellSlotNavigation();
    }

    private void SetupSellSlotNavigation()
    {
        for (int i = 0; i < createdSellSlots.Count; i++)
        {
            Button button = createdSellSlots[i].GetComponent<Button>();
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = createdSellSlots[(i - 1 + createdSellSlots.Count) % createdSellSlots.Count].GetComponent<Button>();
            nav.selectOnDown = createdSellSlots[(i + 1) % createdSellSlots.Count].GetComponent<Button>();
            button.navigation = nav;
        }
    }

    private void HandleSellSlotSelection(ItemHolder itemHolderToSell)
    {
        if (currentState != ShopState.Browsing) return;

        // 판매할 아이템 홀더를 멤버 변수에 저장
        currentSelectedItemHolder = itemHolderToSell;
        lastSelectedSlot = EventSystem.current.currentSelectedGameObject;

        // 수량 선택 팝업을 띄움
        ShowSellQuantityPopup(itemHolderToSell);
    }

    private void ShowSellQuantityPopup(ItemHolder item)
    {
        // 판매 가격 다시 계산 (확인용)
        ShopItem shopData = sellingItems.Find(s => s.itemData == item.ItemData);
        int sellPrice = (shopData != null) ? Mathf.FloorToInt(shopData.price * sellPriceRatio) : 0;

        // UI_QuantityPopup에 넘겨줄 임시 ShopItem 생성 (가격 표시용)
        ShopItem displayItem = new ShopItem { itemData = item.ItemData, price = sellPrice };

        // 판매 가능한 최대 수량 = 플레이어가 가진 수량
        int maxSellable = item.Quantity;

        currentState = ShopState.SelectingQuantity;
        UI_Manager.instance.CloseSpecificUI(sellMenuPanel); // 판매창 닫기
        UI_Manager.instance.OpenUI(quantityPopup.gameObject); // 팝업 열기

        // [중요] 팝업의 '확인' 콜백으로 OnSellQuantityConfirmed를 연결
        quantityPopup.Initialize(displayItem, maxSellable, OnSellQuantityConfirmed, ReturnToSelling);
    }

    private void OnSellQuantityConfirmed(ShopItem displayItem, int quantityToSell)
    {
        // displayItem은 무시 (표시용이었음)
        // 멤버 변수에 저장해둔 currentSelectedItemHolder를 사용
        ItemHolder itemToSell = currentSelectedItemHolder;

        if (itemToSell == null)
        {
            ReturnToSelling();
            return;
        }

        // 1. 인벤토리에서 아이템 제거 시도
        bool success = Player_Inventory.instance.RemoveItem(itemToSell.ItemData, quantityToSell);

        if (success)
        {
            // 2. 판매 가격 계산
            ShopItem shopData = sellingItems.Find(s => s.itemData == itemToSell.ItemData);
            int sellPrice = (shopData != null) ? Mathf.FloorToInt(shopData.price * sellPriceRatio) : 0;
            int totalGain = sellPrice * quantityToSell;

            // 3. 골드 추가
            playerStat.gold += totalGain;
            UI_Manager.instance.ShowMessage($"{itemToSell.ItemData.itemName} {quantityToSell}개 판매 완료. (+{totalGain} G)");
            // TODO: 골드 UI 갱신
        }
        else
        {
            UI_Manager.instance.ShowMessage("아이템 판매에 실패했습니다.");
        }

        ReturnToSelling();
    }

    // (신규) 판매 목록으로 돌아가기
    public void ReturnToSelling()
    {
        currentState = ShopState.Browsing;

        // 판매 목록을 다시 연다
        UI_Manager.instance.OpenUI(sellMenuPanel);

        // [중요] 판매 후 인벤토리 수량이 변경되었으므로 목록을 새로고침
        PopulateSellList();

        // 포커스 설정 (lastSelectedSlot은 아이템이 사라지면 null이 될 수 있으므로, 그냥 첫 번째 슬롯 선택)
        if (createdSellSlots.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(createdSellSlots[0].gameObject);
        }
        else
        {
            // 판매할 아이템이 더 없으면, '판매/구매' 메뉴로 복귀
            ReturnToShopMenu(sellMenuPanel);
        }
    }

    // (수정) ReturnToShopMenu가 어떤 패널을 닫을지 인자를 받도록 수정
    private void ReturnToShopMenu(GameObject panelToClose)
    {
        currentState = ShopState.Browsing;
        UI_Manager.instance.CloseSpecificUI(panelToClose);
        UI_Manager.instance.OpenUI(shopMenuPanel);

        EventSystem.current.SetSelectedGameObject(buyButton.gameObject);
    }

    private void HandleSlotSelection(ShopItem selectedItem)
    {
        if (currentState != ShopState.Browsing) return;

        lastSelectedSlot = EventSystem.current.currentSelectedGameObject;
        currentSelectedItem = selectedItem;

        Debug.Log($"{selectedItem.itemData.itemName} 선택됨. 수량 팝업 열기.");

        ShowQuantityPopup(selectedItem);
    }

    private void ShowQuantityPopup(ShopItem item)
    {
        if (playerStat == null)
        {
            Debug.LogError("Player_Stat 참조를 찾을 수 없습니다.");
            return;
        }

        int currentGold = playerStat.gold;
        int maxAffordable = 99;

        if (item.price > 0)
        {
            // 1개 이상 살 수 있는 골드가 있는지 확인
            if (currentGold < item.price)
            {
                UI_Manager.instance.ShowMessage("골드가 부족합니다.");
                return; 
            }
            
            maxAffordable = currentGold / item.price;
        }

        // 최종 최대 수량 = (99개)와 (살 수 있는 개수) 중 더 적은 값
        int maxBuyable = Mathf.Min(99, maxAffordable);

        currentState = ShopState.SelectingQuantity;
        UI_Manager.instance.CloseSpecificUI(buyMenuPanel); // 구매창 닫기 (UI스택)

        UI_Manager.instance.OpenUI(quantityPopup.gameObject);
        quantityPopup.Initialize(item, maxBuyable, OnQuantityConfirmed, ReturnToBrowsing);
        /*currentState = ShopState.SelectingQuantity;
        buyMenuPanel.SetActive(false);

        quantityPopup.gameObject.SetActive(true);
        quantityPopup.Initialize(item, OnQuantityConfirmed, ReturnToBrowsing);*/
    }

    private void OnQuantityConfirmed(ShopItem item, int quantity)
    {
        Debug.Log($"수량 {quantity}개 확인. 최종 확인 팝업 열기.");
        FinalPurchase(item, quantity);
    }

    private void FinalPurchase(ShopItem item, int quantity) //구매 확인을 눌렀을 때
    {
        if (playerStat == null)
        {
            Debug.LogError("Player_Stat 참조가 없습니다. 구매 실패.");
            ReturnToBrowsing();
            return;
        }

        int totalPrice = item.price * quantity;

        // (Initialize에서 이미 검사했지만) 한 번 더 방어적 검사
        if (playerStat.gold < totalPrice)
        {
            UI_Manager.instance.ShowMessage("골드가 부족합니다.");
            ReturnToBrowsing();
            return;
        }

        // 1. 인벤토리에 아이템 추가 시도
        bool success = Player_Inventory.instance.AddItem(item.itemData, quantity);

        // 2. 인벤토리 추가에 성공했을 때만 골드 차감
        if (success)
        {
            playerStat.gold -= totalPrice; // 골드 차감
            UI_Manager.instance.ShowMessage($"{item.itemData.itemName} {quantity}개 구매 완료.");
            // TODO: 골드 UI 갱신 (예: UI_Manager.Instance.UpdateGoldUI())
        }
        else
        {
            UI_Manager.instance.ShowMessage("인벤토리가 가득 찼습니다.");
        }

        ReturnToBrowsing();
    }

    public void ReturnToBrowsing()
    {
        currentState = ShopState.Browsing;
        //quantityPopupPanel.SetActive(false);

        UI_Manager.instance.OpenUI(buyMenuPanel);
        //buyMenuPanel.SetActive(true);

        if (lastSelectedSlot != null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedSlot);
        }
        else if (createdSlots.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(createdSlots[0].gameObject);
        }
    }

    private void ReturnToShopMenu()
    {
        currentState = ShopState.Browsing;
        UI_Manager.instance.CloseSpecificUI(buyMenuPanel);
        UI_Manager.instance.OpenUI(shopMenuPanel);

        EventSystem.current.SetSelectedGameObject(buyButton.gameObject);
    }
}
