using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Action : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    public Animator animator {  get; private set; }
    public Player_AnimationEvents animEvents { get; private set; }
    public PlayerAttackHitbox attackHitbox { get; private set; }
    public new Rigidbody rigidbody { get; private set; }
    public Player_Move move;

    [Header("Skills")]
    public Skill_Base[] assignedSkills = new Skill_Base[4];
    public Skill_BasicSkill runningAttackData;
    public SkillHolder[] playerSkills;

    private SkillHolder runningAttackHolder;

    public SkillTargetingController targetingController;
    private bool isTargetingSkill = false;
    public SkillHolder skillBeingAimed;

    public Player_Stat stat;
    
    public UI_SkillManager skillUIManagers;
    public Shield_Player shield;

    public bool IsGuarding { get; private set; } = false;
    public bool IsGrounded = true;
    public bool IsDead = false;
    public bool IsKick = false;
    public bool IsAttacking = false;
    public bool IsBuff = false;

    public bool canReceiveInput = true; // 입력을 받을 수 있는 상태인지

    public static event Action<Sprite, float> OnRunningAttackUsed;
    public IPlayerState currentState;
    public int currentComboStep { get; private set; }

    void Start()
    {
        animator = player.GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        move = player.GetComponentInParent<Player_Move>();
        stat = player.GetComponentInParent<Player_Stat>();
        animEvents = player.GetComponent<Player_AnimationEvents>();
        skillUIManagers = FindObjectOfType<UI_SkillManager>();
        shield = player.GetComponentInChildren<Shield_Player>();

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

        if (runningAttackData != null)
        {
            runningAttackHolder = new SkillHolder(runningAttackData);
        }

        if (UI_SkillManager.Instance != null)
        {
            UI_SkillManager.Instance.SetupSkillSlots(playerSkills);
        }
        ChangeState(new PlayerIdleState());
    }

    void Update()
    {
        if (stat.currentHP <= 0 && !(currentState is PlayerDeadState))
        {
            ChangeState(new PlayerDeadState());
            return; 
        }
        if (UI_Manager.Instance != null && UI_Manager.Instance.IsUIOpen)
            return;
        if (IsDead) return;
        if (isTargetingSkill || (UI_Manager.Instance != null && UI_Manager.Instance.IsUIOpen))
            return;
        HandleGuardInput();
        currentState?.Execute(this);
    }

    public void ChangeState(IPlayerState newstate)
    {
        currentState?.Exit(this);
        currentState = newstate;
        currentState.Enter(this);
    }
    
    #region Input Handlers
    // 모든 상태 클래스가 호출할 스킬 처리 전용 함수
    public void HandleSkillInput(int slotIndex)
    {
        if (isTargetingSkill) return;
        if (slotIndex < 0 || slotIndex >= playerSkills.Length) return;

        SkillHolder skillToUse = playerSkills[slotIndex];

        if (skillToUse == null || !skillToUse.CanUse(stat.currentMP)) return;

        if (skillToUse.SkillData is Skill_AreaAttack areaSkill)
        {
            skillBeingAimed = skillToUse;
            ChangeState(new PlayerSkillTargetingState());
        }
        else
        {
            //skillToUse.Use(gameObject);
            ChangeState(new PlayerCastingState(skillToUse));
        }
    }

    // --- ADDED: 모든 상태 클래스가 호출할 아이템 처리 전용 함수 ---
    public void HandleItemInput(int slotIndex)
    {
        // 기존 TryUseItem의 로직을 그대로 가져옵니다.
        if (slotIndex < 0 || slotIndex >= Player_Inventory.instance.quickSlots.Length) return;

        ItemHolder itemToUse = Player_Inventory.instance.quickSlots[slotIndex];
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
                Player_Inventory.instance.quickSlots[slotIndex] = null;
            }

            if (UI_ItemManager.Instance != null)
            {
                UI_ItemManager.Instance.UpdateSlotUI(slotIndex, Player_Inventory.instance.quickSlots[slotIndex]);
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
                enemy.GetComponent<Enemy_Stat>()?.TakeDamage(skillData.damageAmount, AttackType.None);
            }
        }
    }
    #endregion

    private void HandleGuardInput()
    {
        // 방패 들기 (마우스 우클릭 누르는 순간)
        if (Input.GetMouseButtonDown(1))
        {
            StartGuarding();
        }
        // 방패 내리기 (마우스 우클릭 떼는 순간)
        else if (Input.GetMouseButtonUp(1))
        {
            StopGuarding();
        }
    }

    public void OnAttackBlocked()
    {
        // 이미 막혔거나, 죽었거나, 다른 리액션 중일 때는 무시
        if (currentState is PlayerAttackBlockState || currentState is PlayerHitState || IsDead)
        {
            return;
        }

        Debug.Log("공격이 막힘! 상태를 AttackBlocked로 변경합니다.");
        ChangeState(new PlayerAttackBlockState());
    }

    public void StartGuarding()
    {
        if (IsGuarding) return; // 이미 방어 중이면 무시

        IsGuarding = true;
        Debug.Log("방어 시작");

        animator.SetBool("IsGuarding", true);

        shield?.SetActiveShield(true);
    }

    public void StopGuarding()
    {
        if (!IsGuarding) return; // 방어 중이 아니면 무시

        IsGuarding = false;
        Debug.Log("방어 중지");

        animator.SetBool("IsGuarding", false);

        shield?.SetActiveShield(false);
    }

    public bool CanUseRunningAttack()
    {
        if (runningAttackHolder == null) return false;
        return runningAttackHolder.CanUse(0);
    }

    public void SetComboStep(int step)
    {
        currentComboStep = step;
    }

    public void UseRunningAttack()
    {
        if (runningAttackHolder == null) return;

        runningAttackHolder.Use(gameObject);

        if (runningAttackData != null)
        {
            OnRunningAttackUsed?.Invoke(runningAttackData.skillIcon, runningAttackData.cooldownTime);
        }
    }

    public void OnDamageTaken()
    {
        if (!IsDead)
        {
            ChangeState(new PlayerHitState());
        }
    }


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

    public enum AnimationEventType
    {
        COMBO_WINDOW_OPEN,
        ATTACK_ANIMATION_END
    }

    public void OnAnimationEvent(AnimationEventType eventType)
    {
        (currentState as IStateAnimationEvents)?.OnAnimationEvent(eventType, this);
    }
}

public interface IStateAnimationEvents
{
    void OnAnimationEvent(Player_Action.AnimationEventType eventType, Player_Action player);
}
