using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonControl : GreenSlimeControl
{
    private enum SkeletionState { Idle, Chasing }
    private SkeletionState CurrentState = SkeletionState.Idle;

    private int lastDirection = 1;

    public float DetectRange = 50f;
    //public float AttackRange = 5f;
    public float FollowSpeed = 10f;
    public float ChaseMemoryTime = 2f;
    private float ChaseTimer = 0f;
    private Transform PlayerTarget;

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

        float distance = Vector2.Distance(transform.position, PlayerTarget.position);

        switch (CurrentState)
        {
            case SkeletionState.Idle:
                if (distance <= DetectRange)
                {
                    CurrentState = SkeletionState.Chasing;
                    ChaseTimer = ChaseMemoryTime;
                }
                direction = Vector2.zero;
                break;

            case SkeletionState.Chasing:
                if (distance <= DetectRange)
                {
                    ChaseTimer = ChaseMemoryTime;
                }
                else
                {
                    ChaseTimer -= Time.deltaTime;
                    if (ChaseTimer <= 0f)
                    {
                        CurrentState = SkeletionState.Idle;
                        direction = Vector2.zero;
                        break;
                    }
                }

                Vector2 dir = (PlayerTarget.position - transform.position).normalized;
                rb.MovePosition(rb.position + dir * FollowSpeed * Time.deltaTime);
                direction = dir;
                break;
        }

        if (direction != Vector2.zero)
        {
            animator.SetBool("IsMoving", true);

            int currentdir;
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                currentdir = direction.x > 0 ? 3 : 2;
                SpriteRenderer.flipX = direction.x < 0;
            }
            else
            {
                currentdir = direction.y > 0 ? 0 : 1;
            }

            animator.SetInteger("Direction", currentdir);
            lastDirection = currentdir;
        }
        else
        {
            animator.SetBool("IsMoving", false);
            animator.SetInteger("Direction", lastDirection);
        }
    }

    protected override void PickRandomDirection()
    {
        base.PickRandomDirection();
    }

    protected override void OnCollisionStay2D(Collision2D collision)
    {
        if (IsDead) return; 

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerControl player = collision.gameObject.GetComponent<PlayerControl>();
            if (player != null)
            {
                player.TakeDamage(stat.Attack, transform.position);
            }
        }
    }

    public override void TakeDamage(int damage, Vector2 hitSource)
    {
        base.TakeDamage(damage, hitSource);
    }
}
