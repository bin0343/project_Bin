using UnityEngine;
using UnityEngine.UI;

public class UI_EnterButton : MonoBehaviour
{
    [Header("이동 설정")]
    public GameObject nextPanel;

    void Start()
    {
        Image img = GetComponent<Image>();
        if (img != null) img.alphaHitTestMinimumThreshold = 0.5f;

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClickEnter);
        }
    }

    void OnClickEnter()
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
            }
        }
        else
        {
            Debug.LogWarning("Next Panel이 연결되지 않았습니다!");
        }
    }
}