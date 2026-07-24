using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossPhase
{
    Phase1,
    Phase2,
    Phase3,
    Phase4
}

public enum BossState
{
    Dormant,           // 보스전 시작 전
    Decision,          // 다음 패턴 선택
    ExecutingPattern,  // 패턴 실행 중
    PhaseChanging,     // 페이즈 전환 중
    Dead               // 사망
}

public enum BossDistanceZone
{
    Close,
    Far
}

public class BossEnemy : EnemyBase
{
    [Header("보스 상태, 보스전 시작 거리 조건")]
    [SerializeField] private BossPhase currentPhase = BossPhase.Phase1;
    [SerializeField] private BossState currentBossState = BossState.Dormant;
    [SerializeField] private float battleStartDistance = 10f;

    [Header("거리 판단")]
    [SerializeField] private float closeRange = 4f;     //이 안에 있으면 근거리

    [Header("보스 패턴")]
    [Tooltip("보스가 사용할 수 있는 패턴 목록")]
    [SerializeField] private List<BossPatternDataBase> availablePatterns = new List<BossPatternDataBase>();

    [Header("현재 가능한 패턴 - 확인용")]
    [SerializeField] private List<BossPatternDataBase> validPatterns = new List<BossPatternDataBase>();

    [Header("패턴 실행 테스트")]
    [SerializeField]
    private BossPatternDataBase selectedPattern;
    [Tooltip("패턴 종료 후 다음 패턴을 고르기 전 대기 시간")]
    [SerializeField]
    private float delayBetweenPatterns = 1f;

    [Header("패턴 실행기")]
    [SerializeField] private BossPatternExecutor patternExecutor;

    private Coroutine patternCoroutine;
    private float nextPatternDecisionTime;

    private readonly Dictionary<BossPatternDataBase, float> patternReadyTimes = new Dictionary<BossPatternDataBase, float>();

    [SerializeField] private BossDistanceZone currentDistanceZone;

    private Transform playerTransform;
    private Enemy_Stat bossStat;
    private BossPatternDataBase lastPattern;

    public BossPhase CurrentPhase
    {
        get { return currentPhase; }
    }

    public BossState CurrentBossState
    {
        get { return currentBossState; }
    }

    public BossDistanceZone CurrentDistanceZone
    {
        get { return currentDistanceZone; }
    }

    public override float CurrentDamageMultiplier
    {
        get
        {
            if (selectedPattern == null) return 1f;

            return selectedPattern.damageMultiplier;
        }
    }

    public override HitReactionType CurrentHitReactionType
    {
        get
        {
            if (selectedPattern == null) return HitReactionType.Normal;

            return selectedPattern.hitReactionType;
        }
    }

    private void Awake()
    {
        FindPlayer();

        bossStat = GetComponent<Enemy_Stat>();

        if (bossStat == null)
        {
            bossStat = GetComponentInParent<Enemy_Stat>();
        }

        if (patternExecutor == null)
        {
            patternExecutor = GetComponent<BossPatternExecutor>();
        }
    }

    protected override void FixedUpdate()
    {
        if (isDead)
        {
            currentBossState = BossState.Dead;
            return;
        }

        switch (currentBossState)
        {
            case BossState.Dormant:
                DormantLogic();
                break;
            case BossState.Decision:
                DecisionLogic();
                break;
            case BossState.ExecutingPattern:
                break;
            case BossState.PhaseChanging:
                break;
            case BossState.Dead:
                break;
            default:
                break;
        }
    }

    #region Dormant
    public void DormantLogic()
    {
        SetIdleAnimation();

        if (playerTransform == null)
        {
            FindPlayer();

            if (playerTransform == null) return;
        }

        Vector3 difference = playerTransform.position - transform.position;
        difference.y = 0f;

        float startDistanceSqr = battleStartDistance * battleStartDistance;

        if (difference.sqrMagnitude <= startDistanceSqr)
        {
            target = playerTransform;
            StartBossBattle();
        }
    }
    #endregion

    #region Decision
    public void DecisionLogic()
    {
        SetIdleAnimation();

        if (target == null) return;

        UpdateDistanceZone();

        if (patternCoroutine != null) return;

        if (Time.time < nextPatternDecisionTime) return;

        RefreshValidPatterns();

        selectedPattern = SelectPatternByWeight();

        if (selectedPattern == null)
        {
            nextPatternDecisionTime = GetNextPatternCheckTime();

            return;
        }

        StartSelectedPattern();
    }
    #endregion

    public override void OnDamageTaken(Transform attacker)
    {
        if (isDead) return;
        if (attacker == null) return;

        target = attacker;
        playerTransform = attacker;

        StartBossBattle();
    }

    public override bool EnterStunState(float duration, bool forceStun = false)
    {
        return false;
    }

    private void StartBossBattle()
    {
        if (currentBossState != BossState.Dormant) return;

        currentBossState = BossState.Decision;

        UpdateDistanceZone(true);

        if (BossHpBar.instance != null)
        {
            BossHpBar.instance.Show(bossStat);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 씬에서 UI_BossHpBar를 찾을 수 없습니다.");
        }
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    private void UpdateDistanceZone(bool forceRefresh = false)
    {
        if (target == null) return;

        Vector3 difference = target.position - transform.position;
        difference.y = 0f;

        float closeRangeSqr = closeRange * closeRange;

        BossDistanceZone nextZone;

        if (difference.sqrMagnitude <= closeRangeSqr)
        {
            nextZone = BossDistanceZone.Close;
        }
        else
        {
            nextZone = BossDistanceZone.Far;
        }

        bool zoneChanged = currentDistanceZone != nextZone;

        if (!zoneChanged && !forceRefresh) return;

        currentDistanceZone = nextZone;
    }

    #region PatternCoroutine

    private IEnumerator ExecuteSelectedPattern(BossPatternDataBase pattern)
    {
        if (patternExecutor == null)
        {
            FinishCurrentPattern(pattern);
            yield break;
        }

        yield return patternExecutor.Execute(pattern, target);

        if (isDead)
        {
            patternCoroutine = null;
            yield break;
        }

        FinishCurrentPattern(pattern);
    }

    private void FinishCurrentPattern(BossPatternDataBase completedPattern)
    {
        ForceEndAttackEffects();

        if (completedPattern != null)
        {
            StartPatternCooldown(completedPattern);

            lastPattern = completedPattern;
        }

        selectedPattern = null;
        patternCoroutine = null;

        nextPatternDecisionTime = Time.time + delayBetweenPatterns;

        currentBossState = BossState.Decision;
    }

    #endregion

    private void SetIdleAnimation()
    {
        if (animator == null) return;

        animator.SetBool("IsIdle", true);
        animator.SetBool("IsMoving", false);
    }

    private bool IsPatternConditionValid(BossPatternDataBase pattern)
    {
        if (pattern == null) return false;

        if (!pattern.usablePhases.Contains(currentPhase)) return false;

        switch (pattern.usableDistance)
        {
            case BossPatternDistance.Close:
                return currentDistanceZone == BossDistanceZone.Close;
            case BossPatternDistance.Far:
                return currentDistanceZone == BossDistanceZone.Far;
            case BossPatternDistance.Both:
                return true;
        }

        return false;
    }

    private bool IsPatternCooldownReady(BossPatternDataBase pattern)
    {
        if (pattern == null) return false;

        if (!patternReadyTimes.TryGetValue(pattern, out float readyTime)) return true;

        return Time.time >= readyTime;
    }

    private bool IsPatternValid(BossPatternDataBase pattern)
    {
        if (!IsPatternConditionValid(pattern)) return false;

        if (!IsPatternCooldownReady(pattern)) return false;

        return true;
    }

    private void StartPatternCooldown(BossPatternDataBase pattern)
    {
        if (pattern == null) return;

        float cooldown = Mathf.Max(pattern.cooldown, 0f);

        float readyTime = Time.time + cooldown;

        patternReadyTimes[pattern] = readyTime;
    }

    private float GetNextPatternCheckTime()
    {
        float earliestReadyTime = float.PositiveInfinity;

        bool hasConditionCandidate = false;

        foreach (BossPatternDataBase pattern in availablePatterns)
        {
            if (pattern == null) continue;

            if (!IsPatternConditionValid(pattern)) continue;

            hasConditionCandidate = true;

            if (!patternReadyTimes.TryGetValue(pattern, out float readyTime)) return Time.time;

            if (readyTime < earliestReadyTime) earliestReadyTime = readyTime;
        }

        if (!hasConditionCandidate || float.IsPositiveInfinity(earliestReadyTime)) return Time.time + 0.5f;

        return Mathf.Max(Time.time + 0.05f, earliestReadyTime);
    }

    private void RefreshValidPatterns()
    {
        validPatterns.Clear();

        foreach (BossPatternDataBase pattern in availablePatterns)
        {
            if (!IsPatternValid(pattern)) continue;

            validPatterns.Add(pattern);
        }

        foreach (BossPatternDataBase pattern in validPatterns)
        {
            Debug.Log($"- 후보 패턴: {pattern.patternName}");
        }
    }

    private float GetEffectivePatternWeight(BossPatternDataBase pattern)
    {
        if (pattern == null) return 0f;

        float weight = Mathf.Max(pattern.selectionWeight, 0f);

        if (!pattern.patternWeightBonus) return weight;

        if (lastPattern == null) return weight;

        if (lastPattern.PatternType != pattern.previousPatternType)
        {
            return weight;
        }

        float multiplier = Mathf.Max(pattern.patternWeightMultiplier, 1f);

        return weight * multiplier;
    }

    private BossPatternDataBase SelectPatternByWeight()
    {
        if (validPatterns.Count == 0) return null;

        float totalWeight = 0f;

        foreach (BossPatternDataBase pattern in validPatterns)
        {
            if (pattern == null) continue;

            totalWeight += GetEffectivePatternWeight(pattern);
        }

        if (totalWeight <= 0f) return null;

        float randomValue = Random.Range(0f, totalWeight);

        foreach (BossPatternDataBase pattern in validPatterns)
        {
            if (pattern == null) continue;

            float weight = GetEffectivePatternWeight(pattern);

            randomValue -= weight;

            if (randomValue <= 0f)
            {
                return pattern;
            }
        }

        return validPatterns[validPatterns.Count - 1];
    }

    private void StartSelectedPattern()
    {
        if (currentBossState != BossState.Decision) return;

        if (selectedPattern == null) return;

        currentBossState = BossState.ExecutingPattern;

        patternCoroutine = StartCoroutine(ExecuteSelectedPattern(selectedPattern));
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 보스전 시작 거리
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, battleStartDistance);

        // 근거리 패턴 판단 거리
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, closeRange);
    }
#endif
}
