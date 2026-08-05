using UnityEngine;

public class BossSceneHUDController : MonoBehaviour
{
    private bool minimapWasHidden;

    private void Start()
    {
        if (UI_Manager.Instance == null)
        {
            return;
        }

        UI_Manager.Instance.SetMinimapVisible(false);
        minimapWasHidden = true;
    }

    private void OnDestroy()
    {
        if (!minimapWasHidden)
        {
            return;
        }

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.SetMinimapVisible(true);
        }
    }
}
