using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPatternExecutor : MonoBehaviour
{
    [Header("테스트")]
    [Tooltip("아직 구현되지 않은 패턴의 임시 실행 시간")]
    [SerializeField] private float fakePatternDuration = 1.5f;

    [Header("돌진 장애물 검사")]
    [Tooltip("돌진을 막는 벽, 건물 등의 레이어")]
    [SerializeField] private LayerMask dashObstacleLayer;
    [Tooltip("돌진 경로를 검사할 구체의 반경")]
    [SerializeField, Min(0.05f)] private float dashCheckRadius = 0.6f;
    [Tooltip("보스 바닥 위치에서 검사 구체 중심까지의 높이")]
    [SerializeField, Min(0f)] private float dashCheckHeight = 1f;
    [Tooltip("벽과의 간격")]
    [SerializeField, Min(0f)] private float dashWallBuffer = 0.15f;

    [Header("경고선")]
    [SerializeField] private BossDashTelegraph dashTelegraph;
 
    private Animator animator;

    private EnemyAttackHItbox attackHitbox;
    private EnemyBase enemyBase;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        attackHitbox = GetComponentInChildren<EnemyAttackHItbox>();
        enemyBase = GetComponent<EnemyBase>();

        if (dashTelegraph == null)
        {
            dashTelegraph = GetComponentInChildren<BossDashTelegraph>(true);
        }

        if (attackHitbox == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 돌진에 사용할 " + $"EnemyAttackHItbox가 없습니다.");
        }

        dashTelegraph?.Hide();
    }

    public IEnumerator Execute(BossPatternData pattern, Transform target)
    {
        if (pattern == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 실행할 패턴 데이터가 없습니다.");
            yield break;
        }

        switch (pattern.patternType)
        {
            case BossPatternType.BasicMelee:
                yield return ExecuteBasicMelee(pattern, target);
                break;
            case BossPatternType.Dash:
                yield return ExecuteDash(pattern, target);
                break;
            default:
                yield return ExecuteFakePattern(pattern);
                break;
        }
    }

    #region PatternCoroutine
    private IEnumerator ExecuteBasicMelee(BossPatternData pattern, Transform target)
    {
        Debug.Log($"[{gameObject.name}] 근접 패턴 실행: " + $"{pattern.patternName}");

        FaceTarget(target);

        if (animator == null)
        {
            Debug.LogError($"[{gameObject.name}] Animator가 없습니다.");

            yield break;
        }

        animator.SetBool("IsIdle", false);
        animator.SetBool("IsMoving", false);
        animator.SetInteger("AttackIndex", pattern.attackIndex);
        animator.ResetTrigger("IsAttack");
        animator.SetTrigger("IsAttack");

        yield return new WaitForSeconds(pattern.animationTime);
    }

    private IEnumerator ExecuteDash(BossPatternData pattern, Transform target)
    {
        if (pattern == null) yield break;

        if (target == null) yield break;

        Debug.Log($"[{gameObject.name}] 돌진 패턴 실행: " + $"{pattern.patternName}");

        float windupElapsedTime = 0f;

        dashTelegraph?.Hide();

        while (windupElapsedTime < pattern.dashWindupTime)
        {
            if (enemyBase != null && enemyBase.isDead)
            {
                dashTelegraph?.Hide();
                yield break;
            }

            if (target == null)
            {
                dashTelegraph?.Hide();
                yield break;
            }

            windupElapsedTime += Time.deltaTime;

            FaceTarget(target);

            Vector3 previewDirection = transform.forward;
            previewDirection.y = 0f;
            
            UpdateDashTelegraph(pattern, previewDirection);

            yield return null;
        }

        Vector3 dashDirection = transform.forward;
        dashDirection.y = 0f;

        if (dashDirection.sqrMagnitude < 0.001f)
        {
            dashTelegraph?.Hide();
            yield break;
        }

        dashDirection.Normalize();

        dashTelegraph?.Hide();

        PlayDashAnimation(pattern);

        if (attackHitbox != null)
        {
            attackHitbox.EnableHitbox();
        }

        float elapsedTime = 0f;
        float previousCurveValue = 0f;

        while (elapsedTime < pattern.dashDuration)
        {
            if (enemyBase != null && enemyBase.isDead)
            {
                dashTelegraph?.Hide();

                if (attackHitbox != null)
                {
                    attackHitbox.DisableHitbox();
                }

                yield break;
            }

            elapsedTime += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(elapsedTime / pattern.dashDuration);
            float currentCurveValue = pattern.dashMoveCurve.Evaluate(normalizedTime);
            float curveDelta = currentCurveValue - previousCurveValue;

            bool dashBlocked = false;

            if (curveDelta > 0f)
            {
                float requestDistance = pattern.dashDistance * curveDelta;

                dashBlocked = TryGetSafeDash(dashDirection, requestDistance, out Vector3 safeMoveDelta);

                transform.position += safeMoveDelta;
            }

            previousCurveValue = currentCurveValue;

            if (dashBlocked)
            {
                Debug.Log($"[{gameObject.name}] " + $"돌진이 장애물에 막혀 종료되었습니다.");

                break;
            }

            yield return null;
        }

        dashTelegraph?.Hide();

        if (attackHitbox != null)
        {
            attackHitbox.DisableHitbox();
        }
    }

    private IEnumerator ExecuteFakePattern(BossPatternData pattern)
    {
        Debug.Log($"[{gameObject.name}] 미구현 패턴 임시 실행: " + $"{pattern.patternName}");

        yield return new WaitForSeconds(fakePatternDuration);
    }
    #endregion

    private void FaceTarget(Transform target)
    {
        if (target == null) return;

        Vector3 dircetion = target.position - transform.position;
        dircetion.y = 0f;

        if (dircetion.sqrMagnitude < 0.001f) return;

        transform.rotation = Quaternion.LookRotation(dircetion);
    }

    private bool TryGetSafeDash(Vector3 direction, float requestDistance, out Vector3 safeMoveDelta)
    {
        safeMoveDelta = Vector3.zero;

        if (requestDistance <= 0f) return false;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return false;
        
        direction.Normalize();

        Vector3 castOrigin = transform.position + Vector3.up * dashCheckHeight;

        float castDistance = requestDistance + dashWallBuffer;

        if (Physics.SphereCast(castOrigin, dashCheckRadius, direction, out RaycastHit hit, castDistance, dashObstacleLayer, QueryTriggerInteraction.Ignore))
        {
            float allowedDistance = Mathf.Clamp(hit.distance - dashWallBuffer, 0f, requestDistance);

            safeMoveDelta = direction * allowedDistance;

            return allowedDistance < requestDistance - 0.001f;
        }

        safeMoveDelta = direction * requestDistance;

        return false;
    }

    private void UpdateDashTelegraph(BossPatternData pattern, Vector3 dashDirection)
    {
        if (pattern == null) return;

        if (!pattern.showDashTelegraph)
        {
            dashTelegraph?.Hide();
            return;
        }

        if (dashTelegraph == null) return;

        dashDirection.y = 0f;

        if (dashDirection.sqrMagnitude < 0.001f)
        {
            dashTelegraph.Hide();
            return;
        }

        dashDirection.Normalize();

        TryGetSafeDash(dashDirection, pattern.dashDistance, out Vector3 safePreviewDelta);

        Vector3 startPostion = transform.position;
        Vector3 endPosition = startPostion + safePreviewDelta;

        dashTelegraph.UpdateLine(startPostion, endPosition, pattern.dashTelegraphWidth);
    }

    private void PlayDashAnimation(BossPatternData pattern)
    {
        if (pattern == null) return;

        if (!pattern.playDashAnimation) return;

        if (animator == null)
        {
            Debug.LogWarning($"[{gameObject.name}] " + $"돌진 애니메이션을 재생할 Animator가 없습니다.");

            return;
        }

        animator.SetBool("IsIdle", false);
        animator.SetBool("IsMoving", false);
        animator.SetInteger("AttackIndex", pattern.attackIndex);
        animator.ResetTrigger("IsAttack");
        animator.SetTrigger("IsAttack");
    }
}
