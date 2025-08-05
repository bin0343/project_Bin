using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyBase : MonoBehaviour
{
    private ENEMYSTATE CurrentState = ENEMYSTATE.IDLE;
    protected Animator Animator;

    private float IdleDuration = 3f;
    private float IdleTimer = 0f;

    private Vector3 moveTarget;
    private float moveRadius = 10f;  // 랜덤 이동 범위
    private float moveSpeed = 2f;
    private bool IsAttacking = false;
    public bool IsDead = false;

    public Transform Target;
    public float SearchRange = 10f;
    public float AttackRange = 3f;
    private float attackDelay = 1.0f;
    private float lastAttackTime = 0f;

    private NavMeshAgent navAgent;
    private Enemy_Stat Stat;

    protected void Start()
    {
        Animator = GetComponentInChildren<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        Stat = GetComponentInParent<Enemy_Stat>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Target = player.transform;
        }
    }

    protected void FixedUpdate()
    {
        //Debug.Log("현재 상태: " + CurrentState);

        if (Stat.CurrentHP <= 0)
        {
            Dead(); // 체력 확인 후 사망 처리
            return; // 사망 시 다른 상태 실행 막기
        }

        switch (CurrentState)
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
            case ENEMYSTATE.ATTACK:
                Attack();
                break;
            case ENEMYSTATE.Dead:
                Dead();
                break;
        }

        DetectPlayer();
    }

    #region Idle
    protected virtual void Idle()
    {
        Animator.SetBool("IsIdle", true);
        Animator.SetBool("IsMoving", false);

        if (Target != null && Vector3.Distance(transform.position, Target.position) <= SearchRange)
        {
            CurrentState = ENEMYSTATE.SEARCH;
            return;
        }

        IdleTimer += Time.deltaTime;
        if (IdleTimer >= IdleDuration)
        {
            IdleTimer = 0f;
            SetRandomMoveTarget();
            CurrentState = ENEMYSTATE.MOVE;
        }
    }
    #endregion

    #region Move
    protected virtual void Move()
    {
        if (IsAttacking) return;

        Animator.SetBool("IsIdle", false);
        Animator.SetBool("IsMoving", true);

        if (Target != null && Vector3.Distance(transform.position, Target.position) <= SearchRange)
        {
            CurrentState = ENEMYSTATE.SEARCH;
            return;
        }

        if (navAgent == null)
        {
            Vector3 direction = (moveTarget - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, moveTarget) < 0.1f)
            {
                CurrentState = ENEMYSTATE.IDLE;
            }
        }
        else
        {
            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                CurrentState = ENEMYSTATE.IDLE;
            }
        }
    }
    #endregion

    #region Search
    protected virtual void Search()
    {
        if (Target == null)
        {
            CurrentState = ENEMYSTATE.IDLE;
            return;
        }

        Animator.SetBool("IsIdle", false);
        Animator.SetBool("IsMoving", true);

        navAgent.stoppingDistance = AttackRange * 0.9f;
        navAgent.SetDestination(Target.position);

        float distance = Vector3.Distance(transform.position, Target.position);

        if (distance > SearchRange + 3f)
        {
            Target = null;
            navAgent.ResetPath();
            CurrentState = ENEMYSTATE.IDLE;
            IdleTimer = 0f;
            return;
        }

        if (distance <= AttackRange)
        {
            navAgent.ResetPath();
            CurrentState= ENEMYSTATE.ATTACK;
            return;
        }
    }
    #endregion

    #region Attack
    protected virtual void Attack()
    {
        if (Target == null)
        {
            CurrentState = ENEMYSTATE.IDLE;
            return;
        }

        float distance = Vector3.Distance(transform.position, Target.position);
        var relativePos = Target.position - transform.position;
        transform.rotation = Quaternion.LookRotation(relativePos);
        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

        Player_Stat playerStat = Target.GetComponent<Player_Stat>();
        if (playerStat != null && playerStat.CurrentHP <= 0)
        {
            IsAttacking = false;
            Target = null;
            CurrentState = ENEMYSTATE.IDLE;
            navAgent.ResetPath();
            return;
        }

        bool IsAttack = stateInfo.IsName("Brute Attack");
        float AniTime = stateInfo.normalizedTime;

        Animator.SetBool("IsMoving", false);

        if (IsAttack && AniTime < 1f)
        {
            IsAttacking = true;
            return;
        }

        if (IsAttack && AniTime >= 1f)
        {
            IsAttacking = false;

            if (distance >= AttackRange)
            {
                CurrentState = ENEMYSTATE.SEARCH;
                return;
            }

            // 쿨타임이 끝났으면 다음 공격
            if (Time.time >= lastAttackTime + attackDelay)
            {
                Animator.SetTrigger("IsAttack");
                lastAttackTime = Time.time;
                return;
            }
        }

        if (!IsAttack && Time.time >= lastAttackTime + attackDelay)
        {
            Animator.SetTrigger("IsAttack");
            lastAttackTime = Time.time;
        }
    }
    #endregion

    #region Dead
    void Dead()
    {
        if (IsDead) return;

        if (Stat.CurrentHP <= 0)
        {
            Animator.SetTrigger("IsDie");
            IsDead = true;
            CurrentState = ENEMYSTATE.Dead; // 상태 전이
            navAgent.ResetPath(); // 이동 멈추기
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
        if (CurrentState == ENEMYSTATE.ATTACK) return; 

        if (Target == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
            {
                Player_Stat playerStat = found.GetComponent<Player_Stat>();
                if (playerStat != null && playerStat.CurrentHP > 0)
                {
                    float dist = Vector3.Distance(transform.position, found.transform.position);
                    if (dist <= SearchRange)
                    {
                        Target = found.transform;
                        navAgent.SetDestination(Target.position);
                        CurrentState = ENEMYSTATE.SEARCH;
                    }
                }
            }
        }
        else
        {
            Player_Stat playerStat = Target.GetComponent<Player_Stat>();
            if (playerStat != null && playerStat.CurrentHP <= 0)
            {
                Target = null;
                navAgent.ResetPath();
                CurrentState = ENEMYSTATE.IDLE;
                return;
            }


            float dist = Vector3.Distance(transform.position, Target.position);
            if (dist <= SearchRange && CurrentState != ENEMYSTATE.SEARCH)
            {
                navAgent.SetDestination(Target.position);
                CurrentState = ENEMYSTATE.SEARCH;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsDead) return;

        if (other.gameObject.CompareTag("Player_Foot"))
        {
            Animator.SetTrigger("IsStun");
            Debug.Log("공격당함");
        }
    }
}
