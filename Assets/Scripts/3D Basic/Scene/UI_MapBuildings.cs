using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_MapBuilding : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("이동 설정")]
    public GameObject nextPanel;
    public GameObject currentPanel;

    [Header("UI 설정")]
    public GameObject nameTagObj;
    private Image myImage;

    [HideInInspector] public UI_MapPin linkedPin;

    void Start()
    {
        myImage = GetComponent<Image>();

        if (nameTagObj != null) nameTagObj.SetActive(false);

        if (myImage != null) myImage.alphaHitTestMinimumThreshold = 0.5f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (nameTagObj != null) nameTagObj.SetActive(true);
        
        if (linkedPin != null) linkedPin.SetHover(true);
        Debug.Log("핀 확인");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (nameTagObj != null) nameTagObj.SetActive(false);
        
        if (linkedPin != null) linkedPin.SetHover(false);
        Debug.Log("핀 아웃");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (nextPanel != null)
        {
            if (LobbyManager.instance != null)
            {
                LobbyManager.instance.OpenDepthPanel(nextPanel);
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