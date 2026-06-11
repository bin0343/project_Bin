using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMapInteractiveIcon : MonoBehaviour, IPointerClickHandler
{
    [Header("아이콘 설명 (인스펙터에서 작성 가능)")]
    [TextArea]
    public string iconDescription;

    [Header("아이콘 그룹 설정")]
    public MapPinManager.IconGroupType iconGroup = MapPinManager.IconGroupType.General;

    [Header("텔레포트 설정 (Teleport 그룹일 때만 필수)")]
    [Tooltip("순간이동할 3D 목적지 게임오브젝트 이름을 정확히 적으세요.")]
    public string warpTargetName;

    private Vector2 myMapPos;

    public void Setup(string desc, Vector2 pos, MapPinManager.IconGroupType group = MapPinManager.IconGroupType.General)
    {
        iconDescription = desc;
        myMapPos = pos;
        iconGroup = group;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!eventData.dragging && MapPinManager.instance != null)
        {
            Sprite mySprite = GetComponent<Image>().sprite;
            if (myMapPos == Vector2.zero) myMapPos = GetComponent<RectTransform>().anchoredPosition;

            MapPinManager.instance.OpenDetailsPanel(iconDescription, mySprite, myMapPos, iconGroup, warpTargetName, null);
        }
    }
}