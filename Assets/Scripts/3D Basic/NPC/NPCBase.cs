using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public enum NPCTeam
{
    Ally,       //아군(플레이어 편) 
    Enemy,      //적 (시험 상대, 몬스터 편)
    Neutral     //중립
}

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
    [Header("소속 설정(스토리에 따라 변경)")]
    public NPCTeam currentTeam = NPCTeam.Ally;

    [Header("상태 및 타겟")]
    public NPCState currentState = NPCState.IDLE;
    public Transform playerTransform;
    public Transform currentTarget;
    
    private Transform lastAttacker;     // 나를 마지막으로 공격한 적

    [Header("스킬 관리")]
    public List<SkillHolder> skillHolders = new List<SkillHolder>();

    [Header("거리 설정")]
    public float followDistance = 3.0f;
    public float stopDistance = 2.0f;
    public float detectRange = 10.0f;
    public float attackRange = 1.5f;

    [Header("전투 설정")]
    public float attackDelay = 2.0f;
    protected float lastAttackTime = 0f;

    private NavMeshAgent navAgent;
    protected Animator animator;
    private NPC_Stat myStat;
    private NPC_Data myData;
    private Player_Action playerAction;

    private bool isDead = false;

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

        myData = myStat.npcData;
        if (myData != null && myData.npcSkills != null)
        {
            foreach (var skillData in myData.npcSkills)
            {
                if (skillData != null)
                    skillHolders.Add(new SkillHolder(skillData));
            }
        }

        navAgent.stoppingDistance = stopDistance;
    }

    void Update()
    {
        if (myStat.isDead || currentState == NPCState.DEAD) return;

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerAction = playerObj.GetComponent<Player_Action>();
            }
            return; // 찾기 전까진 아무것도 안 함
        }

        if (currentState == NPCState.BATTLE_READY || currentState == NPCState.ATTACK)
        {
            if (!IsTargetAlive(currentTarget))
            {
                StopMoving();
                currentTarget = null;

                FindBestTarget();

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
            case NPCState.DEAD:
                OnDeath();
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
        if (currentTarget == null)
        {
            FindBestTarget();
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
        if (currentTarget == null)
        {
            currentState = NPCState.IDLE;
        }

        transform.LookAt(currentTarget);
        float distToTarget = Vector3.Distance(transform.position, currentTarget.position);

        //공격 쿨타임 체크
        if (Time.time >= lastAttackTime + attackDelay)
        {
            SkillHolder bestSkill = GetBestAvailableSkill(distToTarget);

            if (bestSkill != null)
            {
                StartCoroutine(UseSkillRoutine(bestSkill));
            }
            else
            {
                // 2. 스킬이 없거나 쿨타임이면 일반 공격 (사거리 체크)
                if (distToTarget <= attackRange)
                {
                    StartCoroutine(AttackRoutine());
                }
                else
                {
                    // 공격 사거리 밖이면 다시 접근
                    currentState = NPCState.BATTLE_READY;
                }
            }
        }
    }

    private SkillHolder GetBestAvailableSkill(float distance)
    {
        foreach (var holder in skillHolders)
        {
            // MP와 쿨타임 조건 확인
            if (holder.CanUse(myStat.currentMP))
            {
                // *추가 고려사항: 스킬별 사거리 데이터가 Skill_Data에 있다면 여기서 거리 체크 가능
                // 지금은 일단 사용 가능한 첫 번째 스킬을 반환
                return holder;
            }
        }
        return null;
    }

    private IEnumerator UseSkillRoutine(SkillHolder skill)
    {
        lastAttackTime = Time.time; // 글로벌 쿨타임 적용 (스킬 후 바로 평타 못치게)

        // 스킬 사용 (SkillHolder 내부에서 Skill_Base.ApplySkillEffects 호출)
        skill.Use(gameObject);

        // 애니메이션 대기 등 (Skill_Data에 castTime이 있다면 활용)
        yield return new WaitForSeconds(skill.SkillData.castTime > 0 ? skill.SkillData.castTime : 0.5f);

        // 스킬 사용 후 상태 정리 (필요시)
    }

    #endregion

    #region Helpers

    private void CheckForBattleStart()
    {
        bool isUnderAttack = IsTargetAlive(lastAttacker);
        bool isPlayerFighting = (playerAction != null && playerAction.IsAttacking);

        if (isUnderAttack || isPlayerFighting)
        {
            FindBestTarget(); 

            if (currentTarget != null)
            {
                Debug.Log("동료: 전투 개시!");
                currentState = NPCState.BATTLE_READY;
            }
        }
    }

    private void FindBestTarget()
    {
        int targetLayerMask = 0;    //적대 레이어

        if (currentTeam == NPCTeam.Ally)
        {
            targetLayerMask = LayerMask.GetMask("Enemy");
        }
        else if (currentTeam == NPCTeam.Enemy)
        {
            targetLayerMask = LayerMask.GetMask("Player", "Companion");
        }

        if (IsTargetAlive(lastAttacker))
        {
            float dist = Vector3.Distance(transform.position, lastAttacker.position);
            if (dist <= detectRange * 1.5f)
            {
                currentTarget = lastAttacker;
                return;
            }
        }

        Collider[] targets = Physics.OverlapSphere(transform.position, detectRange, targetLayerMask);

        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var target in targets)
        {
            if (target.gameObject == gameObject) continue;
            if (!IsTargetAlive(target.transform)) continue;

            /*var stat = target.GetComponent<Enemy_Stat>();
            if (stat == null || stat.currentHP <= 0) continue;*/

            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = target.transform;
            }
        }

        currentTarget = nearest;
    }

    private bool IsTargetAlive(Transform targetToCheck)
    {
        if (targetToCheck == null) return false;
        if (!targetToCheck.gameObject.activeInHierarchy) return false;

        var stat = targetToCheck.GetComponent<Enemy_Stat>();
        if (stat != null && stat.currentHP <= 0) return false;

        return true;
    }

    private void StopMoving()
    {
        if (navAgent.isOnNavMesh) navAgent.ResetPath();
        animator.SetBool("IsMoving", false);
    }

    public void OnDamageTaken(Transform attacker)
    {
        if (myStat.isDead) return;

        lastAttacker = attacker; // 복수 대상 등록

        if (currentState == NPCState.IDLE || currentState == NPCState.FOLLOW)
        {
            currentTarget = attacker;
            currentState = NPCState.BATTLE_READY;
        }
    }

    protected virtual IEnumerator AttackRoutine()
    {
        lastAttackTime = Time.time;
        animator.SetTrigger("IsAttack");

        yield return new WaitForSeconds(0.5f);

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
        if (isDead) return;
        StartCoroutine(DeathRoutine());
    }

    public IEnumerator DeathRoutine()
    {
        isDead = true;

        
        currentState = NPCState.DEAD;
        StopMoving();
        if (navAgent.isOnNavMesh) navAgent.isStopped = true;
        animator.SetTrigger("IsDie");

        yield return new WaitForSeconds(5f);

        gameObject.SetActive(false);
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