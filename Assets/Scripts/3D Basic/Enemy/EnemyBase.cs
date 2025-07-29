using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    private ENEMYSTATE CurrentState = ENEMYSTATE.IDLE;
    protected Animator Animator;

    private float IdleDuration = 3f;
    private float IdleTimer = 0f;

    private Vector3 moveTarget;
    private float moveRadius = 10f;  // 랜덤 이동 범위
    private float moveSpeed = 2f;

    public Transform Target;
    public float SearchRange = 10f;

    private NavMeshAgent navAgent;

    protected void Start()
    {
        Animator = GetComponentInChildren<Animator>();
        navAgent = GetComponent<NavMeshAgent>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Target = player.transform;
        }
    }

    protected void Update()
    {
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
                break;
            default:
                break;
        }

        DetectPlayer();
    }

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
    protected virtual void Move()
    {
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

    protected virtual void Search()
    {
        if (Target == null) return;

        Animator.SetBool("IsIdle", false);
        Animator.SetBool("IsMoving", true);

        navAgent.stoppingDistance = 0f;
        navAgent.SetDestination(Target.position);

        float distance = Vector3.Distance(transform.position, Target.position);

        // 쫓는 거리보다 너무 멀어지면 다시 IDLE 상태로 전환
        if (distance > SearchRange + 3f)
        {
            Target = null;
            CurrentState = ENEMYSTATE.IDLE;
            IdleTimer = 0f;
            return;
        }

        /*// 공격 범위 안에 들어오면 상태 전환 (나중에 구현할 수 있음)
        if (distance <= 1.5f) // 예: 공격 범위
        {
            CurrentState = ENEMYSTATE.ATTACK;
        }*/
    }


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
        if (Target == null) return;

        float distance = Vector3.Distance(transform.position, Target.position);

        if (distance <= SearchRange && CurrentState != ENEMYSTATE.SEARCH)
        {
            navAgent.SetDestination(Target.position);
            CurrentState = ENEMYSTATE.SEARCH;
        }
    }
}
