using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LocalMapController : MonoBehaviour
{
    [System.Serializable]
    public struct LocalLocationEntry
    {
        public string locationName; // 장소 이름 (예: 기숙사, 훈련장)
        public Button buttonObj;    // 해당 장소 버튼
        public string warpPointName;
    }

    [Header("장소 목록 설정")]
    [SerializeField] private List<LocalLocationEntry> locations;

    [Header("UI 연결")]
    [SerializeField] private Text titleText;

    [Header("확인 패널")]
    [SerializeField] private GameObject confirmationPanel; 
    [SerializeField] private Text confirmText;             
    [SerializeField] private Button confirmYesButton;

    private int pendingTargetIndex = -1;
    private GameObject lastSelectedMapButton;

    public void OpenLocalMap()
    {
        gameObject.SetActive(true);

        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        if (locations.Count > 0 && locations[0].buttonObj != null)
        {
            EventSystem.current.SetSelectedGameObject(locations[0].buttonObj.gameObject);
        }
    }

    public void OnClickTeleport(int index)
    {
        if (index < 0 || index >= locations.Count) return;

        pendingTargetIndex = index;
        lastSelectedMapButton = EventSystem.current.currentSelectedGameObject;
        
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);

            if (confirmText != null)
            {
                confirmText.text = $"{locations[index].locationName}(으)로\n이동하시겠습니까?";
            }

            if (confirmYesButton != null)
            {
                EventSystem.current.SetSelectedGameObject(confirmYesButton.gameObject);
            }
        }
        else
        {
            ExecuteTeleport(index);
        }
    }

    public void OnConfirmYes()
    {
        if (pendingTargetIndex != -1)
        {
            ExecuteTeleport(pendingTargetIndex);
        }
    }

    public void OnConfirmNo()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }

        if (lastSelectedMapButton != null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedMapButton);
        }
    }

    private void ExecuteTeleport(int index)
    {
        string targetName = locations[index].warpPointName;

        GameObject targetObj = GameObject.Find(targetName);

        if (targetObj != null)
        {
            TeleportPlayer(targetObj.transform);
        }
        else
        {
            Debug.LogError("이동할 목표 지점(Transform)이 연결되지 않았습니다!");
            // 이동 실패 시 확인 창 닫기
            if (confirmationPanel != null) confirmationPanel.SetActive(false);
        }
    }

    private void TeleportPlayer(Transform targetTr)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            UnityEngine.AI.NavMeshAgent agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false; // 잠시 끄기

            player.transform.position = targetTr.position;
            player.transform.rotation = targetTr.rotation;

            Transform cameraArm = player.transform.Find("CameraArm");

            if (cameraArm == null)
            {
                var camScript = player.GetComponentInChildren<CameraArm>();
                if (camScript != null) cameraArm = camScript.transform;
            }

            if (cameraArm != null)
            {
                cameraArm.rotation = targetTr.transform.rotation;
            }

            if (agent != null) agent.enabled = true; // 다시 켜기

            Debug.Log($"{targetTr.name}으로 순간이동 완료!");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        CloseLocalMap();
    }

    public void CloseLocalMap()
    {
        //Time.timeScale = 1f; // 시간 재개
        if (UI_Manager.Instance != null)
        {
            // UI 매니저가 스택에서 빼고, SetActive(false)도 해줍니다.
            UI_Manager.Instance.CloseSpecificUI(this.gameObject);
        }
        else
        {
            // UI 매니저가 없을 때를 대비한 예외처리
            gameObject.SetActive(false);
        }
    }

    // (선택 사항) 버튼 위에 커서가 올라갔을 때(Select) 제목 텍스트 바꾸기
    // 버튼의 EventTrigger 컴포넌트 -> OnSelect에 연결해서 사용 가능
    public void UpdateDescription(int index)
    {
        if (titleText != null && index >= 0 && index < locations.Count)
        {
            titleText.text = locations[index].locationName;
        }
    }
}
