using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_MapBuilding : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("건물 ID")]
    public int buildingID;

    [Header("이동 설정")]
    public GameObject nextPanel;
    public GameObject currentPanel;

    [Header("UI 설정")]
    public GameObject nameTagObj;
    private Image myImage;

    [HideInInspector] public UI_MapPin linkedPin;

    private IntroManager introManager;

    void Start()
    {
        myImage = GetComponent<Image>();
        introManager = FindObjectOfType<IntroManager>();

        if (nameTagObj != null) nameTagObj.SetActive(false);

        if (myImage != null) myImage.alphaHitTestMinimumThreshold = 0.5f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (nameTagObj != null) nameTagObj.SetActive(true);
        
        if (linkedPin != null) linkedPin.SetHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (nameTagObj != null) nameTagObj.SetActive(false);
        
        if (linkedPin != null) linkedPin.SetHover(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (introManager != null)
        {
            // 만약 CheckBuildingClick이 false를 반환하면(클릭 금지), 여기서 함수 종료!
            if (!introManager.CheckBuildingClick(buildingID))
            {
                return;
            }
        }

        if (nextPanel != null)
        {
            if (LobbyManager.instance != null)
            {
                LobbyManager.instance.StartFadeEffect(() => {
                    LobbyManager.instance.OpenDepthPanel(nextPanel);
                }, 1.0f);
            }
            else
            {
                nextPanel.SetActive(true);
                if (currentPanel != null) currentPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("이동할 Next Panel이 연결되지 않았습니다!");
        }
    }
}