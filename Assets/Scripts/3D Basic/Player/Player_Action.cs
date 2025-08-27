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
    public Skill_Base[] assignedSkills = new Skill_Base[4];
    private SkillHolder[] playerSkills;

    /*[Header("Item Quick Slots")]
    public Item_Base[] quickSlotItems = new Item_Base[4];
    private ItemHolder[] itemSlots;*/

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

        /*itemSlots = new ItemHolder[quickSlotItems.Length];
        for (int i = 0; i < quickSlotItems.Length; i++)
        {
            if (quickSlotItems[i] != null)
            {
                // 인스펙터에 할당된 아이템으로 ItemHolder를 생성
                // (임시로 소모품은 5개씩 가진다고 가정)
                itemSlots[i] = new ItemHolder(quickSlotItems[i], 5);
            }
        }

        if (UI_ItemManager.Instance != null)
        {
            UI_ItemManager.Instance.SetupItemSlots(itemSlots);
        }*/
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
        UseItem();
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

    #region UseItem
    // --- 아이템 사용 로직 추가 ---
    void UseItem()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) TryUseItem(0); // 숫자키 1
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryUseItem(1); // 숫자키 2
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryUseItem(2); // 숫자키 3
        if (Input.GetKeyDown(KeyCode.Alpha4)) TryUseItem(3); // 숫자키 4
    }

    void TryUseItem(int slotIndex)
    {
        // CHANGED: Player_Inventory의 퀵슬롯 데이터를 직접 참조
        if (slotIndex < 0 || slotIndex >= Player_Inventory.Instance.quickSlots.Length) return;

        ItemHolder itemToUse = Player_Inventory.Instance.quickSlots[slotIndex];

        if (itemToUse == null)
        {
            Debug.Log($"[{slotIndex + 1}]번 퀵슬롯에 아이템이 없습니다.");
            return;
        }

        itemToUse.Use(gameObject);

        if (itemToUse.Quantity <= 0)
        {
            Debug.Log($"[{itemToUse.ItemData.itemName}]을(를) 모두 사용했습니다.");
            // CHANGED: Player_Inventory의 데이터를 직접 수정
            Player_Inventory.Instance.quickSlots[slotIndex] = null;
        }
        else
        {
            Debug.Log($"[{itemToUse.ItemData.itemName}] 사용! 남은 개수: {itemToUse.Quantity}");
        }

        // UI 갱신 요청
        if (UI_ItemManager.Instance != null)
        {
            UI_ItemManager.Instance.UpdateSlotUI(slotIndex, Player_Inventory.Instance.quickSlots[slotIndex]);
        }
    }
    #endregion

    /*#region Slot Swapping
    // --- 데이터 교환 로직 추가 ---
    public void SwapSkill(int indexA, int indexB)
    {
        // 인덱스가 유효한지 확인
        if (indexA < 0 || indexA >= playerSkills.Length || indexB < 0 || indexB >= playerSkills.Length) return;

        // 데이터 교환
        SkillHolder temp = playerSkills[indexA];
        playerSkills[indexA] = playerSkills[indexB];
        playerSkills[indexB] = temp;

        // 데이터가 변경되었으니 UI를 새로고침
        UI_SkillManager.Instance.SetupSkillSlots(playerSkills);
    }

    public void SwapItem(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= itemSlots.Length || indexB < 0 || indexB >= itemSlots.Length) return;

        ItemHolder temp = itemSlots[indexA];
        itemSlots[indexA] = itemSlots[indexB];
        itemSlots[indexB] = temp;

        UI_ItemManager.Instance.SetupItemSlots(itemSlots);
    }
    #endregion*/

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
