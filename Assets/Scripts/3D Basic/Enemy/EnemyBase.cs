using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum ENEMYSTATE
{
    IDLE,
    MOVE,
    SEARCH,
    BATTLE,
    DEAD
}

public enum BattleAction
{
    Waiting,    // 행동 결정 대기
    Attacking,
    Shielding
}

public enum HitEffectType
{
    Stun,
    Slow,
    Knockback
}

public class EnemyBase : MonoBehaviour  //Time.timeScale = 1f; //연출력에 중요한 요소
{
    private ENEMYSTATE currentState = ENEMYSTATE.IDLE;
    private BattleAction currentBattleAction = BattleAction.Waiting;
    private bool isPerformingAction = false;

    public float shieldProbability = 0.8f;      //플레이어가 공격시 쉴드 확률

    protected Animator animator;

    private float idleDuration = 3f;
    private float idleTimer = 0f;

    private Vector3 moveTarget;
    private float moveRadius = 10f;  // 랜덤 이동 범위
    private float moveSpeed = 2f;
    public bool isAttacking = false;
    public bool isDead = false;

    public Transform target;
    public float searchRange = 10f;
    public float attackRange = 3f;
    private float attackDelay = 1.0f;
    private float lastAttackTime = 0f;

    private NavMeshAgent navAgent;
    private Enemy_Stat stat;
    [SerializeField] public TrailRenderer slashTrail;
    private Player_Action playerAction;

    private ItemDrop itemDropper;

    [Header("UI 설정")]
    public GameObject hpBarObject; // 몬스터 체력바 캔버스 오브젝트
    public float hpBarVisibleRange = 12f; // 체력바가 보이는 거리 (SearchRange보다 길게 설정)


    private bool isPlayerNearby = false; // 시체 근처 감지

    protected void Start()
    {
        animator = GetComponentInChildren<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        stat = GetComponentInParent<Enemy_Stat>();
        itemDropper = GetComponent<ItemDrop>();
        slashTrail = GetComponentInChildren<TrailRenderer>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            playerAction = player.GetComponent<Player_Action>();
        }

        if (hpBarObject != null)
        {
            hpBarObject.SetActive(false);
        }

        slashTrail.emitting = false;
    }

    private void Update()
    {
        if (isPlayerNearby && isDead && Input.GetKeyDown(KeyCode.G))
        {
            if (UI_Loot.Instance.lootPanel.activeSelf)
            {
                UI_Loot.Instance.CloseLootPanel();
            }
            else
            {
                if (itemDropper != null)
                {
                    UI_Loot.Instance.OpenLootPanel(itemDropper);
                }
            }
        }
    }

    protected void FixedUpdate()
    {
        ManageHpBarVisibility();
        if (stat.CurrentHP <= 0)
        {
            Dead();
            return;
        }

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

        if (target != null && Vector3.Distance(transform.position, target.position) <= searchRange)
        {
            currentState = ENEMYSTATE.SEARCH;
            return;
        }

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

        if (target != null && Vector3.Distance(transform.position, target.position) <= searchRange)
        {
            currentState = ENEMYSTATE.SEARCH;
            return;
        }

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
        if (target == null)
        {
            currentState = ENEMYSTATE.IDLE;
            return;
        }

        animator.SetBool("IsIdle", false);
        animator.SetBool("IsMoving", true);

        navAgent.stoppingDistance = attackRange * 0.9f;
        navAgent.SetDestination(target.position);

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > searchRange + 3f)
        {
            target = null;
            navAgent.ResetPath();
            currentState = ENEMYSTATE.IDLE;
            idleTimer = 0f;
            return;
        }

        if (distance <= attackRange)
        {
            navAgent.ResetPath();
            currentState= ENEMYSTATE.BATTLE;
            return;
        }
    }
    #endregion

    #region Battle
    protected virtual void Battle()
    {
        if (target == null || playerAction == null || playerAction.IsDead)
        {
            if (slashTrail != null)
            {
                slashTrail.emitting = false;
            }
            currentState = ENEMYSTATE.IDLE;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance >= attackRange)
        {
            if (slashTrail != null)
            {
                slashTrail.emitting = false;
            }
            isPerformingAction = false;
            currentState = ENEMYSTATE.SEARCH;
            return;
        }
        transform.LookAt(target.position);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        Player_Stat playerStat = target.GetComponent<Player_Stat>();
        if (playerStat != null && playerStat.currentHP <= 0)
        {
            isAttacking = false;
            target = null;
            currentState = ENEMYSTATE.IDLE;
            navAgent.ResetPath();
            return;
        }

        //bool IsAttack = stateInfo.IsName("Brute Attack");
        //float AniTime = stateInfo.normalizedTime;

        animator.SetBool("IsMoving", false);

        /*if (IsAttack && AniTime < 1f)
        {
            isAttacking = true;
            return;
        }*/

        /*if (IsAttack && AniTime >= 1f)
        {
            isAttacking = false;

            if (distance >= attackRange)
            {
                currentState = ENEMYSTATE.SEARCH;
                return;
            }

            // 쿨타임이 끝났으면 다음 공격
            if (Time.time >= lastAttackTime + attackDelay)
            {
                animator.SetTrigger("IsAttack");
                lastAttackTime = Time.time;
                return;
            }
        }*/

        /*if (!IsAttack && Time.time >= lastAttackTime + attackDelay)
        {
            animator.SetTrigger("IsAttack");
            lastAttackTime = Time.time;
        }*/

        if (isPerformingAction) return;

        ChooseNextAction();
        ExecuteAction();
    }
    #endregion

    #region Dead
    void Dead()
    {
        if (isDead) return;

        if (stat.CurrentHP <= 0)
        {
            animator.SetTrigger("IsDie");
            isDead = true;
            currentState = ENEMYSTATE.DEAD; // 상태 전이
            navAgent.isStopped = true;
            navAgent.ResetPath(); // 이동 멈추기
            slashTrail.emitting = false;

            if (itemDropper != null)
            {
                itemDropper.GenerateLoot();
            }

            gameObject.tag = "Corpse";

            if (hpBarObject != null)
            {
                hpBarObject.SetActive(false);
            }

            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            if (target != null)
            {
                Player_Stat playerStat = target.GetComponent<Player_Stat>();
                if (playerStat != null)
                {
                    playerStat.GainExp(stat.ExpReward);
                }
            }
        }
    }
    #endregion

    private void ChooseNextAction()
    {
        if (playerAction.IsAttacking && Random.value < shieldProbability)
        {
            currentBattleAction = BattleAction.Shielding;
            return;
        }

        if (Time.time >= lastAttackTime + attackDelay)
        {
            currentBattleAction = BattleAction.Attacking;
            return;
        }

        //둘다 아니면 대기
        currentBattleAction = BattleAction.Waiting;
    }

    private void ExecuteAction()
    {
        switch (currentBattleAction)
        {
            case BattleAction.Attacking:
                StartCoroutine(AttackCoroutine());
                break;
            case BattleAction.Shielding:
                StartCoroutine (ShieldCoroutine());
                break;
            case BattleAction.Waiting:
                break;
        }
    }

    IEnumerator AttackCoroutine()
    {
        isPerformingAction = true;

        animator.SetTrigger("IsAttack");
        lastAttackTime = Time.time;

        yield return new WaitForSeconds(2.2f); //공격 애니메이션 시간

        isPerformingAction = false;
    }

    IEnumerator ShieldCoroutine()
    {
        isPerformingAction = true;

        var defensePart = GetComponentInChildren<Weapon_EnemyDefense>();
        if (defensePart != null)
        {
            defensePart.SetActiveDefense(true);
        }

        animator.SetTrigger("IsShield");

        yield return new WaitForSeconds(2.0f);  //방패 들고 있을 시간.

        if (defensePart != null)
        {
            defensePart.SetActiveDefense(false);
        }

        //animator.SetTrigger("ShieldEnd"); //쉴드 내리는거(선택사항)
        isPerformingAction = false;
    }

    #region Attack Reaction
    public void OnAttackParried()
    {
        Debug.Log("몬스터: 무기가 튕겨나가는 애니메이션 재생!");
        animator.SetTrigger("IsParried");

        if (slashTrail != null)
        {
            slashTrail.emitting = false;
        }

        // 현재 진행 중인 공격을 강제로 중단하고 싶을 때 사용
        // 예: 공격 코루틴을 중단하거나, 공격 상태를 즉시 종료
        isAttacking = false; // IsAttacking 플래그가 있다면 초기화
        lastAttackTime = Time.time; // 다음 공격까지 잠시 딜레이를 줌
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
        if (currentState == ENEMYSTATE.BATTLE) return; 

        if (target == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
            {
                Player_Stat playerStat = found.GetComponent<Player_Stat>();
                if (playerStat != null && playerStat.currentHP > 0)
                {
                    float dist = Vector3.Distance(transform.position, found.transform.position);
                    if (dist <= searchRange)
                    {
                        target = found.transform;
                        navAgent.SetDestination(target.position);
                        currentState = ENEMYSTATE.SEARCH;
                    }
                }
            }
        }
        else
        {
            Player_Stat playerStat = target.GetComponent<Player_Stat>();
            if (playerStat != null && playerStat.currentHP <= 0)
            {
                target = null;
                navAgent.ResetPath();
                currentState = ENEMYSTATE.IDLE;
                return;
            }


            float dist = Vector3.Distance(transform.position, target.position);
            if (dist <= searchRange && currentState != ENEMYSTATE.SEARCH)
            {
                navAgent.SetDestination(target.position);
                currentState = ENEMYSTATE.SEARCH;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (IsDead) return;

        if (!isDead && other.gameObject.CompareTag("Player_Foot"))
        {
            animator.SetTrigger("IsStun");
            if (slashTrail != null)
            {
                slashTrail.emitting = false;
            }
            Debug.Log("공격당함");
            return;
        }

        if (isDead && other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            UI_Manager.Instance.ShowMessage("G : 시체확인");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if (!IsDead) return;
        if (isDead && other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            UI_Manager.Instance.HideMessage();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isDead && other.CompareTag("Player"))
        {
            if (!isPlayerNearby)
            {
                isPlayerNearby = true;
                UI_Manager.Instance.ShowMessage("G : 시체확인");
            }
        }
    }

    void ManageHpBarVisibility()
    {
        if (isDead || target == null || hpBarObject == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        // 플레이어가 가시 범위 안에 있고 체력바가 꺼져있으면 켠다
        if (distance <= hpBarVisibleRange && !hpBarObject.activeSelf)
        {
            hpBarObject.SetActive(true);
        }
        // 플레이어가 가시 범위를 벗어났고 체력바가 켜져있으면 끈다
        else if (distance > hpBarVisibleRange && hpBarObject.activeSelf)
        {
            hpBarObject.SetActive(false);
        }
    }

    public IEnumerator ApplyKnockback()
    {
        navAgent.isStopped = true;

        Vector3 knockDir = (transform.position - target.position).normalized; // 플레이어 반대 방향
        float knockForce = 10f;
        float knockTime = 0.3f;
        float elapsed = 0f;

        

        if (Shared.MainCamera != null)
        {
            yield return null;
            Shared.MainCamera.Shake(0.15f, knockTime, 3);
        }

        while (elapsed < knockTime)
        {
            transform.position += knockDir * knockForce * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        navAgent.isStopped = false;
        yield return new WaitForSeconds(1.5f);
    }
}
