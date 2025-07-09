using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlimeControl : GreenSlimeControl
{
    private enum SlimeState { Idle, Chasing, Attacking, Waiting }
    private SlimeState CurrentState = SlimeState.Idle;

    public float DetectRange = 20f;
    public float AttackRange = 5f;
    public float FollowSpeed = 3f;
    public float JumpForce = 10f;
    private Transform PlayerTarget;
    private float AttackCool = 4f;
    private float lastAttackTime = -Mathf.Infinity;
    private float waitDuration = 3f;   // 대기 시간
    private float waitTimer = 0f;

    private Vector2 attackTargetPosition;  // 돌진 목표 위치


    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (IsDead) return;
        
        if (PlayerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) PlayerTarget = player.transform;
        }

        if (PlayerTarget == null) return;

        if (CurrentState == SlimeState.Waiting)
        {
            ProcessWaitingState();
            return;
        }

        if (CurrentState == SlimeState.Attacking)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, PlayerTarget.position);
        float currentTime = Time.time;

        switch (CurrentState)
        {
            case SlimeState.Idle:
                if (distance <= DetectRange)
                {
                    CurrentState = SlimeState.Chasing;
                }
                break;

            case SlimeState.Chasing:
                if (distance > DetectRange)
                {
                    CurrentState = SlimeState.Idle;
                }
                else if (distance <= AttackRange && (currentTime - lastAttackTime >= AttackCool))
                {
                    attackTargetPosition = PlayerTarget.position;
                    StartCoroutine(JumpAttack());
                    CurrentState = SlimeState.Attacking;
                    lastAttackTime = currentTime;
                }
                else
                {
                    Vector2 dir = (PlayerTarget.position - transform.position).normalized;
                    rb.MovePosition(rb.position + dir * FollowSpeed * Time.deltaTime);
                    direction = dir;
                }
                break;
        }
    }

    private void ProcessWaitingState()
    {
        direction = Vector2.zero;
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
        {
            Debug.Log("기다림 끝, 추적 시작");
            CurrentState = SlimeState.Chasing;
        }
    }

    protected override void PickRandomDirection()
    {
        base.PickRandomDirection();
    }

    public override void TakeDamage(int damage, Vector2 hitSource)
    {
        base.TakeDamage(damage, hitSource);
    }


    protected override void OnCollisionStay2D(Collision2D collision)
    {
        if (IsDead) return; 

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerControl player = collision.gameObject.GetComponent<PlayerControl>();
            if (player != null)
            {
                player.TakeDamage(contactDamage, transform.position);
            }
        }
    }

    private IEnumerator JumpAttack()
    {
        direction = (attackTargetPosition - rb.position).normalized;

        while (Vector2.Distance(rb.position, attackTargetPosition) > 0.1f)
        {
            Vector2 newPosition = Vector2.MoveTowards(rb.position, attackTargetPosition, JumpForce * Time.deltaTime);
            rb.MovePosition(newPosition);
            yield return null;
        }

        rb.velocity = Vector2.zero;
        direction = Vector2.zero;

        animator.SetBool("IsMoving", false);

        CurrentState = SlimeState.Waiting;
        waitTimer = waitDuration;
        Debug.Log("돌진 끝, 상태 : waiting 시작");
    }
}
