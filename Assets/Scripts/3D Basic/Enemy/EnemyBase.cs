using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum ENEMYSTATE
{
    IDLE,
    MOVE,
    SEARCH,
    BATTLE,
    STUN,
    DEAD
}

public enum BattleAction
{
    Waiting,    // 행동 결정 대기
    Attacking,
    Shielding,
    Avoiding
}

public class EnemyBase : MonoBehaviour  //Time.timeScale = 1f; //연출력에 중요한 요소
{
    private ENEMYSTATE currentState = ENEMYSTATE.IDLE;
    private BattleAction currentBattleAction = BattleAction.Waiting;
    private bool isPerformingAction = false;

    public float shieldProbability = 0.3f;      //플레이어가 공격시 쉴드 확률
    public float avoidProbability = 0.8f;

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
    private float stunDuration = 1.5f;
    private float stunTimer = 0f;

    private NavMeshAgent navAgent;
    private Enemy_Stat stat;
    [SerializeField] public TrailRenderer slashTrail;
    private Player_Action playerAction;

    public ItemDrop itemDropper;

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
        
        /*if (stat.currentHP <= 0 && !isDead)
        {
            Dead();
        }*/
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

        if (navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.stoppingDistance = attackRange * 0.9f;
            navAgent.SetDestination(target.position);
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > searchRange + 3f)
        {
            target = null;
            if (navAgent.enabled && navAgent.isOnNavMesh)
            {
                navAgent.ResetPath();
            }
            currentState = ENEMYSTATE.IDLE;
            idleTimer = 0f;
            return;
        }

        if (distance <= attackRange)
        {
            if (navAgent.enabled && navAgent.isOnNavMesh)
            {
                navAgent.ResetPath();
            }
            currentState = ENEMYSTATE.BATTLE;
            return;
        }
    }
    #endregion

    #region Battle
    protected virtual void Battle()
    {
        if (isPerformingAction)
        {
            return;

        }
        if (target == null || playerAction == null || playerAction.IsDead)
        {
            currentState = ENEMYSTATE.IDLE;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance >= attackRange)
        {
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

        animator.SetBool("IsMoving", false);

        if (isPerformingAction) return;

        ChooseNextAction();
        ExecuteAction();
    }

    private void ChooseNextAction()
    {
        if (playerAction.IsAttacking && Random.value < avoidProbability)
        {
            currentBattleAction = BattleAction.Avoiding;
            return;
        }
        //순서 바꾸면 우선순위 바뀜.
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
                StartCoroutine(ShieldCoroutine());
                break;
            case BattleAction.Avoiding:
                StartCoroutine(AvoidCoroutine());
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
        //navAgent.isStopped = true;
        /*var defensePart = GetComponentInChildren<Weapon_EnemyDefense>();
        if (defensePart != null)
        {
            defensePart.SetActiveDefense(true);
        }*/

        animator.SetTrigger("IsShield");

        yield return new WaitForSeconds(2.0f);  //방패 들고 있을 시간.

        /*if (defensePart != null)
        {
            defensePart.SetActiveDefense(false);
        }*/
        //navAgent.isStopped = false;
        isPerformingAction = false;
    }

    IEnumerator AvoidCoroutine()
    {
        isPerformingAction = true;

        animator.SetTrigger("IsAvoiding");

        float avoidDuration = 0.8f; // 전체 점프 시간
        float avoidDistance = 5.0f; // 점프 거리
        float jumpHeight = 0.6f;    // 점프의 최대 높이

        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + (transform.position - target.position).normalized * avoidDistance;

        float elapsed = 0f;

        if (navAgent.enabled)
        {
            navAgent.enabled = false;
        }

        while (elapsed < avoidDuration)
        {
            float progress = elapsed / avoidDuration;

            Vector3 horizontalPosition = Vector3.Lerp(startPos, endPos, progress);

            float verticalPosition = jumpHeight * Mathf.Sin(progress * Mathf.PI);

            transform.position = horizontalPosition + new Vector3(0, verticalPosition, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(endPos.x, startPos.y, endPos.z);

        if (!navAgent.enabled)
        {
            navAgent.enabled = true;
        }

        isPerformingAction = false;
    }
    #endregion

    #region Dead
    public void Dead()
    {
        Debug.LogError($"--- {gameObject.name}의 Dead() 함수가 호출되었습니다! ---");
        if (isDead) return;

        isDead = true;
        currentState = ENEMYSTATE.DEAD; // 상태를 DEAD로 전환
        animator.SetTrigger("IsDie");   // 사망 애니메이션 재생

        if (navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }
        navAgent.enabled = false; // 죽은 후에는 NavMeshAgent를 완전히 꺼버리는 것이 안전합니다.

        if (itemDropper != null)
        {
            itemDropper.GenerateLoot();
        }

        gameObject.tag = "Corpse"; // 태그 변경

        if (hpBarObject != null)
        {
            hpBarObject.SetActive(false);
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // 경험치 지급
        if (target != null)
        {
            Player_Stat playerStat = target.GetComponent<Player_Stat>();
            if (playerStat != null)
            {
                playerStat.GainExp(stat.ExpReward);
            }
        }
        this.enabled = false;
    }
    #endregion

    #region Attack Reaction
    public void OnAttackParried()
    {
        Debug.Log("몬스터: 무기가 튕겨나가는 애니메이션 재생!");
        animator.SetTrigger("IsParried");

        isAttacking = false; // IsAttacking 플래그가 있다면 초기화
        lastAttackTime = Time.time; // 다음 공격까지 잠시 딜레이를 줌
    }
    #endregion

    #region Stun
    public void EnterStunState(float duration)
    {
        // 이미 스턴 중이거나 죽었다면 중복 실행 방지
        if (isDead) return;

        if (currentState == ENEMYSTATE.STUN)
        {
            stunTimer = 0f;
            animator.Play("Stun", 0, 0f); // 애니메이션을 처음부터 다시 재생하여 경직 효과를 명확히 보여줌
            Debug.Log("스턴 갱신!");
            return;
        }

        currentState = ENEMYSTATE.STUN;
        stunDuration = duration;
        stunTimer = 0f; // 타이머 초기화

        // 현재 하던 모든 행동을 즉시 중단
        isPerformingAction = false;
        StopAllCoroutines();
        //GetComponentInChildren<Weapon_EnemyDefense>(true)?.SetActiveDefense(false);
        if (navAgent.isOnNavMesh) navAgent.isStopped = true;

        animator.SetTrigger("IsStun");
        Debug.Log("STUN 상태 진입: " + duration + "초 동안 행동 불가");
    }

    protected virtual void StunStateLogic()
    {
        stunTimer += Time.deltaTime;
        if (stunTimer >= stunDuration)
        {
            // 스턴 시간이 끝나면 다시 움직일 수 있도록 준비
            if (navAgent.isOnNavMesh) navAgent.isStopped = false;

            // 상황에 맞는 다음 상태로 자연스럽게 전환
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= attackRange)
            {
                currentState = ENEMYSTATE.BATTLE;
            }
            else
            {
                currentState = ENEMYSTATE.SEARCH;
            }
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
                        if (navAgent.enabled && navAgent.isOnNavMesh)
                        {
                            navAgent.SetDestination(target.position);
                        }
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

        if (!isDead && other.gameObject.CompareTag("Player_Foot"))
        {
            EnterStunState(1.5f);
            Debug.Log("공격당함");
            return;
        }

        if (isDead && other.CompareTag("Player"))
        {
            Player_Action playerAction = other.GetComponent<Player_Action>();
            if (playerAction != null)
            {
                playerAction.OnLootableCorpseEnter(itemDropper);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isDead && other.CompareTag("Player"))
        {
            Player_Action playerAction = other.GetComponent<Player_Action>();
            if (playerAction != null)
            {
                playerAction.OnLootableCorpseExit();
            }
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
            navAgent.enabled = false;
            transform.position += knockDir * knockForce * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        navAgent.enabled = true;
    }
}
