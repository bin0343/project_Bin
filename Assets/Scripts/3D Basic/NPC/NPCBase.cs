using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum NPCState
{
    IDLE,
    FOLLOW,
    BATTLE_READY, // 전투 준비 (플레이어 공격 대기)
    ATTACK,
    DEAD
}

public class NPCBase : MonoBehaviour
{
    [Header("상태 및 타겟")]
    public NPCState currentState = NPCState.IDLE;
    public Transform playerTransform;
    public Transform currentTarget;

    // [추가] 나를 마지막으로 공격한 적
    private Transform lastAttacker;

    [Header("거리 설정")]
    public float followDistance = 3.0f;
    public float stopDistance = 2.0f;
    public float detectRange = 10.0f;
    public float attackRange = 1.5f;

    [Header("전투 설정")]
    public float attackDelay = 2.0f;
    private float lastAttackTime = 0f;

    private NavMeshAgent navAgent;
    private Animator animator;
    private NPC_Stat myStat;
    private Player_Action playerAction;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        myStat = GetComponent<NPC_Stat>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerAction = playerObj.GetComponent<Player_Action>();
        }

        navAgent.stoppingDistance = stopDistance;
    }

    void Update()
    {
        if (myStat.isDead || currentState == NPCState.DEAD) return;

        // [수정] 전투 중일 때는 타겟이 죽었는지 매 프레임 체크해야 "제자리 걷기" 버그가 안 생김
        if (currentState == NPCState.BATTLE_READY || currentState == NPCState.ATTACK)
        {
            if (!IsTargetAlive(currentTarget))
            {
                // 타겟이 죽었거나 사라졌으면 즉시 정지 및 재탐색
                StopMoving();
                currentTarget = null;

                // 바로 다음 타겟 찾기 시도
                FindBestTarget();

                // 그래도 없으면 IDLE 복귀
                if (currentTarget == null)
                {
                    currentState = NPCState.IDLE;
                    return; // 이번 프레임 종료
                }
                else
                {
                    currentState = NPCState.BATTLE_READY; // 새 타겟 잡고 전투 계속
                }
            }
        }

        switch (currentState)
        {
            case NPCState.IDLE:
                HandleIdle();
                break;
            case NPCState.FOLLOW:
                HandleFollow();
                break;
            case NPCState.BATTLE_READY:
                HandleBattleReady();
                break;
            case NPCState.ATTACK:
                HandleAttack();
                break;
        }
    }

    #region State Logics

    private void HandleIdle()
    {
        animator.SetBool("IsMoving", false);

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distToPlayer > followDistance)
        {
            currentState = NPCState.FOLLOW;
            return;
        }

        CheckForBattleStart();
    }

    private void HandleFollow()
    {
        animator.SetBool("IsMoving", true);

        // [안전장치] NavMesh 위에 있을 때만 이동
        if (navAgent.isOnNavMesh)
            navAgent.SetDestination(playerTransform.position);

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distToPlayer <= stopDistance)
        {
            StopMoving(); // 이동 멈춤 함수 분리
            currentState = NPCState.IDLE;
            return;
        }

        CheckForBattleStart();
    }

    private void HandleBattleReady()
    {
        // 타겟이 없으면 재탐색 (위의 Update에서 처리했지만 이중 체크)
        if (currentTarget == null)
        {
            FindBestTarget(); // [변경] FindBestTarget 사용
            if (currentTarget == null)
            {
                currentState = NPCState.IDLE;
                return;
            }
        }

        float distToEnemy = Vector3.Distance(transform.position, currentTarget.position);

        // 공격 사거리 안쪽인가?
        if (distToEnemy <= attackRange)
        {
            StopMoving(); // 공격 전에는 멈춰야 함
            currentState = NPCState.ATTACK;
        }
        else
        {
            // 적을 향해 이동
            if (navAgent.isOnNavMesh)
                navAgent.SetDestination(currentTarget.position);
            animator.SetBool("IsMoving", true);
        }
    }

    private void HandleAttack()
    {
        // 타겟 유효성 검사는 Update에서 이미 수행함

        transform.LookAt(currentTarget);

        if (Time.time >= lastAttackTime + attackDelay)
        {
            StartCoroutine(AttackRoutine());
        }

        // 적이 도망가면 다시 추적
        float distToEnemy = Vector3.Distance(transform.position, currentTarget.position);
        if (distToEnemy > attackRange)
        {
            currentState = NPCState.BATTLE_READY;
        }
    }

    #endregion

    #region Helpers

    private void CheckForBattleStart()
    {
        // 1. 내가 맞았거나(lastAttacker), 플레이어가 공격 중이면 전투 태세
        bool isUnderAttack = IsTargetAlive(lastAttacker);
        bool isPlayerFighting = (playerAction != null && playerAction.IsAttacking);

        if (isUnderAttack || isPlayerFighting)
        {
            FindBestTarget(); // 타겟 선정

            if (currentTarget != null)
            {
                Debug.Log("동료: 전투 개시!");
                currentState = NPCState.BATTLE_READY;
            }
        }
    }

    // [핵심 변경] 최적의 타겟 찾기 (우선순위: 나를 때린 놈 > 가장 가까운 놈)
    private void FindBestTarget()
    {
        // 1. 나를 공격한 적이 살아있고 근처에 있다면 1순위
        if (IsTargetAlive(lastAttacker))
        {
            float dist = Vector3.Distance(transform.position, lastAttacker.position);
            if (dist <= detectRange * 1.5f) // 감지 범위보다 조금 더 멀어도 복수하러 감
            {
                currentTarget = lastAttacker;
                return;
            }
        }

        // 2. 그 외 주변 적들 중 가장 가까운 적 탐색
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectRange, LayerMask.GetMask("Enemy"));

        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            // 이미 죽은 적 패스 (Enemy_Stat 체크)
            var stat = enemy.GetComponent<Enemy_Stat>();
            if (stat == null || stat.currentHP <= 0) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy.transform;
            }
        }

        currentTarget = nearest;
    }

    // [추가] 타겟이 살아있는지 확인하는 헬퍼 함수
    private bool IsTargetAlive(Transform targetToCheck)
    {
        if (targetToCheck == null) return false;
        if (!targetToCheck.gameObject.activeInHierarchy) return false;

        var stat = targetToCheck.GetComponent<Enemy_Stat>();
        if (stat != null && stat.currentHP <= 0) return false;

        return true;
    }

    // [추가] 이동 멈춤 처리 (제자리 걸음 버그 방지)
    private void StopMoving()
    {
        if (navAgent.isOnNavMesh) navAgent.ResetPath();
        animator.SetBool("IsMoving", false);
    }

    // [중요] 외부(NPC_Stat)에서 호출: "나 맞았어!"
    public void OnDamageTaken(Transform attacker)
    {
        // 이미 죽었으면 무시
        if (myStat.isDead) return;

        lastAttacker = attacker; // 복수 대상 등록

        // 쉬고 있거나 따라가는 중이었다면 바로 전투 태세로 전환
        if (currentState == NPCState.IDLE || currentState == NPCState.FOLLOW)
        {
            currentTarget = attacker;
            currentState = NPCState.BATTLE_READY;
        }
    }

    private IEnumerator AttackRoutine()
    {
        lastAttackTime = Time.time;
        animator.SetTrigger("IsAttack");

        yield return new WaitForSeconds(0.5f);

        // 공격 시점에도 타겟이 살아있는지 체크
        if (IsTargetAlive(currentTarget))
        {
            var enemyStat = currentTarget.GetComponent<Enemy_Stat>();
            if (enemyStat != null)
            {
                int dmg = myStat.attackPower;
                enemyStat.TakeDamage(dmg, AttackType.Normal);
                myStat.GainExp(10);
            }
        }

        yield return new WaitForSeconds(1.0f);
    }

    public void OnDeath()
    {
        currentState = NPCState.DEAD;
        StopMoving();
        if (navAgent.isOnNavMesh) navAgent.isStopped = true;
        animator.SetTrigger("IsDie");
    }

    public void OnAttackFinished()
    {
        if (currentState == NPCState.ATTACK)
        {
            currentState = NPCState.BATTLE_READY;
        }
    }

    #endregion
}