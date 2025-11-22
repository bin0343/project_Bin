using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MapController : MonoBehaviour
{
    [Header("UI 요소 연결")]
    [SerializeField] private GameObject confirmationPanel; // 확인 팝업 패널
    [SerializeField] private Text confirmText;             // "XX로 이동하시겠습니까?" 텍스트
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("맵 버튼 설정")]
    // 씬 이름과 해당 버튼을 짝지어두는 구조체 리스트
    [SerializeField] private List<MapButtonEntry> mapEntries;

    private string targetSceneName; // 이동하려고 선택한 씬 이름 임시 저장
    private GameObject lastSelectedMapButton;

    [System.Serializable]
    public struct MapButtonEntry
    {
        public string sceneName; // 예: TownScene
        public Button buttonObj; // 해당 씬을 담당하는 버튼 오브젝트
    }

    public void OpenMapPanel()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f; // 게임 일시정지

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        string currentScene = SceneManager.GetActiveScene().name;
        Button startButton = null;

        foreach (var entry in mapEntries)
        {
            if (entry.sceneName == currentScene)
            {
                startButton = entry.buttonObj;
                break;
            }
        }

        if (startButton == null && mapEntries.Count > 0)
        {
            startButton = mapEntries[0].buttonObj;
        }

        if (startButton != null)
        {
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
    }

    public void OnClickMapButton(string sceneName)
    {
        targetSceneName = sceneName;
        lastSelectedMapButton = EventSystem.current.currentSelectedGameObject;
        confirmationPanel.SetActive(true);
        if (confirmText != null)
        {
            confirmText.text = $"{sceneName}으로 이동하시겠습니까?";
        }
        EventSystem.current.SetSelectedGameObject(confirmYesButton.gameObject);
    }

    public void OnConfirmYes()
    {
        Time.timeScale = 1f; // 게임 시간 재개
        SceneManager.LoadScene(targetSceneName);
    }   

    public void OnConfirmNo()
    {
        confirmationPanel.SetActive(false);
        if (lastSelectedMapButton != null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedMapButton);
        }
    }

    public void CloseMapPanel()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    /*public void OnClickLocationButton(string sceneName)
    {
        // 씬 이름이 비어있지 않은지 확인
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log($"{sceneName}으로 이동합니다.");

            // 시간이 멈춰있다면 다시 흐르게 하고 이동 (중요!)
            Time.timeScale = 1f;

            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("이동할 씬 이름이 설정되지 않았습니다!");
        }
    }*/
}
