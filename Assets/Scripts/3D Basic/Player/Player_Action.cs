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
    private Player_Stat Stat;

    //public Skill_Base[] Skill;
    //public Sprite[] SkillIcons;
    public UI_SkillManager SkillUIManagers;

    private float IdleTimer = 0f;
    private float IdleDelay = 5f;
    private float JumpForce = 5f;
    
    public bool IsGrounded = true;
    public bool IsDead = false;
    public bool IsKick = false;
    public bool IsAttacking = false;
    public bool IsBuff = false;

    public bool canReceiveInput = true; // 입력을 받을 수 있는 상태인지

    void Start()
    {
        Animator = Player.GetComponent<Animator>();
        Rigidbody = GetComponent<Rigidbody>();
        Move = Player.GetComponentInParent<Player_Move>();
        Stat = Player.GetComponentInParent<Player_Stat>();
        SkillUIManagers = FindObjectOfType<UI_SkillManager>();

        for (int i = 0; i < SkillUIManagers.skillSlots.Length; i++)
        {
            var slot = SkillUIManagers.skillSlots[i];
            if (slot != null && slot.assignedSkill != null)
            {
                slot.assignedSkill.ResetSkill(); // 쿨타임 초기화
                slot.Setup(slot.assignedSkill);
            }
        }
    }

    void Update()
    {
        if (IsDead) return;
        Shield();
        Attack();
        Idle();
        Sit();
        Jump();
        Kick();
        Die();
        UseSkill();
    }

    #region Attack
    void Attack()
    {
        Vector3 MoveDir = Move.CharacterBody.forward;

        if (Animator == null) return;

        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

        bool isInAttackState = stateInfo.IsTag("Attack");
        bool isInAttack3 = stateInfo.IsName("Attack3");  // Attack3 상태 체크

        if (isInAttackState)
            IsAttacking = true;
        else
            IsAttacking = false;

        if (isInAttack3 && !canReceiveInput)
            return;

        if (Input.GetMouseButtonDown(0) && IsGrounded && !Move.IsRunning && canReceiveInput)
        {
            Animator.SetTrigger("IsAttacking");
            canReceiveInput = false;
        }

        if (Input.GetKeyDown(KeyCode.V) && IsGrounded)
        {
            Animator.SetTrigger("IsJumpAttack");
            //Rigidbody.AddForce(upward + forward, ForceMode.Impulse);
            IsGrounded = false;
        }

        if (Input.GetMouseButtonDown(0) && IsGrounded && Move.IsRunning)
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
    #endregion

    #region kick
    void Kick()
    {
        if (Input.GetKeyDown (KeyCode.F))
        {
            Animator.SetTrigger("IsKick");
            IsKick = true;
        }
    }

    #endregion

    #region Die
    void Die()
    {
        //if (IsDead) return;

        if (Stat.CurrentHP <= 0)
        {
            Animator.SetTrigger("IsDie");
            IsDead = true;
        }
    }
    #endregion

    #region UseSkill
    void UseSkill()
    {
        if (!IsGrounded) return;

        // 슬롯 번호와 키 매핑
        if (Input.GetKeyDown(KeyCode.F1)) TryUseSkill(0);
        if (Input.GetKeyDown(KeyCode.F2)) TryUseSkill(1);
        if (Input.GetKeyDown(KeyCode.F3)) TryUseSkill(2);
        if (Input.GetKeyDown(KeyCode.F4)) TryUseSkill(3);
    }

    void TryUseSkill(int slotIndex)
    {
        if (SkillUIManagers == null)
        {
            Debug.LogError("SkillUIManager가 할당되지 않았습니다.");
            return;
        }

        if (slotIndex < 0 || slotIndex >= SkillUIManagers.skillSlots.Length)
        {
            Debug.Log($"[{slotIndex}]번 슬롯 인덱스 범위 초과");
            return;
        }

        var slot = SkillUIManagers.skillSlots[slotIndex];

        if (slot == null)
        {
            Debug.Log($"[{slotIndex}]번 슬롯 UI가 비어있습니다.");
            return;
        }

        var skill = slot.GetSkill();

        if (skill == null)
        {
            Debug.Log($"[{slotIndex}]번 슬롯에 스킬이 없습니다."); // 스킬 없을 때 로그
            return;
        }

        var stat = GetComponent<Player_Stat>();
        if (!skill.CanUse(stat.CurrentMP))
        {
            Debug.Log($"[{skill.SkillName}] 스킬이 아직 사용 불가 (쿨타임 or MP 부족).");
            return;
        }

        skill.Use(gameObject);
        slot.StartCooldown();
    }
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            IsGrounded = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Weapon_Enemy"))
        {
            Debug.Log("피격당함");
        }
    }
}
