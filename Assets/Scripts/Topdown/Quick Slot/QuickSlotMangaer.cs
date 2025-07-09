using UnityEngine;

public class QuickSlotManager : MonoBehaviour
{
    public static QuickSlotManager Instance;
    public QuickSlotUI[] quickSlots; // 인스펙터에서 슬롯들 등록

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        HandleQuickSlotInput();
    }

    void HandleQuickSlotInput()
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
            {
                Debug.Log($"{i}번 퀵슬롯 아이템을 사용하였습니다!");
                quickSlots[i].UseItem();
            }
        }
    }

    public void AssignItemToSlot(InventoryItem item, int targetSlotIndex)
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            if (quickSlots[i].GetAssignedItem() == item)
            {
                quickSlots[i].ClearSlot();
            }
        }
        quickSlots[targetSlotIndex].SetAssignedItem(item);
    }
}