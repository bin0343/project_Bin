using UnityEngine;
using System;

public class SkillTargetingController : MonoBehaviour
{
    public LayerMask groundLayer;

    private bool isTargeting = false;
    private Camera currentCamera; // 현재 사용 중인 카메라 (동적으로 할당)

    private GameObject rangeIndicatorInstance;   // 최대 사거리 원
    private GameObject targetAreaIndicatorInstance; // 마우스 따라다니는 공격 범위 원

    private float currentSkillRange;
    private Transform playerTransform;

    public Action<Vector3> OnTargetSelected;
    public Action OnTargetingCancelled;

    // Start에서 카메라를 가져오지 않습니다. (씬 변경 시 카메라가 바뀌기 때문)
    void Start()
    {
    }

    public void EnterTargetingMode(Skill_AreaAttack skillData, Transform pTransform)
    {
        // [중요] 스킬을 쓰려고 할 때, 현재 활성화된 카메라를 찾아서 할당합니다.
        currentCamera = Camera.main;

        // 만약 Camera.main이 못 찾는 경우를 대비한 안전 장치 (태그 문제 등)
        if (currentCamera == null)
        {
            currentCamera = FindObjectOfType<Camera>();
        }

        isTargeting = true;
        this.currentSkillRange = skillData.castRange;
        this.playerTransform = pTransform;

        // 사거리 표시 (큰 원)
        if (skillData.castRangeIndicatorPrefab != null)
        {
            rangeIndicatorInstance = Instantiate(skillData.castRangeIndicatorPrefab, playerTransform.position, Quaternion.identity);
            float castDiameter = skillData.castRange * 2f;
            rangeIndicatorInstance.transform.localScale = new Vector3(castDiameter, 0.01f, castDiameter);
        }

        // 타겟 범위 표시 (작은 원)
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
        if (!isTargeting || currentCamera == null) return;

        // 사거리 원은 플레이어 따라다니기
        if (rangeIndicatorInstance != null)
        {
            rangeIndicatorInstance.transform.position = playerTransform.position;
        }

        // 1. 현재 카메라에서 마우스 위치로 레이 발사
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition = Vector3.zero;
        bool hasHit = false;

        // 2. 쿼터뷰 전용: 수학적 평면(Plane)을 이용한 교차점 계산
        // 플레이어의 발바닥 높이(y)에 가상의 무한한 바닥이 있다고 가정합니다.
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, playerTransform.position.y, 0));
        float enter;

        // 먼저 Plane과 만나는지 체크 (가장 정확함)
        if (groundPlane.Raycast(ray, out enter))
        {
            targetPosition = ray.GetPoint(enter);
            hasHit = true;
        }
        // Plane이 안되면 물리적인 바닥 체크 (차선책)
        else if (Physics.Raycast(ray, out RaycastHit hit, 300f, groundLayer))
        {
            targetPosition = hit.point;
            hasHit = true;
        }

        if (hasHit)
        {
            // 3. 사거리 제한 로직
            float distance = Vector3.Distance(playerTransform.position, targetPosition);
            if (distance > currentSkillRange)
            {
                Vector3 direction = (targetPosition - playerTransform.position).normalized;
                targetPosition = playerTransform.position + direction * currentSkillRange;
            }

            // 4. 지형 높이 보정 (언덕/계단 고려)
            // 구한 위치의 하늘 위에서 아래로 레이를 쏴서 정확한 바닥 높이를 다시 찾습니다.
            RaycastHit groundHit;
            // 레이어 마스크가 설정되어 있다면 그것을 쓰고, 아니면 Everything(-1)
            int mask = groundLayer.value != 0 ? groundLayer.value : -1;

            if (Physics.Raycast(targetPosition + Vector3.up * 50f, Vector3.down, out groundHit, 100f, mask))
            {
                targetPosition = groundHit.point + Vector3.up * 0.01f; // 바닥보다 살짝 위에 표시
            }
            else
            {
                // 바닥을 못 찾으면 플레이어 높이로 강제 설정
                targetPosition.y = playerTransform.position.y + 0.01f;
            }

            // 인디케이터 이동
            if (targetAreaIndicatorInstance != null)
            {
                targetAreaIndicatorInstance.transform.position = targetPosition;
            }
        }

        // 입력 처리
        if (Input.GetMouseButtonDown(0))
        {
            ConfirmTarget(targetPosition);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            CancelTarget();
        }
    }

    private void ConfirmTarget(Vector3 pos)
    {
        isTargeting = false;
        OnTargetSelected?.Invoke(pos);
        CleanupIndicators();
    }

    private void CancelTarget()
    {
        isTargeting = false;
        OnTargetingCancelled?.Invoke();
        CleanupIndicators();
    }

    private void CleanupIndicators()
    {
        if (rangeIndicatorInstance != null) Destroy(rangeIndicatorInstance);
        if (targetAreaIndicatorInstance != null) Destroy(targetAreaIndicatorInstance);

        UI_Manager.Instance.IsInTargetingMode = false;

        // [수정된 부분] 카메라가 플레이어 안에 있는지 확인하여 뷰 모드 판별
        // 백뷰(TPS): 카메라는 플레이어의 자식임 -> 커서 잠금(기존 로직)
        // 쿼터뷰: 카메라는 씬에 별도로 존재함(플레이어 자식 아님) -> 커서 유지

        bool isQuarterView = false;

        // currentCamera가 할당되어 있고, 카메라가 플레이어의 자식이 아니라면 쿼터뷰로 간주
        if (currentCamera != null && playerTransform != null)
        {
            isQuarterView = !currentCamera.transform.IsChildOf(playerTransform);
        }

        if (isQuarterView)
        {
            // 쿼터뷰라면 조준이 끝나도 커서를 계속 보이게 강제 설정
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // 백뷰라면 UI_Manager의 기본 설정(보통 커서 숨김/잠금)을 따름
            UI_Manager.Instance.UpdateCursorState();
        }
    }
}