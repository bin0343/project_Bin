using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMapInteractiveIcon : MonoBehaviour, IPointerClickHandler
{
    [Header("아이콘 설명 (인스펙터에서 작성 가능)")]
    [TextArea]
    public string iconDescription;

    private Vector2 myMapPos;

    public void Setup(string desc, Vector2 pos)
    {
        iconDescription = desc;
        myMapPos = pos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 맵을 드래그하는 중이 아니라면 클릭으로 인정
        if (!eventData.dragging && MapPinManager.instance != null)
        {
            Sprite mySprite = GetComponent<Image>().sprite;

            // 만약 Setup으로 좌표를 안 받았다면, 현재 내 UI 좌표를 그대로 사용
            if (myMapPos == Vector2.zero)
            {
                myMapPos = GetComponent<RectTransform>().anchoredPosition;
            }

            // 매니저에게 상세 패널 열기 요청
            MapPinManager.instance.OpenDetailsPanel(iconDescription, mySprite, myMapPos);
        }
    }
}