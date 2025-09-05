using UnityEngine;
using System;

public class SkillTargetingController : MonoBehaviour
{
    public LayerMask groundLayer;

    private bool isTargeting = false;
    private Camera mainCamera;
    private GameObject rangeIndicatorInstance; // 최대 사거리 원
    private GameObject targetAreaIndicatorInstance; // 마우스 따라다니는 공격 범위 원

    private float currentSkillRange;
    private Transform playerTransform;

    public Action<Vector3> OnTargetSelected;
    public Action OnTargetingCancelled;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void EnterTargetingMode(Skill_AreaAttack skillData, Transform pTransform)
    {
        isTargeting = true;
        this.currentSkillRange = skillData.castRange;
        this.playerTransform = pTransform;

        if (skillData.castRangeIndicatorPrefab != null)
        {
            rangeIndicatorInstance = Instantiate(skillData.castRangeIndicatorPrefab, playerTransform.position, Quaternion.identity);
            float castDiameter = skillData.castRange * 2f;
            rangeIndicatorInstance.transform.localScale = new Vector3(castDiameter, 0.01f, castDiameter);
        }

        if (skillData.targetIndicatorPrefab != null)
        {
            targetAreaIndicatorInstance = Instantiate(skillData.targetIndicatorPrefab);
            float attackDiameter = skillData.attackRadius * 2f;
            targetAreaIndicatorInstance.transform.localScale = new Vector3(attackDiameter, 0.01f, attackDiameter);
        }

        UI_Manager.Instance.IsInTargetingMode = true;
        // 커서 상태 업데이트를 강제로 한 번 호출하여 즉시 반영
        UI_Manager.Instance.UpdateCursorState();
    }

    void Update()
    {
        if (!isTargeting) return;

        // --- ADDED: 최대 사거리 원이 플레이어를 따라다니도록 위치 갱신 ---
        if (rangeIndicatorInstance != null)
        {
            rangeIndicatorInstance.transform.position = playerTransform.position;
        }
        // ----------------------------------------------------

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 targetPosition = hit.point;

            float distance = Vector3.Distance(playerTransform.position, targetPosition);
            if (distance > currentSkillRange)
            {
                Vector3 direction = (targetPosition - playerTransform.position).normalized;
                targetPosition = playerTransform.position + direction * currentSkillRange;
            }

            // CHANGED: targetAreaIndicatorInstance를 사용하도록 수정
            if (targetAreaIndicatorInstance != null)
            {
                targetAreaIndicatorInstance.transform.position = targetPosition + new Vector3(0, 0.05f, 0);
            }

            if (Input.GetMouseButtonDown(0))
            {
                isTargeting = false;
                OnTargetSelected?.Invoke(targetPosition);
                CleanupIndicators();
            }
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

        // CHANGED: targetAreaIndicatorInstance를 사용하도록 수정
        if (targetAreaIndicatorInstance != null) Destroy(targetAreaIndicatorInstance);

        // CHANGED: 커서 전용 함수 호출
        UI_Manager.Instance.IsInTargetingMode = false;
        // 커서 상태 업데이트를 강제로 한 번 호출하여 즉시 반영
        UI_Manager.Instance.UpdateCursorState();
    }
}
