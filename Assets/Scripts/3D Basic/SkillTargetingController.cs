using UnityEngine;
using System;

public class SkillTargetingController : MonoBehaviour
{
    public LayerMask groundLayer; // 바닥을 감지하기 위한 레이어 마스크

    private bool isTargeting = false;
    private Camera mainCamera;
    private GameObject rangeIndicatorInstance;
    private GameObject targetIndicatorInstance;

    private float currentSkillRange;
    private Transform playerTransform;

    // 콜백: 조준이 완료되거나 취소되었음을 다른 스크립트에 알림
    public Action<Vector3> OnTargetSelected;
    public Action OnTargetingCancelled;

    void Start()
    {
        mainCamera = Camera.main;
    }

    // Player_Action에서 호출하여 조준 모드를 시작
    /*public void EnterTargetingMode(Skill_Active_Leap skillData, Transform pTransform)
    {
        isTargeting = true;
        this.currentSkillRange = skillData.skillRange;
        this.playerTransform = pTransform;

        // 범위 및 조준점 UI 생성
        if (skillData.rangeIndicatorPrefab != null)
        {
            rangeIndicatorInstance = Instantiate(skillData.rangeIndicatorPrefab, playerTransform.position, Quaternion.identity);
            rangeIndicatorInstance.transform.localScale = Vector3.one * currentSkillRange * 2;
        }
        if (skillData.targetIndicatorPrefab != null)
        {
            targetIndicatorInstance = Instantiate(skillData.targetIndicatorPrefab);
        }

        // 커서 보이기
        UI_Manager.Instance.OpenUI(null); // 임시로 OpenUI를 호출해 커서를 보이게 함
    }*/

    void Update()
    {
        if (!isTargeting) return;

        // 마우스 위치에 따라 조준점 이동
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 targetPosition = hit.point;

            // 플레이어로부터 최대 사거리를 벗어나지 않도록 위치 보정
            float distance = Vector3.Distance(playerTransform.position, targetPosition);
            if (distance > currentSkillRange)
            {
                Vector3 direction = (targetPosition - playerTransform.position).normalized;
                targetPosition = playerTransform.position + direction * currentSkillRange;
            }

            if (targetIndicatorInstance != null) targetIndicatorInstance.transform.position = targetPosition;

            // 좌클릭: 위치 확정 및 스킬 실행
            if (Input.GetMouseButtonDown(0))
            {
                isTargeting = false;
                OnTargetSelected?.Invoke(targetPosition);
                CleanupIndicators();
            }
            // 우클릭: 조준 취소
            else if (Input.GetMouseButtonDown(1))
            {
                isTargeting = false;
                OnTargetingCancelled?.Invoke();
                CleanupIndicators();
            }
        }
    }

    private void CleanupIndicators()
    {
        if (rangeIndicatorInstance != null) Destroy(rangeIndicatorInstance);
        if (targetIndicatorInstance != null) Destroy(targetIndicatorInstance);

        UI_Manager.Instance.CloseTopUI(); // 커서를 다시 숨김
    }
}
