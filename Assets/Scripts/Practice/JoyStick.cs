using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoyStick : MonoBehaviour
{
    public Image IMGBALL;
    
    Vector3 Input = Vector3.zero;
    Vector3 Position = Vector3.zero;

    public void OnDown(PointerEventData eventData)
    {
        IMGBALL.rectTransform.anchoredPosition = Vector3.zero;
    }

    public void OnUp(PointerEventData eventData)
    {
        Input = Vector3.zero;
        IMGBALL.rectTransform.anchoredPosition = Vector3.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(IMGBALL.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localpoint))
        {
            localpoint.x = localpoint.x / IMGBALL.rectTransform.sizeDelta.x;
            localpoint.y = localpoint.y / IMGBALL.rectTransform.sizeDelta.y;

            Input.x = localpoint.x;
            Input.y = localpoint.y;

            Input = (Input.magnitude > 1.0f) ? Input.normalized : Input;

            Position.x = Input.x * IMGBALL.rectTransform.sizeDelta.x / 2f;
            Position.z = Input.y * IMGBALL.rectTransform.sizeDelta.y / 2f;

            Vector2 vec = new Vector2(Position.x, Position.z);

            IMGBALL.rectTransform.anchoredPosition = vec;

            //캐릭터 이동
        }
    }
}
