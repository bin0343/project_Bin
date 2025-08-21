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

    [Header("Skills")]
    // CHANGED: 인스펙터에서 스킬 '설계도'들을 여기에 할당합니다.
    public Skill_Base[] assignedSkills = new Skill_Base[4];
    // ADDED: 플레이어가 실제로 소유하고 상태를 관리할 스킬 목록
    private SkillHolder[] playerSkills;
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

        playerSkills = new SkillHolder[assignedSkills.Length];

        for (int i = 0; i < assignedSkills.Length; i++)
        {
            // 할당된 스킬 에셋이 있는 칸만 SkillHolder를 생성
            if (assignedSkills[i] != null)
            {
                playerSkills[i] = new SkillHolder(assignedSkills[i]);
            }
            // 할당되지 않은 칸은 null로 유지
            else
            {
                playerSkills[i] = null;
            }
        }

        if (UI_SkillManager.Instance != null)
        {
            UI_SkillManager.Instance.SetupSkillSlots(playerSkills);
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
        if (slotIndex < 0 || slotIndex >= playerSkills.Length) return;

        // CHANGED: playerSkills[slotIndex]가 비어있는지(null) 확인
        SkillHolder skillToUse = playerSkills[slotIndex];

        if (skillToUse == null)
        {
            Debug.Log($"[{slotIndex}]번 슬롯이 비어있습니다.");
            return;
        }

        if (skillToUse.CanUse(Stat.CurrentMP))
        {
            skillToUse.Use(gameObject);
            Debug.Log($"[{skillToUse.SkillData.SkillName}] 스킬 사용!");
        }
        else
        {
            Debug.Log($"[{skillToUse.SkillData.SkillName}] 스킬 사용 불가 (쿨타임 or MP 부족).");
        }
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
