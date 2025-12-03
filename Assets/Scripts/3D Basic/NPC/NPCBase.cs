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
    public Transform playerTransform; // 플레이어 (따라다닐 대상)
    public Transform currentTarget;   // 공격할 몬스터

    [Header("거리 설정")]
    public float followDistance = 3.0f; // 이 거리보다 멀어지면 따라감
    public float stopDistance = 2.0f;   // 이 거리 안쪽이면 멈춤
    public float detectRange = 10.0f;   // 적 탐지 범위
    public float attackRange = 1.5f;    // 공격 사거리

    [Header("전투 설정")]
    public float attackDelay = 2.0f;
    private float lastAttackTime = 0f;

    // 컴포넌트 참조
    private NavMeshAgent navAgent;
    private Animator animator;
    private NPC_Stat myStat;
    private Player_Action playerAction; // 플레이어의 상태(공격 여부) 확인용

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        myStat = GetComponent<NPC_Stat>();

        // 플레이어 찾기 (태그 사용)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerAction = playerObj.GetComponent<Player_Action>();
        }

        // NavMeshAgent 설정
        navAgent.stoppingDistance = stopDistance;
    }

    void Update()
    {
        if (myStat.isDead || currentState == NPCState.DEAD) return;

        // FSM 상태 머신
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

    // 1. IDLE: 플레이어 주변에서 대기하며 거리를 잼
    private void HandleIdle()
    {
        animator.SetBool("IsMoving", false);

        // 플레이어와의 거리 체크
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 너무 멀어지면 따라가기
        if (distToPlayer > followDistance)
        {
            currentState = NPCState.FOLLOW;
            return;
        }

        // 주변에 적이 있고, 플레이어가 공격 중이면 전투 돌입!
        CheckForBattleStart();
    }

    // 2. FOLLOW: 플레이어를 따라감
    private void HandleFollow()
    {
        animator.SetBool("IsMoving", true);
        navAgent.SetDestination(playerTransform.position);

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 충분히 가까워지면 다시 IDLE
        if (distToPlayer <= stopDistance)
        {
            navAgent.ResetPath();
            currentState = NPCState.IDLE;
            return;
        }

        // 이동 중에도 전투 시작 체크
        CheckForBattleStart();
    }

    // 3. BATTLE_READY: 전투 감지는 했으나 타겟팅/대기 중
    private void HandleBattleReady()
    {
        // 타겟이 없거나 죽었으면 다시 탐색
        if (currentTarget == null)
        {
            FindNearestEnemy();
            if (currentTarget == null)
            {
                // 적이 없으면 다시 일상 모드로 복귀
                currentState = NPCState.IDLE;
                return;
            }
        }

        // 타겟을 향해 이동 및 공격 전환
        float distToEnemy = Vector3.Distance(transform.position, currentTarget.position);
        if (distToEnemy <= attackRange)
        {
            currentState = NPCState.ATTACK;
        }
        else
        {
            // 적을 향해 이동
            navAgent.SetDestination(currentTarget.position);
            animator.SetBool("IsMoving", true);
        }
    }

    // 4. ATTACK: 실제 공격 수행
    private void HandleAttack()
    {
        if (currentTarget == null)
        {
            currentState = NPCState.BATTLE_READY;
            return;
        }

        navAgent.ResetPath(); // 멈춰서 공격
        animator.SetBool("IsMoving", false);
        transform.LookAt(currentTarget); // 적 바라보기

        // 공격 쿨타임 체크
        if (Time.time >= lastAttackTime + attackDelay)
        {
            StartCoroutine(AttackRoutine());
        }

        // 적이 도망가서 멀어지면 다시 추적
        float distToEnemy = Vector3.Distance(transform.position, currentTarget.position);
        if (distToEnemy > attackRange)
        {
            currentState = NPCState.BATTLE_READY;
        }
    }

    #endregion

    #region Helpers

    // 전투 시작 조건 체크 (IDLE, FOLLOW 상태에서 호출)
    private void CheckForBattleStart()
    {
        // 1. 플레이어가 '공격 상태(IsAttacking)'인지 확인
        if (playerAction != null && playerAction.IsAttacking)
        {
            // 2. 주변에 적이 있는지 확인
            FindNearestEnemy();

            if (currentTarget != null)
            {
                // 조건 만족! 전투 모드로 전환
                Debug.Log("동료: 플레이어의 공격을 감지! 전투 합류!");
                currentState = NPCState.BATTLE_READY;
            }
        }
    }

    // 가장 가까운 적 찾기
    private void FindNearestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectRange, LayerMask.GetMask("Enemy")); // Enemy 레이어 설정 필수

        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            // 죽은 적은 제외 (Enemy_Stat의 isDead 체크 등 필요)
            // if (enemy.GetComponent<Enemy_Stat>().isDead) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy.transform;
            }
        }

        currentTarget = nearest;
    }

    // 공격 코루틴
    private IEnumerator AttackRoutine()
    {
        lastAttackTime = Time.time;
        animator.SetTrigger("IsAttack"); // 애니메이션 트리거

        // 데미지 판정 타이밍 맞추기 (예: 0.5초 뒤)
        yield return new WaitForSeconds(0.5f);

        if (currentTarget != null)
        {
            // 적에게 데미지 주기
            var enemyStat = currentTarget.GetComponent<Enemy_Stat>();
            if (enemyStat != null)
            {
                // NPC 공격력으로 데미지 계산
                int dmg = myStat.attackPower;
                enemyStat.TakeDamage(dmg, AttackType.Normal);

                // 경험치 획득 (막타가 아니어도 공격 시 경험치 줄지, 처치 시 줄지는 기획에 따라)
                myStat.GainExp(10); // 예시: 공격 한 번당 경험치 10 획득
            }
        }

        yield return new WaitForSeconds(1.0f); // 후딜레이
    }

    public void OnDeath()
    {
        currentState = NPCState.DEAD;
        navAgent.isStopped = true;
        animator.SetTrigger("IsDie");
    }

    #endregion
}