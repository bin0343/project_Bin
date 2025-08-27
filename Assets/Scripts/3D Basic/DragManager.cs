using UnityEngine;
using UnityEngine.UI;

public class DragManager : MonoBehaviour
{
    public Image dragIcon; // 인스펙터에서 DragIcon 오브젝트를 연결

    void Awake()
    {
        // 게임 시작 시 DragSlot의 정적 변수에 고스트 아이콘을 할당
        DragSlot.dragIcon = dragIcon;
    }
}
