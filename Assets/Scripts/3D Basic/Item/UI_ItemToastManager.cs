using UnityEngine;

public class UI_ItemToastManager : MonoBehaviour
{
    public static UI_ItemToastManager instance;

    [Header("UI 셋업")]
    public GameObject toastPrefab; // 방금 만든 UI_ItemToast 프리팹
    public Transform toastParent;  // 팝업들이 생성될 위치 (Vertical Layout Group)

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void ShowToast(Item_Base item, int amount)
    {
        if (item == null || toastPrefab == null || toastParent == null) return;

        // 프리팹 생성 후 내용물 채우기
        GameObject go = Instantiate(toastPrefab, toastParent);
        UI_ItemToast toastScript = go.GetComponent<UI_ItemToast>();
        if (toastScript != null)
        {
            toastScript.Setup(item, amount);
        }
    }
}