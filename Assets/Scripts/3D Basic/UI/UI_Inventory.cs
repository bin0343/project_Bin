using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
    public GameObject slotPrefab;   // 인벤토리 슬롯 프리팹
    public Transform slotGridParent;      // 슬롯들이 생성될 Grid Layout Group
    public UI_ItemSlot[] slots;

    void Start()
    {
        // Player_Inventory의 슬롯 개수에 맞춰 UI 슬롯 동적 생성
        int inventorySize = Player_Inventory.Instance.inventorySlots.Count;
        slots = new UI_ItemSlot[inventorySize];

        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotGridParent);
            slots[i] = slotGO.GetComponent<UI_ItemSlot>();
            slots[i].Initialize(SlotType.INVENTORY, i); // 슬롯 초기화
        }

        if(gameObject.activeSelf)
            gameObject.SetActive(false); // 처음엔 비활성화
    }

    // 인벤토리 창이 열릴 때 호출되어 UI를 최신 데이터로 갱신
    public void RefreshUI()
    {
        List<ItemHolder> inventoryData = Player_Inventory.Instance.inventorySlots;
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventoryData.Count && inventoryData[i] != null)
            {
                slots[i].Setup(inventoryData[i]);
            }
            else
            {
                slots[i].Clear();
            }
        }
    }
}
