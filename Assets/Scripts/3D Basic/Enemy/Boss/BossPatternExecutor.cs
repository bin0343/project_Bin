using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    [Header("점프 공격")]
    [SerializeField] private BossAreaTelegraph leapTelegraph;
    [Tooltip("착탄할 바닥 레이어")]
    [SerializeField] private LayerMask leapGroundLayer;
    [Tooltip("점프 범위 공격 대상")]
    [SerializeField] private LayerMask attackTargetLayer;
    [SerializeField, Min(0.1f)] private float leapGroundCheckHeight = 10f;
    [SerializeField, Min(0.1f)] private float leapGroundCheckDistance = 30f;
    [SerializeField, Min(0.1f)] private float leapNavMeshSampleDistance = 2f;

    private NavMeshAgent navAgent;
    private Enemy_Stat enemyStat;

    private bool isLeapActive;
    private bool leapImpactTriggered;
    private bool leapAgentWasEnabled;

    private BossLeapPatternData activeLeapPattern;
    private Vector3 activeLeapImpactPoint;
    private Vector3 activeLeapLandingPoint;

    private Animator animator;

    private EnemyAttackHItbox attackHitbox;
    private EnemyBase enemyBase;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        attackHitbox = GetComponentInChildren<EnemyAttackHItbox>();
        enemyBase = GetComponent<EnemyBase>();
        navAgent = GetComponent<NavMeshAgent>();
        enemyStat = GetComponent<Enemy_Stat>();

        if (leapTelegraph == null)
        {
            leapTelegraph = GetComponentInChildren<BossAreaTelegraph>(true);
        }

        if (dashTelegraph == null)
        {
            dashTelegraph = GetComponentInChildren<BossDashTelegraph>(true);
        }

        if (attackHitbox == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 돌진에 사용할 " + $"EnemyAttackHItbox가 없습니다.");
        }

        dashTelegraph?.Hide();
        leapTelegraph?.Hide();
    }

    public IEnumerator Execute(BossPatternDataBase pattern, Transform target)
    {
        if (pattern == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 실행할 패턴 데이터가 없습니다.");
            yield break;
        }

        switch (pattern)
        {
            case BossBasicMeleePatternData meleePattern:
                yield return ExecuteBasicMelee(meleePattern, target);
                break;
            case BossDashPatternData dashPattern:
                yield return ExecuteDash(dashPattern, target);
                break;
            case BossLeapPatternData leapPattern:
                yield return ExecuteLeap(leapPattern, target);
                break;
            default:
                yield return ExecuteFakePattern(pattern);
                break;
        }
    }

    #region PatternCoroutine
    private IEnumerator ExecuteBasicMelee(BossBasicMeleePatternData pattern, Transform target)
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

    private IEnumerator ExecuteDash(BossDashPatternData pattern, Transform target)
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

    private IEnumerator ExecuteLeap(BossLeapPatternData pattern, Transform target)
    {
        if (pattern == null) yield break;
        if (target == null) yield break;

        Vector3 startPosition = transform.position;
        Vector3 impactPoint = GetLeapImpactPoint(target);
        Vector3 landingPoint = GetLeapLandingPoint(startPosition, impactPoint, pattern.leapLandingStopDistance);

        FaceTarget(target);

        activeLeapPattern = pattern;
        activeLeapImpactPoint = impactPoint;
        activeLeapLandingPoint = landingPoint;

        isLeapActive = true;
        leapImpactTriggered = false;

        if (pattern.showLeapTelegraph)
        {
            leapTelegraph?.Show(impactPoint, pattern.leapAttackRadius);
        }
        else
        {
            leapTelegraph?.Hide();
        }

        PlayLeapAnimation(pattern);

        leapAgentWasEnabled = navAgent != null && navAgent.enabled;

        if (leapAgentWasEnabled)
        {
            if (navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
                navAgent.ResetPath();
            }

            navAgent.enabled = false;
        }

        float elapsedTime = 0f;

        float impactEventTimeout = Mathf.Max(pattern.animationTime, pattern.leapTimeToImpact + 0.1f);

        while (!leapImpactTriggered && elapsedTime < impactEventTimeout)
        {
            if (enemyBase != null && enemyBase.isDead)
            {
                leapTelegraph?.Hide();

                isLeapActive = false;
                activeLeapPattern = null;

                yield break;
            }

            elapsedTime += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(elapsedTime / Mathf.Max(pattern.leapTimeToImpact, 0.01f));

            Vector3 horizontalPosition = Vector3.Lerp(startPosition, landingPoint, normalizedTime);

            float vecticalOffset = 4f * pattern.leapHeight * normalizedTime * (1f - normalizedTime);

            transform.position = horizontalPosition + Vector3.up * vecticalOffset;

            yield return null;
        }

        if (!leapImpactTriggered)
        {
            Debug.LogWarning($"[{gameObject.name}] " + $"점프 착지 Animation Event가 없어 " + $"시간 기준으로 착지 처리합니다.");

            TriggerLeapImpact();
        }

        RestoreAgentAfterLeap();

        float remainingAnimationTime =Mathf.Max(pattern.animationTime - elapsedTime, pattern.leapRecoveryTime);

        if (remainingAnimationTime > 0f)
        {
            yield return new WaitForSeconds(remainingAnimationTime);
        }

        isLeapActive = false;
        activeLeapPattern = null;
    } 

    private IEnumerator ExecuteFakePattern(BossPatternDataBase pattern)
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

    private void UpdateDashTelegraph(BossDashPatternData pattern, Vector3 dashDirection)
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

    private Vector3 GetLeapImpactPoint(Transform target)
    {
        Vector3 targetPosition = target.position;
        Vector3 rayOrigin = targetPosition + Vector3.up * leapGroundCheckHeight;
        float rayDistance = leapGroundCheckHeight + leapGroundCheckDistance;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, leapGroundLayer, QueryTriggerInteraction.Ignore))
        {
            targetPosition = hit.point;
        }
        else
        {
            targetPosition.y = transform.position.y;
        }

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit navHit, leapNavMeshSampleDistance, NavMesh.AllAreas))
        {
            targetPosition = navHit.position;
        }

        return targetPosition;
    }

    private Vector3 GetLeapLandingPoint(Vector3 startPosition, Vector3 impactPoint, float stopDistance)
    {
        Vector3 direction = impactPoint - startPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return impactPoint;
        }

        direction.Normalize();
        
        Vector3 landingPoint = impactPoint - direction * stopDistance;
        landingPoint.y = impactPoint.y;
        
        if (NavMesh.SamplePosition(landingPoint, out NavMeshHit navHit, leapNavMeshSampleDistance, NavMesh.AllAreas))
        {
            landingPoint = navHit.position;
        }

        return landingPoint;
    }

    private void PlayDashAnimation(BossDashPatternData pattern)
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

    private void PlayLeapAnimation(BossLeapPatternData pattern)
    {
        if (pattern == null) return;
        if (!pattern.playLeapAnimation) return;
        if (animator == null) return;

        animator.SetBool("IsIdle", false);
        animator.SetBool("IsMoving", false);
        animator.SetInteger("AttackIndex", pattern.attackIndex);
        animator.ResetTrigger("IsAttack");
        animator.SetTrigger("IsAttack");
    }

    public void TriggerLeapImpact()
    {
        if (!isLeapActive) return;
        if (leapImpactTriggered) return;
        if (activeLeapPattern == null) return;

        leapImpactTriggered = true;

        transform.position = activeLeapLandingPoint;

        leapTelegraph?.Hide();

        Vector3 effectDirection = activeLeapImpactPoint - activeLeapLandingPoint;

        SpawnPatternEffect(activeLeapPattern.impactEffect, activeLeapImpactPoint, effectDirection, activeLeapPattern.leapAttackRadius);

        if (CameraShakeManager.instance != null)
        {
            CameraShakeManager.instance.Shake(activeLeapPattern.leapShakeAmplitude, activeLeapPattern.leapShakeFrequency, activeLeapPattern.leapShakeDuration);
        }

        PerformLeapAreaAttack(activeLeapPattern, activeLeapImpactPoint);
    }

    private void PerformLeapAreaAttack(BossLeapPatternData pattern, Vector3 impactPoint)
    {
        Collider[] colliders = Physics.OverlapSphere(impactPoint, pattern.leapAttackRadius, attackTargetLayer, QueryTriggerInteraction.Ignore);

        HashSet<Character_Stat> hitTargets = new HashSet<Character_Stat>();

        foreach (Collider col in colliders)
        {
            Character_Stat targetStat = col.GetComponentInParent<Character_Stat>();

            if (targetStat == null) continue;
            if (!hitTargets.Add(targetStat)) continue;

            Player_Action playerAction = targetStat.GetComponentInParent<Player_Action>();
         
            if (playerAction != null && playerAction.IsInvincible)
            {
                if (playerAction.IsRollingState())
                {
                    playerAction.TriggerPerfectEvade();
                }

                continue;
            }

            int attackPower = enemyStat != null ? enemyStat.attackPower : 10;
            float damageMultiplier = enemyBase != null ? enemyBase.CurrentDamageMultiplier : 1f;
            int finalAttackPower = Mathf.RoundToInt(attackPower * damageMultiplier);
            int damage = Mathf.Max(finalAttackPower - targetStat.defensePower, 1);

            targetStat.TakeDamage(damage, transform);

            if (playerAction != null)
            {
                HitReactionType reactionType = enemyBase != null ? enemyBase.CurrentHitReactionType : HitReactionType.Normal;

                playerAction.OnDamageTaken(reactionType, impactPoint);
            }
        }
    }

    private GameObject SpawnPatternEffect(BossEffectData effectData, Vector3 spawnPosition, Vector3 lookDirection, float targetRadius = 0f)
    {
        if (effectData == null) return null;
        if (effectData.prefab == null) return null;

        spawnPosition.y += effectData.groundOffset;
        lookDirection.y = 0f;
        Quaternion spawnRotation = Quaternion.identity;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            spawnRotation = Quaternion.LookRotation(lookDirection.normalized);
        }

        GameObject effectObject = Instantiate(effectData.prefab, spawnPosition, spawnRotation);

        if (targetRadius > 0f)
        {
            float baseRadius = Mathf.Max(effectData.baseRadius, 0.01f);
            float scaleMultiplier = targetRadius / baseRadius;

            effectObject.transform.localScale *= scaleMultiplier;
        }

        if (effectData.lifetime > 0f)
        {
            Destroy(effectObject, effectData.lifetime);
        }

        return effectObject;
    }

    private void RestoreAgentAfterLeap()
    {
        if (!leapAgentWasEnabled) return;
        leapAgentWasEnabled = false;
        if (navAgent == null) return;
        if (enemyBase != null && enemyBase.isDead) return;

        navAgent.enabled = true;

        if (navAgent.isOnNavMesh)
        {
            navAgent.Warp(transform.position);

            navAgent.isStopped = true;
        }
    }
}
