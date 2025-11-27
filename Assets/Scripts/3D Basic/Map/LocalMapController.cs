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
        public Transform warpPoint; // [핵심] 이동할 씬 내의 좌표 (SpawnPoint)
    }

    [Header("장소 목록 설정")]
    [SerializeField] private List<LocalLocationEntry> locations;

    [Header("UI 연결")]
    [SerializeField] private Text titleText;

    public void OpenLocalMap()
    {
        gameObject.SetActive(true);
        //Time.timeScale = 0f; // 일시정지

        // 쿼터뷰/마을에서는 마우스가 필요할 수도 있지만, 키보드 조작을 원하시면 잠금
        // 상황에 맞춰 주석 해제하세요.
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        // 첫 번째 버튼에 포커스 (키보드 조작 시작점)
        if (locations.Count > 0 && locations[0].buttonObj != null)
        {
            EventSystem.current.SetSelectedGameObject(locations[0].buttonObj.gameObject);
        }
    }

    public void OnClickTeleport(int index)
    {
        if (index < 0 || index >= locations.Count) return;

        // 이동할 목표 지점 가져오기
        Transform targetTr = locations[index].warpPoint;

        if (targetTr != null)
        {
            TeleportPlayer(targetTr);
        }
        else
        {
            Debug.LogError("이동할 목표 지점(Transform)이 연결되지 않았습니다!");
        }
    }

    private void TeleportPlayer(Transform targetTr)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // [중요] NavMeshAgent가 켜져 있으면 transform.position 변경이 무시될 수 있음
            UnityEngine.AI.NavMeshAgent agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false; // 잠시 끄기

            // 1. 위치 이동
            player.transform.position = targetTr.position;

            // 2. 회전도 맞춰주기 (도착했을 때 벽 보고 있으면 이상하니까)
            player.transform.rotation = targetTr.rotation;

            // 3. 카메라 갱신 (혹시 카메라가 부드럽게 따라가는 방식이면 순간이동 시 휙 튀는 걸 방지)
            // QuarterViewCamera나 CameraArm이 LateUpdate라 보통은 괜찮습니다.

            if (agent != null) agent.enabled = true; // 다시 켜기

            Debug.Log($"{targetTr.name}으로 순간이동 완료!");
        }

        CloseLocalMap();
    }

    public void CloseLocalMap()
    {
        //Time.timeScale = 1f; // 시간 재개
        gameObject.SetActive(false);
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
