using UnityEngine;

[System.Serializable]
public class QuickSlotHolder : MonoBehaviour
{
    public Item_Base linkedItemData;

    // 쿨타임 등 퀵슬롯 자체의 상태는 여기에 계속 저장할 수 있습니다.
    private float lastUseTime;

    public QuickSlotHolder()
    {
        linkedItemData = null;
    }
}
