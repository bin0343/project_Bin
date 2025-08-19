using UnityEngine;
using System.Collections;

public class UI_QuickSlotManager : MonoBehaviour
{
    public UI_QuickSlot[] slots;
    private int selectedIndex = 0;
    private Vector3 normalScale = Vector3.one;
    private Vector3 pressedScale = new Vector3(0.9f, 0.9f, 1f);
    private float animDuration = 0.1f;

    void Awake()
    {
        slots = GetComponentsInChildren<UI_QuickSlot>();
    }

    void Start()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].slotNumber != null)
                slots[i].slotNumber.text = (i + 1).ToString();
        }
    }

    void Update()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedIndex = i;
                StartCoroutine(PressAnimation(slots[i].transform));
            }
        }
    }

    IEnumerator PressAnimation(Transform slotTransform)
    {
        float t = 0f;
        Vector3 start = normalScale;
        Vector3 end = pressedScale;

        // 크기 줄이기
        while (t < 1f)
        {
            t += Time.deltaTime / animDuration;
            slotTransform.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }

        t = 0f;

        // 크기 다시 원상복구
        while (t < 1f)
        {
            t += Time.deltaTime / animDuration;
            slotTransform.localScale = Vector3.Lerp(end, start, t);
            yield return null;
        }
    }
}
