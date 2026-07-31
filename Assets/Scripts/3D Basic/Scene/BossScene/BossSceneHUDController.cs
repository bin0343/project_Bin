using UnityEngine;

public class BossSceneHUDController : MonoBehaviour
{
    private bool minimapWasHidden;

    private void Start()
    {
        if (UI_Manager.instance == null)
        {
            return;
        }

        UI_Manager.instance.SetMinimapVisible(false);
        minimapWasHidden = true;
    }

    private void OnDestroy()
    {
        if (!minimapWasHidden)
        {
            return;
        }

        if (UI_Manager.instance != null)
        {
            UI_Manager.instance.SetMinimapVisible(true);
        }
    }
}
