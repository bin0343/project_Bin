using UnityEngine;
using System;

public class SkillTargetingController : MonoBehaviour
{
    public LayerMask groundLayer;

    private bool isTargeting = false;
    private Camera mainCamera;
    private GameObject rangeIndicatorInstance;   // 최대 사거리 원
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
        UI_Manager.Instance.UpdateCursorState();
    }

    void Update()
    {
        if (!isTargeting) return;

        if (rangeIndicatorInstance != null)
        {
            rangeIndicatorInstance.transform.position = playerTransform.position;
        }

        // 마우스 → 땅으로 레이캐스트
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Vector3 targetPosition = Vector3.zero;

        // 먼저 groundLayer에 직접 맞는지 체크
        bool raycastHitGround = Physics.Raycast(ray, out hit, 100f, groundLayer);

        if (raycastHitGround)
        {
            targetPosition = hit.point;
        }
        else
        {
            // 땅을 못 맞췄을 때 (허공/벽/몬스터 등) → 마우스 월드 좌표 기준으로 보정
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y)
            );

            // 플레이어 기준 방향으로 캐스팅 범위 제한
            Vector3 directionToMouse = (mouseWorldPos - playerTransform.position).normalized;
            targetPosition = playerTransform.position + directionToMouse * currentSkillRange;
        }

        // 사거리 보정
        float distance = Vector3.Distance(playerTransform.position, targetPosition);
        if (distance > currentSkillRange)
        {
            Vector3 direction = (targetPosition - playerTransform.position).normalized;
            targetPosition = playerTransform.position + direction * currentSkillRange;
        }

        // 최종적으로는 항상 땅 위로 보정 (언덕, 계단, 경사 고려)
        RaycastHit groundHit;
        if (Physics.Raycast(targetPosition + Vector3.up * 50f, Vector3.down, out groundHit, 100f, groundLayer))
        {
            targetPosition = groundHit.point + Vector3.up * 0.01f;
        }

        // indicator 위치 갱신
        if (targetAreaIndicatorInstance != null)
        {
            targetAreaIndicatorInstance.transform.position = targetPosition;
        }

        // 확정 or 취소
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

    private void CleanupIndicators()
    {
        if (rangeIndicatorInstance != null) Destroy(rangeIndicatorInstance);
        if (targetAreaIndicatorInstance != null) Destroy(targetAreaIndicatorInstance);

        UI_Manager.Instance.IsInTargetingMode = false;
        UI_Manager.Instance.UpdateCursorState();
    }
}
