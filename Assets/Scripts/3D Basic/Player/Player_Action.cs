using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player_Action : MonoBehaviour
{
    [SerializeField] private GameObject player;
    public Animator animator {  get; private set; }
    public Player_AnimationEvents animEvents { get; private set; }
    public new Rigidbody rigidbody { get; private set; }
    public Player_Move move;

    [Header("Skills")]
    public Skill_Base[] assignedSkills = new Skill_Base[4];
    public SkillHolder[] playerSkills;
    [HideInInspector] public SkillHolder activeCastingSkill;

    public Character_Stat stat;
    public UI_SkillManager skillUIManagers;
    public Weapon_Player currentWeapon { get; private set; }

    [Header("공격 시 이동(커브 기반)")]
    [Tooltip("1타, 2타, 3타에 해당하는 전진 커브 (X: 0~1 정규화된 시간, Y: 누적 전진량)")]
    public AnimationCurve[] attackMoveCurves = new AnimationCurve[3];
    public float attackMoveDistance = 30f;

    [Header("오토 타겟팅 설정")]
    [Tooltip("공격 시 적 탐색 반경")]
    public float targetSearchRadius = 30f;
    [Tooltip("몬스터들 레이어")]
    public LayerMask enemyLayer;

    [Header("아이템 줍기 반경")]
    public float pickupRadius = 3.0f;
    public LayerMask itemLayer;

    [Header("땅 감지(점프)")]
    public Transform groundCheckPos; // 발바닥 위치 (Inspector에서 할당 필요, 없으면 transform.position 사용)
    public float groundCheckDistance = 0.2f;
    public float fallMultiplier = 2.5f;     // 떨어질 때 가속도
    public float lowJumpMultiplier = 2.0f;  // 스페이스바를 짧게 눌렀을 때의 가속도
    public LayerMask groundLayer;

    private float coyoteTime = 0.15f;
    private float coyoteTimer = 0f;
    private float jumpGraceTimer = 0f;
    private float airborneTimer = 0f;
    private float fallAnimationDelay = 2f;

    [HideInInspector] public bool IsGrounded = true;
    [HideInInspector] public bool IsDead = false;
    [HideInInspector] public bool IsKick = false;
    [HideInInspector] public bool IsAttacking = false;
    [HideInInspector] public bool IsBuff = false;
    [HideInInspector] public bool canReceiveInput = true; // 입력을 받을 수 있는 상태인지
    [HideInInspector] public bool CanRotate = true;
    [HideInInspector] public bool IsInvincible = false; //무적상태(구르기)
    [HideInInspector] public bool comboQueued = false;

    public IPlayerState currentState;
    public int currentComboStep { get; private set; }

    public bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        animator = player.GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        move = player.GetComponentInParent<Player_Move>();
        stat = player.GetComponentInParent<Character_Stat>();
        animEvents = player.GetComponent<Player_AnimationEvents>();
        skillUIManagers = FindObjectOfType<UI_SkillManager>();

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

        ChangeState(new PlayerIdleState());
    }

    void Start()
    {
        if (UI_SkillManager.Instance != null)
        {
            UI_SkillManager.Instance.SetupSkillSlots(playerSkills);
        }

        if (BattleManager.instance != null)
        {
            BattleManager.instance.ChangeCameraTarget(transform.root);
        }
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
       
        if (IsDead) return;

        if (UI_Manager.instance != null && UI_Manager.instance.IsUIOpen)
        {
            if (move != null) move.ForceMove(Vector3.zero, 0f);
            return;
        }

        currentState?.Execute(this);
        CheckGroundStatus();

        if (IsGrounded)
        {
            airborneTimer = 0f; // 땅에 닿으면 타이머 초기화
            animator.SetBool("IsGrounded", true); // 즉시 착지 모션 재생
        }
        else
        {
            if (rigidbody.velocity.y <= 0.1f)
            {
                airborneTimer += Time.deltaTime;
            }
            else
            {
                if (airborneTimer < 10f)
                {
                    airborneTimer = 0f;
                }
            }

            if (airborneTimer >= 10f || (airborneTimer > fallAnimationDelay && rigidbody.velocity.y < -0.1f))
            {
                animator.SetBool("IsGrounded", false);
            }
        }

        animator.SetFloat("VerticalVelocity", rigidbody.velocity.y);
    }

    private void OnEnable()
    {
        if (UI_SkillManager.Instance != null)
        {
            UI_SkillManager.Instance.SetupSkillSlots(playerSkills);
        }
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

    public void ForceJumpAirborne()
    {
        IsGrounded = false;
        coyoteTime = 0f;
        jumpGraceTimer = 0.1f;
        airborneTimer = 10f;
    }

    private void CheckGroundStatus()
    {
        if (jumpGraceTimer > 0f)
        {
            jumpGraceTimer -= Time.deltaTime;
            IsGrounded = false;
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        if (groundCheckPos != null)
        {
            origin = groundCheckPos.position + Vector3.up * 0.3f;
        }

        float currentCheckDist = IsGrounded ? (groundCheckDistance + 0.3f) : groundCheckDistance;
        float checkDist = currentCheckDist + 0.5f;
        float sphereRadius = 0.25f;

        if (Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hit, checkDist, groundLayer))
        {
            IsGrounded = true;
            coyoteTimer = coyoteTime;
        }
        else
        {
            if (coyoteTimer > 0f)
            {
                coyoteTimer -= Time.deltaTime;
                IsGrounded = true;
            }
            else
            {
                IsGrounded = false;
            }
        }
    }

    #region Input Handlers
    public void HandleSkillInput(int slotIndex)
    {
        Player_Equipment myEquipment = GetComponent<Player_Equipment>();
        if (myEquipment != null)
        {
            myEquipment.EnterCombatState();
        }

        if (slotIndex < 0 || slotIndex >= playerSkills.Length) return;

        SkillHolder skillToUse = playerSkills[slotIndex];

        if (skillToUse == null || !skillToUse.CanUse()) return;

        ChangeState(new PlayerCastingState(skillToUse)); 
    }

    public void HandleItemInput(int slotIndex)
    {
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

    public void ExecuteSkillEffectEvent()
    {
        if (activeCastingSkill != null)
        {
            activeCastingSkill.SkillData.ApplySkillEffects(gameObject);
            activeCastingSkill = null;
        }
    }

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

    public void SetComboStep(int step)
    {
        currentComboStep = step;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.15f); // 반투명 푸른색
        Gizmos.DrawSphere(transform.position, attackMoveDistance);

        Gizmos.color = new Color(0f, 0.5f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, attackMoveDistance);
    }
}

public interface IStateAnimationEvents
{
    void OnAnimationEvent(Player_Action.AnimationEventType eventType, Player_Action player);
}

