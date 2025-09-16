using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Action : MonoBehaviour
{
    [SerializeField]
    private GameObject Player;
    public Animator Animator {  get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Player_Move Move;

    [Header("Skills")]
    public Skill_Base[] assignedSkills = new Skill_Base[4];
    public SkillHolder[] playerSkills;

    public SkillTargetingController targetingController;
    private bool isTargetingSkill = false;
    public SkillHolder skillBeingAimed;

    public Player_Stat Stat;
    
    public UI_SkillManager SkillUIManagers;
    
    public bool IsGrounded = true;
    public bool IsDead = false;
    public bool IsKick = false;
    public bool IsAttacking = false;
    public bool IsBuff = false;

    public bool canReceiveInput = true; // 입력을 받을 수 있는 상태인지

    public IPlayerState currentState;

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
        if (Stat.CurrentHP <= 0 && !(currentState is PlayerDeadState))
        {
            ChangeState(new PlayerDeadState());
            return; 
        }
        if (UI_Manager.Instance != null && UI_Manager.Instance.IsUIOpen)
            return;
        if (IsDead) return;
        if (isTargetingSkill || (UI_Manager.Instance != null && UI_Manager.Instance.IsUIOpen))
            return;
        
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

    // --- ADDED: 애니메이션 이벤트를 현재 상태에 전달하는 중개 함수 ---
    public void OnAnimationEvent(AnimationEventType eventType)
    {
        (currentState as IStateAnimationEvents)?.OnAnimationEvent(eventType);
    }
}

public interface IStateAnimationEvents
{
    void OnAnimationEvent(Player_Action.AnimationEventType eventType);
}
