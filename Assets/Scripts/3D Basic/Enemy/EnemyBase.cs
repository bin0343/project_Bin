using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;

public enum ENEMYSTATE
{
    IDLE,
    MOVE,
    SEARCH,
    BATTLE,
    STUN,
    DEAD
}

public enum SearchPhase
{
    Chasing,
    Investigating,
    Patrolling
}

public enum BattleAction
{
    Waiting,    // 행동 결정 대기
    Attacking
}

public class EnemyBase : MonoBehaviour
{
    private ENEMYSTATE currentState = ENEMYSTATE.IDLE;
    private BattleAction currentBattleAction = BattleAction.Waiting;
    private SearchPhase currentSearchPhase;
    private bool isPerformingAction = false;

    protected Animator animator;

    private float idleDuration = 3f;
    private float idleTimer = 0f;

    private Vector3 moveTarget;
    private float moveRadius = 10f;
    private float moveSpeed = 2f;
    public bool isAttacking = false;
    public bool isDead = false;

    public Transform target;
    public float searchRange = 10f;
    public float attackRange = 3f;
    [SerializeField] private float battleExitRange = 0.5f;
    private float stunDuration = 1.5f;
    private float stunTimer = 0f;
    private bool hasAggro = false;  // 어그로 여부

    [Header("AI 공격 타이밍")]
    [SerializeField] private float attackDelay = 1.0f;
    [SerializeField] private float attackAnimationTime = 2.2f;
    private float nextAttackReadyTime = 0f;

    [Header("AI 스턴, 공격 우선순위")]
    [Tooltip("공격 모션이 시작된 경우 스턴 무시")]
    [SerializeField] private bool ignoreStunWhileAttacking = true;
    [Tooltip("공격 쿨타임 끝나면 스턴 무시")]
    [SerializeField] private bool ignoreStunWhenAttackReady = true;

    private NavMeshAgent navAgent;
    private Enemy_Stat stat;
    [SerializeField] public TrailRenderer slashTrail;
    private Player_Action playerAction;
    private Enemy_AnimationEvent animationEvent;
    private MonsterSpawner mySpawner;

    public ItemDrop itemDropper;

    [Header("UI 설정")]
    public GameObject hpBarObject; 
    public float hpBarVisibleRange = 12f; 

    [Header("AI - 시야각")]
    [Range(0, 360)]
    public float viewAngle = 120f; 
    public float viewRadius { get { return searchRange; } }
    public Transform eyeTransform;

    [Header("AI - 레이어")]
    public LayerMask playerLayer;
    public LayerMask obstacleLayerMask; 

    [Header("AI - 추적로직")]
    public float timeToGiveUp = 5f;     //추적 포기 시간
    private Vector3 lastKnownPosition;

    [Header("AI - Patrol Logic")]
    [Tooltip("마지막 목격 지점 주변을 순찰할 반경")]
    public float patrolRadius = 5f;
    [Tooltip("주변을 순찰할 총 시간 (초)")]
    public float patrolDuration = 5f;
    private float patrolTimer = 0f;

    public void SetSpawner(MonsterSpawner spawner) { mySpawner = spawner; }

    [SerializeField] private EnemyDissolveEffect dissolveEffect;

    protected void Start()
    {
        animator = GetComponentInChildren<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        stat = GetComponentInParent<Enemy_Stat>();
        itemDropper = GetComponent<ItemDrop>();
        slashTrail = GetComponentInChildren<TrailRenderer>();
        animationEvent = GetComponentInChildren<Enemy_AnimationEvent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerAction = playerObj.GetComponent<Player_Action>();
        }

        if (hpBarObject != null)
        {
            hpBarObject.SetActive(false);
        }

        slashTrail.emitting = false;
        if (dissolveEffect == null)
        {
            dissolveEffect = GetComponentInChildren<EnemyDissolveEffect>();
        }
    }

    protected void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

        ManageHpBarVisibility();

        switch (currentState)
        {
            case ENEMYSTATE.IDLE:
                Idle();
                break;
            case ENEMYSTATE.MOVE:
                Move();
                break;
            case ENEMYSTATE.SEARCH:
                Search();
                break;
            case ENEMYSTATE.BATTLE:
                Battle();
                break;
            case ENEMYSTATE.STUN:
                StunStateLogic();
                break;
            case ENEMYSTATE.DEAD:
                Dead();
                break;
        }

        DetectPlayer();
    }

    #region Idle
    protected virtual void Idle()
    {
        animator.SetBool("IsIdle", true);
        animator.SetBool("IsMoving", false);

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {
            idleTimer = 0f;
            SetRandomMoveTarget();
            currentState = ENEMYSTATE.MOVE;
        }
    }
    #endregion

    #region Move
    protected virtual void Move()
    {
        if (isAttacking) return;

        animator.SetBool("IsIdle", false);
        animator.SetBool("IsMoving", true);

        if (navAgent == null)
        {
            Vector3 direction = (moveTarget - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, moveTarget) < 0.1f)
            {
                currentState = ENEMYSTATE.IDLE;
            }
        }
        else
        {
            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                currentState = ENEMYSTATE.IDLE;
            }
        }
    }
    #endregion

    #region Search
    protected virtual void Search()
    {
        Debug.Log("현재 수색 단계: " + currentSearchPhase);
        switch (currentSearchPhase)
        {
            case SearchPhase.Chasing:
                Chase();
                break;
            case SearchPhase.Investigating:
                Investigate();
                break;
            case SearchPhase.Patrolling:
                PatrolArea();
                break;
        }
    }

    private bool IsTargetVisible()
    {
        if (target == null) return false;

        Vector3 dirToTarget = (target.position - transform.position).normalized;

        // 시야각 체크
        if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
        {
            float distToTarget = Vector3.Distance(transform.position, target.position);
            Vector3 eyePos = eyeTransform != null ? eyeTransform.position : transform.position;

            // 장애물 체크
            if (!Physics.Raycast(eyePos, dirToTarget, distToTarget, obstacleLayerMask))
            {
                return true; //타겟 보임
            }
        }
        return false; //타겟 안보임
    }

    private void Chase()
    {
        if (target == null)
        {
            currentState = ENEMYSTATE.IDLE;
            hasAggro = false;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (hasAggro)
        {
            lastKnownPosition = target.position;
        }
        else
        {
            if (IsTargetVisible())
            {
                lastKnownPosition = target.position;
            }
            else
            {
                currentSearchPhase = SearchPhase.Investigating;
                return;
            }
        }

        if (distance <= attackRange)
        {
            StopMoveForBattle();
            currentState = ENEMYSTATE.BATTLE;
            return;
        }

        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = false;
            navAgent.stoppingDistance = attackRange * 0.9f;
            navAgent.SetDestination(target.position);
        }

        animator.SetBool("IsIdle", false);
        animator.SetBool("IsMoving", true);
    }

    private void Investigate()
    {
        if (navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = false;
            navAgent.SetDestination(lastKnownPosition);
        }

        animator.SetBool("IsIdle", false);
        animator.SetBool("IsMoving", true);

        if (IsTargetVisible())
        {
            currentSearchPhase = SearchPhase.Chasing;
            return;
        }

        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            currentSearchPhase = SearchPhase.Patrolling;
            patrolTimer = 0f;
        }
    }

    private void PatrolArea()
    {
        if (navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = false;
        }
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsMoving", true);

        patrolTimer += Time.fixedDeltaTime;
        if (patrolTimer > patrolDuration)
        {
            target = null;
            currentState = ENEMYSTATE.IDLE;
            animator.SetBool("IsIdle", true);
            animator.SetBool("IsMoving", false);
            return;
        }

        if (IsTargetVisible())
        {
            currentSearchPhase = SearchPhase.Chasing;
            return;
        }

        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += lastKnownPosition;

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(randomDirection, out navHit, patrolRadius, -1))
            {
                navAgent.SetDestination(navHit.position);
            }
        }
    }

    private void StopMoveForBattle()
    {
        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }

        animator.SetBool("IsMoving", false);
        animator.SetBool("IsIdle", true);
    }
    #endregion

    #region Battle
    protected virtual void Battle()
    {
        if (isPerformingAction) return;

        if (target == null || IsTargetDead(target))
        {
            currentState = ENEMYSTATE.IDLE;
            target = null;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > attackRange + battleExitRange)
        {
            isPerformingAction = false;
            currentState = ENEMYSTATE.SEARCH;
            currentSearchPhase = SearchPhase.Chasing;
            return;
        }

        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }

        animator.SetBool("IsMoving", false);
        animator.SetBool("IsIdle", true);

        transform.LookAt(target.position);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        Character_Stat playerStat = target.GetComponent<Character_Stat>();
        if (playerStat != null && playerStat.currentHP <= 0)
        {
            isAttacking = false;
            target = null;
            currentState = ENEMYSTATE.IDLE;
            navAgent.ResetPath();
            return;
        }

        ChooseNextAction();
        ExecuteAction();
    }

    private bool IsTargetDead(Transform targetObj)
    {
        if (targetObj.CompareTag("Player"))
        {
            return playerAction != null && playerAction.stat.currentHP <= 0;
        }
        else if (targetObj.CompareTag("Companion"))
        {
            var npcStat = targetObj.GetComponent<Character_Stat>();
            return npcStat != null && npcStat.currentHP <= 0;
        }
        return true;
    }

    private void ChooseNextAction()
    {
        if (CanStartAttackNow())
        {
            currentBattleAction = BattleAction.Attacking;
            return;
        }

        currentBattleAction = BattleAction.Waiting;
    }

    private void ExecuteAction()
    {
        switch (currentBattleAction)
        {
            case BattleAction.Attacking:
                StartCoroutine(AttackCoroutine());
                break;
            case BattleAction.Waiting:
                break;
        }
    }

    IEnumerator AttackCoroutine()
    {
        isPerformingAction = true;
        isAttacking = false;
        nextAttackReadyTime = float.PositiveInfinity;
        animator.SetTrigger("IsAttack");
        yield return new WaitForSeconds(attackAnimationTime);

        if (animationEvent != null) animationEvent.ForceEndAttack();
        isPerformingAction = false;
        isAttacking = false;

        nextAttackReadyTime = Time.time + attackDelay;
    }

    #endregion

    #region Dead
    public void Dead()
    {
        if (isDead) return;

        if (animationEvent != null) animationEvent.ForceEndAttack();

        transform.DOKill();

        isDead = true;

        currentState = ENEMYSTATE.DEAD;
        animator.SetTrigger("IsDie");

        if (dissolveEffect != null)
        {
            dissolveEffect.PlayDissolve();
        }
        else
        {
            gameObject.SetActive(false);
        }

        if (navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }
        navAgent.enabled = false;

        gameObject.tag = "Corpse";

        if (itemDropper != null)
        {
            itemDropper.GenerateLoot();
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;
            col.enabled = false;
        }

        if (hpBarObject != null)
        {
            hpBarObject.SetActive(false);
        }

        if (target != null)
        {
            Character_Stat playerStat = target.GetComponent<Character_Stat>();
            if (playerStat != null)
            {
                playerStat.GainExp(stat.ExpReward);
            }
        }

        //퀘스트 로직
        if (QuestManager.instance != null && stat != null)
        {
            if (!string.IsNullOrEmpty(stat.EnemyName))
            {
                QuestManager.instance.AdvanceQuestProgress(stat.EnemyName, 1);
            }
            else
            {
                Debug.LogWarning($"이 몬스터({gameObject.name})의 Enemy_Stat에 EnemyName이 지정되지 않아 퀘스트 카운트가 오르지 않습니다.");
            }
        }
    }
    #endregion

    #region Stun
    public bool EnterStunState(float duration)
    {
        // 이미 스턴 중이거나 죽었다면 중복 실행 방지
        if (isDead) return false;

        if (ShouldIgnoreHitStun())
        {
            if (currentState == ENEMYSTATE.STUN && !isPerformingAction && !isAttacking)
            {
                ExitStunToCombatState();
            }

            return false;
        }

        if (currentState == ENEMYSTATE.STUN)
        {
            stunTimer = 0f;
            animator.Play("Stun", 0, 0f);
            Debug.Log("스턴 갱신!");
            return true;
        }

        transform.DOKill();

        currentState = ENEMYSTATE.STUN;
        stunDuration = duration;
        stunTimer = 0f; // 타이머 초기화

        isPerformingAction = false;
        if (animationEvent != null) animationEvent.ForceEndAttack();
        StopAllCoroutines();
        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh) navAgent.isStopped = true;

        animator.SetTrigger("IsStun");
        Debug.Log("STUN 상태 진입: " + duration + "초 동안 행동 불가");
        return true;
    }

    private bool IsAttackCooldownReady()
    {
        return Time.time >= nextAttackReadyTime;
    }

    private bool IsTargetInAttackRange()
    {
        if (target == null) return false;
        return Vector3.Distance(transform.position, target.position) <= attackRange;
    }

    private bool CanStartAttackNow()
    {
        return IsAttackCooldownReady() && IsTargetInAttackRange();
    }

    private bool ShouldIgnoreHitStun()
    {
        if (isDead) return true;

        if (ignoreStunWhileAttacking && (isPerformingAction || isAttacking))
        {
            return true;
        }

        if (ignoreStunWhenAttackReady && CanStartAttackNow())
        {
            return true;
        }

        return false;
    }

    private void ExitStunToCombatState()
    {
        stunTimer = 0f;

        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = false;
        }

        if (target == null)
        {
            currentState = ENEMYSTATE.IDLE;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            currentState = ENEMYSTATE.BATTLE;
        }
        else
        {
            currentState = ENEMYSTATE.SEARCH;
            currentSearchPhase = SearchPhase.Chasing;
        }
    }

    protected virtual void StunStateLogic()
    {
        if (ignoreStunWhenAttackReady && CanStartAttackNow())
        {
            ExitStunToCombatState();
            return;
        }

        stunTimer += Time.deltaTime;
        if (stunTimer >= stunDuration)
        {
            ExitStunToCombatState();
        }
    }



    #endregion

    protected virtual void SetRandomMoveTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * moveRadius;
        moveTarget = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        if (navAgent != null)
        {
            navAgent.SetDestination(moveTarget);
        }
    }

    protected virtual void DetectPlayer()
    {
        if (currentState == ENEMYSTATE.STUN || currentState == ENEMYSTATE.DEAD || currentState == ENEMYSTATE.BATTLE)
        {
            return;
        }

        if (target == null)
        {
            Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, playerLayer);

            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                Transform potentialTarget = targetsInViewRadius[i].transform;
                Vector3 dirToTarget = (potentialTarget.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
                {
                    float distToTarget = Vector3.Distance(transform.position, potentialTarget.position);
                    Vector3 eyePos = eyeTransform != null ? eyeTransform.position : transform.position;

                    if (!Physics.Raycast(eyePos, dirToTarget, distToTarget, obstacleLayerMask))
                    {
                        target = potentialTarget;
                        currentState = ENEMYSTATE.SEARCH;
                        return;
                    }
                    else
                    {
                        Debug.Log("시야가 장애물에 막혔습니다."); // 디버그 로그
                    }
                }
                else
                {
                    Debug.Log(potentialTarget.name + "가 시야각 밖에 있습니다."); // 디버그 로그
                }
            }
        }
        else
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > searchRange * 1.5f)
            {
                target = null;
                hasAggro = false;
                currentState = ENEMYSTATE.IDLE;
                currentSearchPhase = SearchPhase.Chasing;
            }
        }
    }

    void ManageHpBarVisibility()
    {
        if (isDead || hpBarObject == null) return;

        Transform playerTr = null;
        if (playerAction != null)
        {
            playerTr = playerAction.transform;
        }
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTr = p.transform;
        }

        if (playerTr == null) return;

        float distance = Vector3.Distance(transform.position, playerTr.position);

        if (distance <= hpBarVisibleRange && !hpBarObject.activeSelf)
        {
            hpBarObject.SetActive(true);
        }
        else if (distance > hpBarVisibleRange && hpBarObject.activeSelf)
        {
            hpBarObject.SetActive(false);
        }
    }

    public void ApplyKnockback()
    {
        if (target == null) return;

        Vector3 knockDir = (transform.position - target.position).normalized;
        float knockDistance = 3.0f; // 기존 힘(10) * 시간(0.3) 대략 계산
        float knockTime = 0.3f;

        if (Shared.MainCamera != null)
        {
            Shared.MainCamera.Shake(0.15f, knockTime, 3);
        }

        if (navAgent.enabled) navAgent.enabled = false;

        transform.DOMove(transform.position + knockDir * knockDistance, knockTime)
            .SetEase(Ease.OutCubic) // 부드러운 감속 효과
            .OnComplete(() => {
                if (!isDead && navAgent != null) navAgent.enabled = true;
            });
    }

    public void OnDamageTaken(Transform attacker)
    {
        if (isDead || attacker == null) return;

        target = attacker;
        hasAggro = true;

        lastKnownPosition = attacker.position;
        currentSearchPhase = SearchPhase.Chasing;

        idleTimer = 0f;
        patrolTimer = 0f;

        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = false;
            navAgent.ResetPath();
        }

        if (currentState == ENEMYSTATE.STUN || currentState == ENEMYSTATE.DEAD) return;

        float distance = Vector3.Distance(transform.position, attacker.position);

        if (distance <= attackRange)
            currentState = ENEMYSTATE.BATTLE;
        else
            currentState = ENEMYSTATE.SEARCH;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 시야 반경(searchRange)을 원으로 표시
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // 시야각을 부채꼴로 표시
        Vector3 forward = transform.forward;
        Quaternion rotLeft = Quaternion.Euler(0, -viewAngle / 2, 0);
        Quaternion rotRight = Quaternion.Euler(0, viewAngle / 2, 0);
        Vector3 dirLeft = rotLeft * forward;
        Vector3 dirRight = rotRight * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, dirLeft * viewRadius);
        Gizmos.DrawRay(transform.position, dirRight * viewRadius);

        UnityEditor.Handles.color = new Color(1, 1, 0, 0.1f);
        UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, dirLeft, viewAngle, viewRadius);

        // 현재 타겟이 있다면 타겟까지 선을 표시
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
#endif
}
