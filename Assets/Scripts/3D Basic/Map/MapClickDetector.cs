using UnityEngine;
using UnityEngine.EventSystems;

public class MapClickDetector : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // 지도를 드래그해서 움직인 경우라면 핀 생성 취소
        if (eventData.dragging) return;

        // 클릭된 스크린 좌표를 Content 내부의 로컬 2D 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        // 매니저를 호출하여 해당 위치에 핀 생성 패널 열기
        if (MapPinManager.instance != null)
        {
            MapPinManager.instance.OpenCreatePanel(localPoint);
        }
    }
}