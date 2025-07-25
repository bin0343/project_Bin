using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Action : MonoBehaviour
{
    [SerializeField]
    private GameObject Player;
    private Animator Animator;
    private Rigidbody Rigidbody;
    private Player_Move Move;

    private float IdleTimer = 0f;
    private float IdleDelay = 5f;
    private float JumpForce = 5f;
    private float JumpAttackForce = 5.5f;

    private bool IsGrounded = true;
    public bool IsAttacking { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        Animator = Player.GetComponent<Animator>();
        Rigidbody = GetComponent<Rigidbody>();
        Move = Player.GetComponentInParent<Player_Move>();
    }

    // Update is called once per frame
    void Update()
    {
        Shield();
        Attack();
        Idle();
        Sit();
        Jump();
    }

    #region Attack
    void Attack()
    {
        Vector3 MoveDir = Move.CharacterBody.forward;
        Vector3 upward = Vector3.up * JumpAttackForce * 1f;
        Vector3 forward = MoveDir * JumpAttackForce * 0.5f;
        Vector3 JumpDirection = (Vector3.up * JumpForce + MoveDir).normalized;

        if (Animator == null) return;

        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

        bool isInJumpAttack = stateInfo.IsName("JumpAttack");
        bool isInRunningSlash = stateInfo.IsName("RunningSlash");
        IsAttacking = isInJumpAttack || isInRunningSlash;

        if (Input.GetMouseButtonDown(0) && IsGrounded && !Move.IsRunning)
        {
            
            Animator.SetTrigger("IsAttacking");
        }

        if (Input.GetKeyDown(KeyCode.V) && IsGrounded)
        {
            Animator.SetTrigger("IsJumpAttack");
            Rigidbody.AddForce(upward + forward, ForceMode.Impulse);
            IsGrounded = false;
        }

        if (Input.GetMouseButtonDown(0) && Move.IsRunning)
        {
            Animator.SetTrigger("RunningSlash");
            Rigidbody.velocity = MoveDir * Move.CharacterRunSpeed;
        }
    }
    #endregion

    #region Shield
    void Shield()
    {
        if (Animator == null) return;

        if (Input.GetMouseButton(1))
        {
            Animator.SetBool("IsShield", true);
        }
        else
        {
            Animator.SetBool("IsShield", false);
        }
    }
    #endregion

    #region Idle
    void Idle()
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle")) // Idle_SubSM에 태그 "Idle"을 붙여두자
        {
            IdleTimer += Time.deltaTime;
            if (IdleTimer >= IdleDelay)
            {
                int rand = Random.Range(1, 4); // 1~3
                Animator.SetInteger("RandomIdleIndex", rand);
                IdleTimer = 0;
            }
        }
        else
        {
            IdleTimer = 0;
            Animator.SetInteger("RandomIdleIndex", 0);
        }
    }
    #endregion

    #region Sit
    void Sit()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Animator.SetTrigger("SitTrigger");
            Animator.SetBool("IsSitting", true);
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            Animator.SetBool("IsSitting", false);
        }
    }
    #endregion

    #region Jump
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded)
        {
            Animator.SetTrigger("IsJump");
            Rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            IsGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            IsGrounded = true;
        }
    }
    #endregion
}
