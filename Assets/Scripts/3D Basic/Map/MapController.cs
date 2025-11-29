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

    private MapButtonEntry currentTargetEntry;
    //private string targetSceneName; // 이동하려고 선택한 씬 이름 임시 저장
    private GameObject lastSelectedMapButton;

    [System.Serializable]
    public struct MapButtonEntry
    {
        public string locationName; // UI 표시용 (예: 숲, 학교)
        public string sceneName;
        public Button buttonObj; // 해당 씬을 담당하는 버튼 오브젝트

        public string targetSpawnName;  //다음 씬에 갈때 찾을 스폰포인트 이름
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

    public void OnClickMapButton(int index)
    {
        if (index < 0 || index >= mapEntries.Count) return;

        currentTargetEntry = mapEntries[index];
        lastSelectedMapButton = EventSystem.current.currentSelectedGameObject;
        confirmationPanel.SetActive(true);
        if (confirmText != null)
        {
            string displayName = string.IsNullOrEmpty(currentTargetEntry.locationName) ? currentTargetEntry.sceneName : currentTargetEntry.locationName;
            confirmText.text = $"{displayName}으로 이동하시겠습니까?";
        }
        EventSystem.current.SetSelectedGameObject(confirmYesButton.gameObject);
    }

    public void OnConfirmYes()
    {
        SceneTransferManager.TargetSpawnName = currentTargetEntry.targetSpawnName;
        CloseMapPanel();
        SceneManager.LoadScene(currentTargetEntry.sceneName);
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
        confirmationPanel.SetActive (false);
        gameObject.SetActive(false);
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }
}
