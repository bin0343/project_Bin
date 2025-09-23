using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_LootSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Components")]
    public Image itemIcon;
    public Text itemNameText;

    private Item_Base displayedItem;
    private UI_Loot parentLootPanel;

    // 슬롯 초기화 및 UI 설정
    public void Setup(Item_Base item, UI_Loot parent)
    {
        this.displayedItem = item;
        this.parentLootPanel = parent;

        if (item != null)
        {
            itemIcon.sprite = item.itemIcon;
            itemNameText.text = item.itemName;
            itemIcon.gameObject.SetActive(true);
            itemNameText.gameObject.SetActive(true);
        }
        else
        {
            Clear();
        }
    }

    // 슬롯을 비우는 함수
    public void Clear()
    {
        displayedItem = null;
        itemIcon.gameObject.SetActive(false);
        itemNameText.gameObject.SetActive(false);
    }

    // 이 슬롯이 클릭되었을 때 호출되는 함수
    public void OnPointerClick(PointerEventData eventData)
    {
        // 아이템이 있고, '우클릭'을 했을 때만 작동
        if (displayedItem != null && eventData.button == PointerEventData.InputButton.Right)
        {
            // 1. 플레이어 인벤토리에 아이템 추가 시도
            bool success = Player_Inventory.instance.AddItem(displayedItem);

            // 2. 인벤토리에 성공적으로 추가되었다면
            if (success)
            {
                // 부모(UI_Loot)에게 내가 획득되었음을 알림
                parentLootPanel.OnItemLooted(displayedItem);
            }
        }
    }
}
