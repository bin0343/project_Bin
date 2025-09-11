using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Action : MonoBehaviour
{
    [SerializeField]
    private GameObject Player;
    public Animator Animator;
    public Rigidbody Rigidbody;
    private Player_Move Move;

    [Header("Skills")]
    public Skill_Base[] assignedSkills = new Skill_Base[4];
    public SkillHolder[] playerSkills;

    public SkillTargetingController targetingController;
    private bool isTargetingSkill = false;
    public SkillHolder skillBeingAimed;

    public Player_Stat Stat;
    
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

    private IPlayerState currentState;

    void Start()
    {
        Animator = Player.GetComponent<Animator>();
        Rigidbody = GetComponent<Rigidbody>();
        Move = Player.GetComponentInParent<Player_Move>();
        Stat = Player.GetComponentInParent<Player_Stat>();
        SkillUIManagers = FindObjectOfType<UI_SkillManager>();

        targetingController = GetComponent<SkillTargetingController>();
        if (targetingController != null)
        {
            targetingController.OnTargetSelected += FinalizeSkillTargeting;
            targetingController.OnTargetingCancelled += CancelSkillTargeting;
        }

        playerSkills = new SkillHolder[assignedSkills.Length];

        for (int i = 0; i < assignedSkills.Length; i++)
        {
            if (assignedSkills[i] != null)
            {
                playerSkills[i] = new SkillHolder(assignedSkills[i]);
            }
            else
            {
                playerSkills[i] = null;
            }
        }

        if (UI_SkillManager.Instance != null)
        {
            UI_SkillManager.Instance.SetupSkillSlots(playerSkills);
        }
        ChangeState(new PlayerIdleState());
    }

    void Update()
    {
        if (UI_Manager.Instance != null && UI_Manager.Instance.IsUIOpen)
            return;
        if (IsDead) return;
        if (isTargetingSkill || (UI_Manager.Instance != null && UI_Manager.Instance.IsUIOpen))
            return;
        //Shield();
        //Attack();
        //Idle();
        //Sit();
        //Jump();
        //Kick();
        //Die();
        //UseSkill();
        //UseItem();
        currentState?.Execute(this);
    }

    public void ChangeState(IPlayerState newstate)
    {
        currentState?.Exit(this);
        currentState = newstate;
        currentState.Enter(this);
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
        if (isTargetingSkill) return;
        if (slotIndex < 0 || slotIndex >= playerSkills.Length) return;

        // CHANGED: playerSkills[slotIndex]가 비어있는지(null) 확인
        SkillHolder skillToUse = playerSkills[slotIndex];

        if (skillToUse == null || !skillToUse.CanUse(Stat.CurrentMP))
        {
            // 스킬 사용 불가 메시지 (필요시)
            return;
        }

        // 사용하려는 스킬이 '범위 지정 공격' 타입인지 확인
        if (skillToUse.SkillData is Skill_AreaAttack areaSkill)
        {
            isTargetingSkill = true;
            skillBeingAimed = skillToUse; // 어떤 스킬을 조준하는지 기억
            targetingController.EnterTargetingMode(areaSkill, transform); // 조준 모드 시작!
        }
        else // 버프 등 다른 종류의 즉발 스킬일 경우
        {
            skillToUse.Use(gameObject);
            Debug.Log($"[{skillToUse.SkillData.SkillName}] 스킬 즉시 사용!");
        }
    }
    #endregion

    #region Input Handlers
    // 모든 상태 클래스가 호출할 스킬 처리 전용 함수
    public void HandleSkillInput(int slotIndex)
    {
        // 기존 TryUseSkill의 로직을 그대로 가져옵니다.
        if (isTargetingSkill) return;
        if (slotIndex < 0 || slotIndex >= playerSkills.Length) return;

        SkillHolder skillToUse = playerSkills[slotIndex];

        if (skillToUse == null || !skillToUse.CanUse(Stat.CurrentMP)) return;

        if (skillToUse.SkillData is Skill_AreaAttack areaSkill)
        {
            skillBeingAimed = skillToUse;
            ChangeState(new PlayerSkillTargetingState());
        }
        else
        {
            skillToUse.Use(gameObject);
        }
    }

    // --- ADDED: 모든 상태 클래스가 호출할 아이템 처리 전용 함수 ---
    public void HandleItemInput(int slotIndex)
    {
        // 기존 TryUseItem의 로직을 그대로 가져옵니다.
        if (slotIndex < 0 || slotIndex >= Player_Inventory.Instance.quickSlots.Length) return;

        ItemHolder itemToUse = Player_Inventory.Instance.quickSlots[slotIndex];
        if (itemToUse == null) return;

        bool success = itemToUse.Use(gameObject);
        if (success)
        {
            if (itemToUse.ItemData.itemType == ITEMTYPE.Consumable)
            {
                itemToUse.Quantity--;
            }

            if (itemToUse.Quantity <= 0)
            {
                Player_Inventory.Instance.quickSlots[slotIndex] = null;
            }

            if (UI_ItemManager.Instance != null)
            {
                UI_ItemManager.Instance.UpdateSlotUI(slotIndex, Player_Inventory.Instance.quickSlots[slotIndex]);
            }
        }
    }
    #endregion

    #region Targeting Callback
    // 조준이 완료(마우스 좌클릭)되었을 때 호출될 함수
    private void FinalizeSkillTargeting(Vector3 targetPosition)
    {
        if (skillBeingAimed == null) return;

        // 1. 스킬 사용 처리 (MP소모, 쿨타임 시작, 시전 애니메이션)
        skillBeingAimed.Use(gameObject);

        // 2. 실제 스킬 효과(파티클 생성, 피해)는 코루틴으로 처리
        StartCoroutine(SpawnEffectAndDealDamage(targetPosition, skillBeingAimed.SkillData as Skill_AreaAttack));

        // 3. 상태 초기화
        isTargetingSkill = false;
        skillBeingAimed = null;

        ChangeState(new PlayerIdleState());
    }

    // 조준이 취소(마우스 우클릭)되었을 때 호출될 함수
    private void CancelSkillTargeting()
    {
        isTargetingSkill = false;
        skillBeingAimed = null;
        Debug.Log("스킬 조준을 취소했습니다.");

        ChangeState(new PlayerIdleState());
    }

    // 지정된 위치에 스킬 효과를 생성하고 피해를 주는 코루틴
    private IEnumerator SpawnEffectAndDealDamage(Vector3 position, Skill_AreaAttack skillData)
    {
        if (skillData == null || skillData.effectPrefab == null) yield break;

        // 약간의 딜레이 후 효과 생성 및 피해 적용
        yield return new WaitForSeconds(0.5f);

        Instantiate(skillData.effectPrefab, position, Quaternion.identity);

        Collider[] hitEnemies = Physics.OverlapSphere(position, skillData.attackRadius);
        foreach (var enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<Enemy_Stat>()?.TakeDamage(skillData.damageAmount);
            }
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

        bool success = itemToUse.Use(gameObject);

        if (success)
        {
            if (itemToUse.ItemData.itemType == ITEMTYPE.Consumable)
            {
                itemToUse.Quantity--;
            }

            if (itemToUse.Quantity <= 0)
            {
                Debug.Log($"[{itemToUse.ItemData.itemName}]을(를) 모두 사용했습니다.");
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
