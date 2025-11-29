using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private Player_Move playerMove;
    [SerializeField] private GameObject dailyCameraSystem;
    [SerializeField] private GameObject combatCameraSystem;

    private bool isCombatMode = false;

    void Start()
    {
        // 1. 플레이어 찾기 (DontDestroyOnLoad로 넘어온 플레이어)
        if (playerMove == null)
        {
            playerMove = FindObjectOfType<Player_Move>();
        }

        // 2. 시작하자마자 일상 모드로 초기화 (카메라 켜기 포함)
        SwitchToDailyMode();
    }

    void Update()
    {
        // 전투 카메라가 없으면(마을 씬) V키 전환 기능 막기
        if (combatCameraSystem == null) return;

        if (Input.GetKeyDown(KeyCode.V))
        {
            isCombatMode = !isCombatMode;
            if (isCombatMode) SwitchToCombatMode();
            else SwitchToDailyMode();
        }
    }

    public void SwitchToDailyMode()
    {
        // [수정] 인스펙터에 연결된 dailyCameraSystem만 믿지 말고,
        // Player_Move에게 직접 카메라를 켜라고 명령합니다.
        if (playerMove != null)
        {
            // 1. 플레이어 안에 있는 백뷰 카메라(CameraArm) 켜기
            playerMove.SetDailyCameraActive(true);

            // 2. 이동 기준을 다시 기본값(CameraArm)으로 복구
            // (SetReferenceTransform에 null을 넣으면 내부에서 CameraArm을 쓰도록 되어있음)
            playerMove.SetReferenceTransform(null);
        }

        // 혹시 인스펙터에 따로 연결해둔 게 있다면 켜기 (예비용)
        if (dailyCameraSystem != null)
        {
            dailyCameraSystem.SetActive(true);
        }

        if (combatCameraSystem != null)
        {
            combatCameraSystem.SetActive(false);
        }

        isCombatMode = false;

        // 마우스 커서 숨기기 & 잠금 (백뷰 모드)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // UI 매니저에게 전투모드 해제 알림 (커서 관련)
        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.SetBattleMode(false);
        }
    }

    public void SwitchToCombatMode()
    {
        if (playerMove != null)
        {
            // 플레이어 카메라 끄기
            playerMove.SetDailyCameraActive(false);
        }

        if (dailyCameraSystem != null) dailyCameraSystem.SetActive(false);
        if (combatCameraSystem != null) combatCameraSystem.SetActive(true);

        isCombatMode = true;

        // 마우스 커서 보이기 (쿼터뷰 모드)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerMove != null && combatCameraSystem != null)
        {
            playerMove.SetReferenceTransform(combatCameraSystem.transform);
        }

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.SetBattleMode(true);
        }
    }
}
