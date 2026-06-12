using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player_Action : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    public Animator animator {  get; private set; }
    public Player_AnimationEvents animEvents { get; private set; }
    //public PlayerAttackHitbox attackHitbox { get; private set; }
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
    public Weapon_Player currentWeapon { get; private set; }

    [Header("아이템 줍기 반경")]
    public float pickupRadius = 3.0f;
    public LayerMask itemLayer;

    [Header("땅 감지(점프)")]
    public Transform groundCheckPos; // 발바닥 위치 (Inspector에서 할당 필요, 없으면 transform.position 사용)
    public float groundCheckDistance = 0.2f;
    public float fallMultiplier = 2.5f;     // 떨어질 때 가속도
    public float lowJumpMultiplier = 2.0f;  // 스페이스바를 짧게 눌렀을 때의 가속도


    [HideInInspector] public bool IsGuarding { get; private set; } = false;
    [HideInInspector] public bool IsGrounded = true;
    [HideInInspector] public bool IsDead = false;
    [HideInInspector] public bool IsKick = false;
    [HideInInspector] public bool IsAttacking = false;
    [HideInInspector] public bool IsBuff = false;
    [HideInInspector] public bool canReceiveInput = true; // 입력을 받을 수 있는 상태인지
    [HideInInspector] public bool CanRotate = true;
    [HideInInspector] public bool IsInvincible = false; //무적상태(구르기)

    public static event Action<Sprite, float> OnRunningAttackUsed;
    public IPlayerState currentState;
    public int currentComboStep { get; private set; }

    public bool IsPointerOverUI()       //마우스가 ui위에 있는지 확인
    {
        // EventSystem이 없으면 false 반환 (에러 방지)
        if (EventSystem.current == null) return false;

        // 마우스 포인터가 UI 요소(Raycast Target이 켜진 패널/버튼 등) 위에 있으면 true 반환
        return EventSystem.current.IsPointerOverGameObject();
    }

    void Start()
    {
        animator = player.GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        move = player.GetComponentInParent<Player_Move>();
        stat = player.GetComponentInParent<Player_Stat>();
        animEvents = player.GetComponent<Player_AnimationEvents>();
        skillUIManagers = FindObjectOfType<UI_SkillManager>();
        shield = player.GetComponentInChildren<Shield_Player>();
        //attackHitbox = GetComponentInChildren<PlayerAttackHitbox>(true);

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

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            TryPickUpNearbyItems();
        }
        if (stat.currentHP <= 0 && !(currentState is PlayerDeadState))
        {
            ChangeState(new PlayerDeadState());
            return; 
        }
        /*if (UI_Manager.Instance != null && UI_Manager.Instance.IsUIOpen)
            return;*/
        if (IsDead) return;
        if (isTargetingSkill)
            return;
        //HandleGuardInput();
        currentState?.Execute(this);

        CheckGroundStatus();
        animator.SetBool("IsGrounded", IsGrounded);
        animator.SetFloat("VerticalVelocity", rigidbody.velocity.y);
    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        if (rigidbody.velocity.y < 0)
        {
            // 떨어질 때 중력을 강하게
            rigidbody.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rigidbody.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            // 올라가는 중인데 점프 키를 뗐다면 (소점프)
            rigidbody.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    public void ChangeState(IPlayerState newstate)
    {
        if (newstate is PlayerHitState || newstate is PlayerIdleState || newstate is PlayerDeadState || newstate is PlayerRollState)
        {
            currentWeapon?.StopTrail();
            currentWeapon?.DisableHitbox();
            IsAttacking = false;
        }
        currentState?.Exit(this);
        currentState = newstate;
        currentState.Enter(this);
    }

    private void CheckGroundStatus()
    {
        // 점프 시작 직후(Y속도가 양수)에는 땅 체크를 잠시 무시해야 "점프하자마자 착지"하는 버그를 막을 수 있음
        if (rigidbody.velocity.y > 0.1f)
        {
            IsGrounded = false;
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        if (groundCheckPos != null)
        {
            origin = groundCheckPos.position + Vector3.up * 0.5f;
        }

        float checkDist = groundCheckDistance + 0.5f;
        float sphereRadius = 0.2f;

        // 아래로 레이를 쏴서 Ground 레이어에 닿으면 땅에 있는 것임
        // *주의: Player_Move의 groundLayer 설정을 활용하거나 직접 레이어 마스크 지정 필요
        // 여기서는 일단 모든 레이어 검사 혹은 move 스크립트의 groundLayer 참조 권장
        if (Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hit, checkDist))
        {
            IsGrounded = true;
        }
        else
        {
            IsGrounded = false;
        }
    }

    #region Input Handlers
    // 모든 상태 클래스가 호출할 스킬 처리 전용 함수
    public void HandleSkillInput(int slotIndex)
    {
        if (Player_Equipment.instance != null)
        {
            Player_Equipment.instance.EnterCombatState();
        }

        if (isTargetingSkill) return;
        if (slotIndex < 0 || slotIndex >= playerSkills.Length) return;

        SkillHolder skillToUse = playerSkills[slotIndex];

        if (skillToUse == null || !skillToUse.CanUse()) return;

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

    private void TryPickUpNearbyItems()
    {
        // 플레이어 주변의 아이템 레이어 물체 감지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupRadius, itemLayer);

        bool pickedUpAny = false;

        foreach (var hitCollider in hitColliders)
        {
            FieldItem fieldItem = hitCollider.GetComponent<FieldItem>();
            if (fieldItem != null)
            {
                Item_Base itemData = fieldItem.GetItem();

                // 아이템 데이터가 있고, 인벤토리에 추가 성공했다면
                if (itemData != null && Player_Inventory.instance.AddItem(itemData))
                {
                    Debug.Log($"아이템 획득: {itemData.itemName}");
                    fieldItem.DestroyItem(); // 필드 오브젝트 삭제
                    pickedUpAny = true;

                    break;
                    // 만약 한 번에 다 줍게 하려면 break를 지우기.
                    // 하나씩 줍게 하려면 여기서 break; 
                }
                else if (itemData != null)
                {
                    Debug.Log("인벤토리가 가득 찼습니다.");
                    // 가득 찼다는 메시지 띄우기 (UI_Manager 활용)
                    break;
                }
            }
        }
    }

    public void SetCurrentWeapon(Weapon_Player newWeapon)
    {
        currentWeapon = newWeapon;
    }

    public void OnAttackBlocked()
    {
        // 이미 막혔거나, 죽었거나, 다른 리액션 중일 때는 무시
        if (currentState is PlayerHitState || IsDead)
        {
            return;
        }

        Debug.Log("공격이 막힘! 상태를 AttackBlocked로 변경합니다.");
        ChangeState(new PlayerIdleState());

        // 2. 공격 관련 상태들을 확실하게 정리해줍니다.
        IsAttacking = false;
        currentWeapon?.DisableHitbox();
        currentWeapon?.StopTrail();
    }

    public bool CanUseRunningAttack()
    {
        if (runningAttackHolder == null) return false;
        return runningAttackHolder.CanUse();
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
        if (IsInvincible) return;

        if (!IsDead)
        {
            currentWeapon?.ForceStopTrail();
            currentWeapon?.DisableHitbox();
            IsAttacking = false;

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
