using UnityEngine;

public class BossExitUI : MonoBehaviour
{
    [SerializeField]
    private GameObject exitButton;

    [SerializeField]
    private GameObject confirmPanel;

    private string exitSceneName;
    private string exitSpawnPointId;

    private bool hasExitDestination;

    private void Awake()
    {
        SetVisible(false);

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    public void RegisterExitDestination(string sceneName, string spawnPointId)
    {
        exitSceneName = sceneName;
        exitSpawnPointId = spawnPointId;

        hasExitDestination = !string.IsNullOrEmpty(exitSceneName) && !string.IsNullOrEmpty(exitSpawnPointId);

        SetVisible(hasExitDestination);
    }

    public void ClearExitDestination()
    {
        exitSceneName =string.Empty;
        exitSpawnPointId =string.Empty;

        hasExitDestination =false;

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        if (UI_Manager.Instance != null && UI_Manager.Instance.IsExternalModalOpen)
        {
            UI_Manager.Instance.SetExternalModalOpen(false);
        }

        SetVisible(false);
    }

    public void OnClickExitButton()
    {
        if (!hasExitDestination) return;

        if (confirmPanel == null) return;

        confirmPanel.SetActive(true);
        confirmPanel.transform.SetAsLastSibling();

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.SetExternalModalOpen(true);
        }
        else
        {
            Time.timeScale = 0f;

            Cursor.visible = true;
            Cursor.lockState =CursorLockMode.None;
        }
    }

    public void OnClickCancel()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.SetExternalModalOpen(false);
        }
        else
        {
            Time.timeScale = 1f;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void OnClickConfirm()
    {
        if (!hasExitDestination)
        {
            return;
        }

        if (SceneTransitionManager.Instance == null) return;

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.SetExternalModalOpen(false);
        }
        else
        {
            Time.timeScale = 1f;
        }

        if (exitButton != null)
        {
            exitButton.SetActive(false);
        }

        SceneTransitionManager.Instance.LoadScene(exitSceneName, exitSpawnPointId);
    }

    private void SetVisible(bool visible)
    {
        if (exitButton != null)
        {
            exitButton.SetActive(visible);
        }
    }
}