using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum ShopTab { Buy, Sell }

public class UI_ShopPanel : MonoBehaviour
{
    [Header("--- 데이터 설정 ---")]
    public List<ShopItem> itemsForSale; // 파는 물건 목록 (인스펙터에서 등록)
    public float sellPriceRatio = 0.5f; // 판매 시 가격 비율 (50%)

    [Header("--- 좌측: 리스트 영역 ---")]
    public Transform contentParent; // ScrollView의 Content
    public GameObject shopSlotPrefab; // UI_ShopSlot 프리팹

    [Header("--- 탭 설정 ---")]
    public Toggle toggleBuy;
    public Toggle toggleSell;
    public Image imgBuyBg;
    public Image imgSellBg;
    public Text txtBuy;
    public Text txtSell;
    public Color activeColor = Color.yellow;
    public Color inactiveColor = Color.gray;

    [Header("--- 우측: 상세 정보 및 거래 영역 ---")]
    public GameObject detailsGroup;
    public Image itemIcon;
    public Text itemName;
    public Text itemDesc;
    public Text itemPriceLabel; // "구매 가격" or "판매 가격"
    public Text itemPriceValue;

    [Header("--- 거래 조작 ---")]
    public Text txtQuantity; // 현재 선택 수량 표시 (예: "1")
    public Button btnPlus;
    public Button btnMinus;
    public Button btnAction; // "구매하기" or "판매하기" 버튼
    public Text txtBtnAction; // 버튼 텍스트

    // 내부 변수
    private ShopTab currentTab = ShopTab.Buy;
    private List<UI_ShopSlot> createdSlots = new List<UI_ShopSlot>();
    private Item_Base selectedItem;
    private int selectedItemPrice;
    private int currentQuantity = 1;
    private int maxQuantity = 99; // 최대 구매/판매 가능 수량

    private void OnEnable()
    {
        // 켜질 때 구매 탭으로 초기화
        currentTab = ShopTab.Buy;
        if (toggleBuy != null) toggleBuy.SetIsOnWithoutNotify(true);
        if (toggleSell != null) toggleSell.SetIsOnWithoutNotify(false);

        UpdateTabVisuals();
        RefreshList();
        ClearDetails();
    }

    // --- 탭 전환 ---
    public void OnTabChanged(bool isOn)
    {
        if (toggleBuy.isOn) currentTab = ShopTab.Buy;
        else currentTab = ShopTab.Sell;

        UpdateTabVisuals();
        RefreshList();
        ClearDetails();
    }

    void UpdateTabVisuals()
    {
        bool isBuy = (currentTab == ShopTab.Buy);

        if (imgBuyBg) imgBuyBg.color = isBuy ? activeColor : inactiveColor;
        if (txtBuy) txtBuy.color = isBuy ? activeColor : inactiveColor;

        if (imgSellBg) imgSellBg.color = !isBuy ? activeColor : inactiveColor;
        if (txtSell) txtSell.color = !isBuy ? activeColor : inactiveColor;
    }

    // --- 리스트 갱신 ---
    void RefreshList()
    {
        // 기존 슬롯 삭제
        foreach (Transform child in contentParent) Destroy(child.gameObject);
        createdSlots.Clear();

        if (currentTab == ShopTab.Buy)
        {
            // [구매 탭] 미리 등록된 판매 아이템 목록 표시
            foreach (var shopItem in itemsForSale)
            {
                CreateSlot(shopItem.itemData, shopItem.price);
            }
        }
        else
        {
            // [판매 탭] 내 인벤토리 아이템 표시
            if (Player_Inventory.instance != null)
            {
                foreach (var holder in Player_Inventory.instance.inventorySlots)
                {
                    if (holder != null && holder.ItemData != null)
                    {
                        // 상점 데이터에서 원래 가격 찾기
                        ShopItem original = itemsForSale.Find(x => x.itemData == holder.ItemData);
                        int basePrice = (original != null) ? original.price : 10; // 없으면 기본 10원
                        int sellPrice = Mathf.FloorToInt(basePrice * sellPriceRatio);

                        CreateSlot(holder.ItemData, sellPrice);
                    }
                }
            }
        }
    }

    void CreateSlot(Item_Base item, int price)
    {
        GameObject obj = Instantiate(shopSlotPrefab, contentParent);
        UI_ShopSlot slot = obj.GetComponent<UI_ShopSlot>();
        slot.Setup(item, price, OnSlotClicked);
        createdSlots.Add(slot);
    }

    // --- 슬롯 클릭 시 상세 정보 ---
    void OnSlotClicked(Item_Base item, int price)
    {
        foreach (var slot in createdSlots) slot.Deselect();

        selectedItem = item;
        selectedItemPrice = price;
        currentQuantity = 1;

        detailsGroup.SetActive(true);

        // 상세 정보 표시
        if (itemIcon) itemIcon.sprite = item.itemIcon;
        if (itemName) itemName.text = item.itemName;
        if (itemDesc) itemDesc.text = item.itemDescription;

        if (itemPriceLabel) itemPriceLabel.text = (currentTab == ShopTab.Buy) ? "개당 구매가" : "개당 판매가";
        if (itemPriceValue) itemPriceValue.text = $"{price} G";

        UpdateQuantityUI();
        UpdateActionButton();
    }

    // --- 수량 조절 버튼 ---
    public void OnClickPlus()
    {
        // 최대 수량 계산 (소지금, 재고 등에 따라 제한 가능)
        int limit = 99;

        if (currentTab == ShopTab.Buy)
        {
            // 가진 돈으로 살 수 있는 최대치
            int myGold = Player_Stat.globalInstance != null ? Player_Stat.globalInstance.gold : 0;
            if (selectedItemPrice > 0) limit = myGold / selectedItemPrice;
            if (limit > 99) limit = 99;
        }
        else
        {
            // 판매할 때는 가진 개수가 최대치
            // (간단히 인벤토리를 뒤져서 총 개수 확인)
            // 여기서는 복잡하니 일단 99로 두고, 실제 판매 시 검사
            if (Player_Inventory.instance != null)
            {
                // 현재 인벤토리에서 이 아이템의 총 개수를 찾아야 정확함 (생략 가능)
            }
        }

        if (currentQuantity < limit) currentQuantity++;
        UpdateQuantityUI();
    }

    public void OnClickMinus()
    {
        if (currentQuantity > 1) currentQuantity--;
        UpdateQuantityUI();
    }

    void UpdateQuantityUI()
    {
        if (txtQuantity) txtQuantity.text = currentQuantity.ToString();

        // 총 가격 계산해서 버튼 텍스트 업데이트
        int total = selectedItemPrice * currentQuantity;
        if (txtBtnAction)
        {
            string action = (currentTab == ShopTab.Buy) ? "구매하기" : "판매하기";
            txtBtnAction.text = $"{action} ({total} G)";
        }
    }

    void UpdateActionButton()
    {
        // 버튼 활성화/비활성화 (돈 부족 등)
        if (btnAction) btnAction.interactable = true;
    }

    // --- 거래 실행 버튼 ---
    public void OnClickAction()
    {
        if (selectedItem == null) return;

        if (currentTab == ShopTab.Buy)
        {
            BuyItem();
        }
        else
        {
            SellItem();
        }
    }

    void BuyItem()
    {
        int totalCost = selectedItemPrice * currentQuantity;
        Player_Stat playerStat = Player_Stat.globalInstance;

        if (playerStat == null) return;

        if (playerStat.gold >= totalCost)
        {
            if (Player_Inventory.instance.AddItem(selectedItem, currentQuantity))
            {
                playerStat.gold -= totalCost;
                Debug.Log($"구매 성공: {selectedItem.itemName} x{currentQuantity}");

                // UI 갱신 (돈 줄어든 거 반영)
                if (LobbyManager.instance != null) LobbyManager.instance.RefreshUserInfo();

                // 구매 후 수량 초기화
                currentQuantity = 1;
                UpdateQuantityUI();
            }
            else
            {
                Debug.Log("인벤토리 공간 부족");
            }
        }
        else
        {
            Debug.Log("골드 부족");
        }
    }

    void SellItem()
    {
        // 판매 로직: 인벤토리에서 아이템 제거 -> 골드 추가
        if (Player_Inventory.instance.RemoveItem(selectedItem, currentQuantity))
        {
            int totalGain = selectedItemPrice * currentQuantity;
            Player_Stat.globalInstance.gold += totalGain;

            Debug.Log($"판매 성공: {selectedItem.itemName} x{currentQuantity} (+{totalGain} G)");

            // UI 갱신
            if (LobbyManager.instance != null) LobbyManager.instance.RefreshUserInfo();

            // 판매 후 목록 갱신 (다 팔았으면 목록에서 사라져야 하니까)
            RefreshList();
            ClearDetails();
        }
        else
        {
            Debug.Log("판매 실패 (아이템 부족)");
        }
    }

    void ClearDetails()
    {
        if (detailsGroup) detailsGroup.SetActive(false);
        selectedItem = null;
    }
}