using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public float AttackRange = 3f;
    private float attackDelay = 1.0f;
    private float lastAttackTime = 0f;

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

    protected void FixedUpdate()
    {
        Debug.Log("현재 상태: " + CurrentState);

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

    protected virtual void Attack()
    {
        float distance = Vector3.Distance(transform.position, Target.position);

        if (distance >= AttackRange)
        {
            CurrentState = ENEMYSTATE.SEARCH; // 다시 추적
            return;
        }

        // 쿨타임 체크
        if (Time.time >= lastAttackTime + attackDelay)
        {
            Animator.SetTrigger("IsAttack"); // Animator의 Trigger 이름 확인할 것!
            lastAttackTime = Time.time;
        }
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
        if (CurrentState == ENEMYSTATE.ATTACK) return; 

        if (Target == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
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
        else
        {
            float dist = Vector3.Distance(transform.position, Target.position);
            if (dist <= SearchRange && CurrentState != ENEMYSTATE.SEARCH)
            {
                navAgent.SetDestination(Target.position);
                CurrentState = ENEMYSTATE.SEARCH;
            }
        }
    }
}
