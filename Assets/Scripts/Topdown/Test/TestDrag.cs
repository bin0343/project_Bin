using UnityEngine;
using UnityEngine.EventSystems;

public class TestDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("TestDrag: BeginDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("TestDrag: Dragging...");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("TestDrag: EndDrag");
    }
}