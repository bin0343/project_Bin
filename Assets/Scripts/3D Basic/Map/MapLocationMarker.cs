using UnityEngine;

public class MapLocationMarker : MonoBehaviour
{
    [Header("지도 표시 정보")]
    [Tooltip("지도에서 표시할 장소 이름")]
    public string locationName;

    [TextArea]
    [Tooltip("지도 아이콘을 눌렀을 때 표시할 설명")]
    public string description;

    [Tooltip("지도에서 사용할 아이콘")]
    public Sprite mapIcon;


    [Header("지도 표시 설정")]
    [Tooltip("전체 지도에 표시할지")]
    public bool showOnLocalMap = true;

    [Tooltip("미니맵에 표시할지")]
    public bool showOnMiniMap = true;

    [Header("발견 / 공개 상태")]
    [Tooltip("아직 공개되지 않은 장소면 지도와 미니맵에 표시하지 않음")]
    public bool isRevealed = true;


    [Header("텔레포트 설정")]
    [Tooltip("전체 지도에서 이 장소로 텔레포트할 수 있는지")]
    public bool canTeleport = false;

    [Tooltip("텔레포트할 실제 도착 위치. 비워두면 이 오브젝트 위치를 사용")]
    public Transform teleportTarget;


    public Transform GetTeleportTarget()
    {
        if (teleportTarget != null)
        {
            return teleportTarget;
        }

        return transform;
    }

    public void SetRevealed(bool revealed)
    {
        isRevealed = revealed;

        RefreshMapMarkers();
    }

    private void RefreshMapMarkers()
    {
        LocalMapLocationManager localMapManager = FindObjectOfType<LocalMapLocationManager>(true);

        if (localMapManager != null)
        {
            localMapManager.RefreshLocationMarkers();
        }

        MiniMapLocationManager miniMapManager = FindObjectOfType<MiniMapLocationManager>(true);

        if (miniMapManager != null)
        {
            miniMapManager.RefreshLocationMarkers();
        }
    }
}