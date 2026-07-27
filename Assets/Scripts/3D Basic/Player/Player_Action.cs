using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public enum HitReactionType
{
    None,   //데미지만
    Normal, //데미지 + Idle상태일때 확률로 HitState
    Stagger,    //HitState(강제 경직)
    Knockback,   //HitState + 넉백
    LaunchKnockDown  //넘어지면서 뒤로 날아감
}

public class Player_Action : MonoBehaviour
{
    [SerializeField] private GameObject player;
    public Animator animator {  get; private set; }
    public Player_AnimationEvents animEvents { get; private set; }
    public new Rigidbody rigidbody { get; private set; }
    public Player_Move move;

    [Header("Skills")]
    public Skill_Base[] assignedSkills = new Skill_Base[2];
    public SkillHolder[] playerSkills;
    [HideInInspector] public SkillHolder activeCastingSkill;

    [Header("스킬 입력 키")]
    [SerializeField]
    private KeyCode[] skillKeys = { KeyCode.E, KeyCode.Q };

    public Character_Stat stat;
    public UI_SkillManager skillUIManagers;
    public Weapon_Player currentWeapon { get; private set; }

    [Header("공격 시 이동(커브 기반)")]
    [Tooltip("1타, 2타, 3타에 해당하는 전진 커브 (X: 0~1 정규화된 시간, Y: 누적 전진량)")]
    public AnimationCurve[] attackMoveCurves = new AnimationCurve[3];
    public float attackMoveDistance = 30f;
    public float attackDashStopDistance = 0.8f;
    public float attackDashCurveSpeed = 1f; // 커브를 읽는 속도, 1이면 기본, 1보다 크면 빠르게

    [Header("오토 타겟팅 설정")]
    [Tooltip("몬스터들 레이어")]
    public LayerMask enemyLayer;

    [Header("아이템 줍기 반경")]
    public float pickupRadius = 3.0f;
    public LayerMask itemLayer;

    [Header("땅 감지(점프)")]
    public Transform groundCheckPos; // 발바닥 위치 (Inspector에서 할당 필요, 없으면 transform.position 사용)
    public float groundCheckDistance = 0.2f;
    public float fallMultiplier = 2.5f;     // 떨어질 때 가속도
    public LayerMask groundLayer;

    [Header("콤보 입력")]
    public float comboInputBufferTime = 0.2f;   //콤보 입력 저장 시간
    [HideInInspector] public float lastComboInputTime = -999f;
    [HideInInspector] public bool comboQueued = false;

    [Header("피격 리액션")]
    [SerializeField, Range(0f, 1f)] private float idleHitReactionChance = 0.35f;
    [SerializeField] private float hitReactionCooldown = 0.7f;
    private float lastHitReactionTime = -999f;

    [Header("피격 넉백")]
    [SerializeField] private float enemyKnockbackDistance = 2.5f;
    [SerializeField] private float enemyKnockbackDuration = 0.25f;

    [Header("런치 넉다운")]
    [SerializeField] private float launchKnockDownDistance = 5f;
    [SerializeField] private float launchKnockDownHeight = 2.2f;
    [SerializeField] private float launchKnockDownDuration = 0.65f;
    [Tooltip("돌진 공격을 옆에서 맞았을 때 대각선으로 날아가는 정도")]
    [SerializeField, Range(0f, 1.5f)]
    private float launchSideInfluence = 0.8f;

    [Header("피격 이동 충돌 보정")]
    [SerializeField] private LayerMask hitReactionObstacleLayer;
    [SerializeField] private float hitReactionCheckRadius = 0.35f;
    [SerializeField] private float hitReactionCheckHeight = 0.8f;
    [SerializeField] private float hitReactionWallBuffer = 0.1f;

    private Coroutine enemyHitMoveCoroutine;

    private bool isHitReactionMotionActive;
    private bool hitAnimationEnded;

    [Header("공격 대쉬 충돌 보정")]
    [SerializeField] private LayerMask attackDashObstacleLayer;
    [SerializeField] private float attackDashCheckRadius = 0.35f;
    [SerializeField] private float attackDashCheckHeight = 0.8f;
    [SerializeField] private float attackDashWallBuffer = 0.1f;

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

    [Header("극한 회피")]
    [Tooltip("극한회피 성공 시 느려지는 시간 배율")]
    public float perfectEvadeTimeScale = 0.25f;
    [Tooltip("극한회피 슬로우 지속 시간. 실제 시간 기준")]
    public float perfectEvadeSlowDuration = 0.25f;
    [Tooltip("극한회피 성공 후 공격 보너스 유지 시간")]
    public float perfectEvadeBonusDuration = 3.0f;
    [Tooltip("극한회피 성공 후 공격 보너스 데미지 배율")]
    public float perfectEvadeAttackMultiplier = 1.5f;
    [SerializeField] private PerfectDodgeVignette perfectDodgeVignetteUI;
    [HideInInspector] public bool hasPerfectEvadeAttackBouns;
    private float perfectEvadeBonusEndUnscaledTime = -999f;
    private Coroutine perfectEvadeSlowCoroutine;
    [SerializeField] private PlayerAfterImageEffect afterImageEffect;

    [Header("회피 설정")]
    [SerializeField] private float dashInternalCooldown = 1.5f;

    public float DashInternalCooldown
    {
        get { return dashInternalCooldown; }
    }

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

        if (perfectDodgeVignetteUI == null) perfectDodgeVignetteUI = FindObjectOfType<PerfectDodgeVignette>(true);

        if (afterImageEffect == null) afterImageEffect = GetComponent<PlayerAfterImageEffect>();

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

        UpdatePerfectEvadeBonus();

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

        if (rigidbody.velocity.y < 0f)
        {
            rigidbody.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    public void ChangeState(IPlayerState newstate)
    {
        if (newstate is PlayerHitState || newstate is PlayerIdleState || newstate is PlayerDeadState || newstate is PlayerRollState)
        {
            currentWeapon?.ForceStopTrail();
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
    public bool TryGetUsableSkillInput(out int skillIndex)
    {
        skillIndex = -1;

        if (!IsGrounded) return false;
        if (IsPointerOverUI()) return false;
        if (playerSkills == null) return false;
        if (skillKeys == null) return false;

        int checkCount = Mathf.Min(skillKeys.Length, playerSkills.Length);

        for (int i = 0; i < checkCount; i++)
        {
            if (!Input.GetKeyDown(skillKeys[i])) continue;

            SkillHolder skill = playerSkills[i];

            if (skill == null) continue;

            if (!skill.CanUse()) continue;

            skillIndex = i;
            return true;
        }

        return false;
    }

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
        if (activeCastingSkill == null)
        {
            Debug.LogWarning("[Skill] activeCastingSkill이 null입니다. 스킬 효과 실행 실패");
            return;
        }

        Debug.Log("[Skill] 스킬 효과 실행: " + activeCastingSkill.SkillData.skillName);

        activeCastingSkill.Use(gameObject);
        activeCastingSkill = null;
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

    #region Evade

    private void UpdatePerfectEvadeBonus()
    {
        if (!hasPerfectEvadeAttackBouns) return;

        if (Time.unscaledTime >= perfectEvadeBonusEndUnscaledTime)
        {
            hasPerfectEvadeAttackBouns = false;
        }
    }

    public bool IsRollingState()
    {
        return currentState is PlayerRollState;
    }

    public void TriggerPerfectEvade()
    {
        hasPerfectEvadeAttackBouns = true;
        perfectEvadeBonusEndUnscaledTime = Time.unscaledTime + perfectEvadeBonusDuration;

        PlayerRollState.ResetInternalCooldown();

        if (perfectEvadeSlowCoroutine != null)
        {
            StopCoroutine(perfectEvadeSlowCoroutine);
            RestoreTimeScale();
        }

        if (perfectDodgeVignetteUI != null)
        {
            perfectDodgeVignetteUI.Play();
        }

        if (afterImageEffect != null)
        {
            afterImageEffect.PlayBurst();
        }

        perfectEvadeSlowCoroutine = StartCoroutine(PerfectEvadeSlowCoroutine());
    }

    public float GetPerfectEvadeAttackMultiplier()
    {
        UpdatePerfectEvadeBonus();

        if (!hasPerfectEvadeAttackBouns) return 1f;

        return perfectEvadeAttackMultiplier;
    }

    public void ConsumePerfectEvadeAttackBonus()
    {
        hasPerfectEvadeAttackBouns = false;
    }

    private IEnumerator PerfectEvadeSlowCoroutine()
    {
        Time.timeScale = perfectEvadeTimeScale;
        Time.fixedDeltaTime = 0.02f * perfectEvadeTimeScale;

        yield return new WaitForSecondsRealtime(perfectEvadeSlowDuration);

        RestoreTimeScale();

        perfectEvadeSlowCoroutine = null;
    }

    #endregion

    #region Hit
    public void OnDamageTaken(HitReactionType reactionType = HitReactionType.Normal, Vector3? sourcePosition = null, Vector3? attackDircetion = null)
    {
        if (IsInvincible) return;
        if (IsDead) return;

        if (!ShouldEnterHitState(reactionType)) return;

        EnterHitState(reactionType, sourcePosition, attackDircetion);
    }

    private bool ShouldEnterHitState(HitReactionType reactionType)
    {
        if (currentState is PlayerDeadState)
            return false;

        if (currentState is PlayerHitState)
            return false;

        if (reactionType == HitReactionType.None)
            return false;

        if (Time.time < lastHitReactionTime + hitReactionCooldown)
            return false;

        if (reactionType == HitReactionType.Stagger || reactionType == HitReactionType.Knockback || reactionType == HitReactionType.LaunchKnockDown)
            return true;

        if (reactionType == HitReactionType.Normal)
        {
            if (!(currentState is PlayerIdleState))
                return false;

            return Random.value <= idleHitReactionChance;
        }

        return false;
    }

    private void EnterHitState(HitReactionType reactionType, Vector3? sourcePosition = null, Vector3? attackDirection = null)
    {
        lastHitReactionTime = Time.time;

        currentWeapon?.ForceStopTrail();
        currentWeapon?.DisableHitbox();
        IsAttacking = false;

        if (enemyHitMoveCoroutine != null)
        {
            StopCoroutine(enemyHitMoveCoroutine);
            enemyHitMoveCoroutine = null;
        }

        bool isKnockback = reactionType == HitReactionType.Knockback;
        bool isLaunchKnockDown = reactionType == HitReactionType.LaunchKnockDown;
        bool hasMovement = isKnockback || isLaunchKnockDown;

        hitAnimationEnded = false;
        isHitReactionMotionActive = hasMovement;

        ChangeState(new PlayerHitState(isLaunchKnockDown));

        if (!hasMovement) return;

        Vector3 reactionDirection = CalculateHitReactionDirection(sourcePosition, attackDirection, isLaunchKnockDown);

        float moveDistance = isLaunchKnockDown ? launchKnockDownDistance : enemyKnockbackDistance;
        float moveDuration = isLaunchKnockDown ? launchKnockDownDuration : enemyKnockbackDuration;
        float arcHeight = isLaunchKnockDown ? launchKnockDownHeight : 0f;

        enemyHitMoveCoroutine = StartCoroutine(HitReactionMoveRoutine(reactionDirection, moveDistance, moveDuration, arcHeight));
    }

    private Vector3 CalculateHitReactionDirection(Vector3? sourcePosition, Vector3? attackDirection, bool useDirectionalLaunch)
    {
        Vector3 radialDirection = Vector3.zero;

        if (sourcePosition.HasValue)
        {
            radialDirection = transform.position - sourcePosition.Value;
            radialDirection.y = 0f;
        }

        if (useDirectionalLaunch && attackDirection.HasValue)
        {
            Vector3 forwardDirection = attackDirection.Value;
            forwardDirection.y = 0f;

            if (forwardDirection.sqrMagnitude > 0.001f)
            {
                forwardDirection.Normalize();

                Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

                float sideAmount = 0f;

                if (radialDirection.sqrMagnitude > 0.001f)
                {
                    radialDirection.Normalize();

                    sideAmount = Vector3.Dot(radialDirection, rightDirection);
                }

                Vector3 directionalLaunch = forwardDirection + rightDirection * sideAmount * launchSideInfluence;

                if (directionalLaunch.sqrMagnitude > 0.001f)
                {
                    return directionalLaunch.normalized;
                }
            }
        }

        if (radialDirection.sqrMagnitude > 0.001f)
        {
            return radialDirection.normalized;
        }

        if (attackDirection.HasValue)
        {
            Vector3 fallbackAttackDirection = attackDirection.Value;
            fallbackAttackDirection.y = 0f;

            if (fallbackAttackDirection.sqrMagnitude > 0.001f)
            {
                return fallbackAttackDirection.normalized;
            }
        }

        Vector3 fallbackDirection = -player.transform.forward;
        fallbackDirection.y = 0f;

        return fallbackDirection.normalized;
    }

    private IEnumerator HitReactionMoveRoutine(Vector3 direction, float distance, float duration, float arcHeight)
    {
        CanRotate = false;

        if (move != null)
        {
            move.ForceMove(Vector3.zero, 0f);
        }

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = -player.transform.forward;
            direction.y = 0f;
        }

        direction.Normalize();

        duration = Mathf.Max(duration, 0.01f);

        float safeDistance = GetSafeHitReactionDistance(direction, distance);

        Vector3 startPosition = rigidbody.position;
        Vector3 endPosition = startPosition + direction * safeDistance;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (IsDead) break;

            elapsedTime += Time.fixedDeltaTime;

            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            float easedTime = 1f - Mathf.Pow(1f - normalizedTime, 3f);

            Vector3 nextPosition = Vector3.Lerp(startPosition, endPosition, easedTime);

            if (arcHeight > 0f)
            {
                float verticalOffset = 4f * arcHeight * normalizedTime * (1f - normalizedTime);

                nextPosition.y = Mathf.Lerp(startPosition.y, endPosition.y, normalizedTime) + verticalOffset;
            }
            else
            {
                nextPosition.y = rigidbody.position.y;
            }

            rigidbody.MovePosition(nextPosition);

            yield return new WaitForFixedUpdate();
        }

        rigidbody.velocity = Vector3.zero;

        CanRotate = true;

        isHitReactionMotionActive = false;
        enemyHitMoveCoroutine = null;

        TryFinishHitReaction();
    }

    private float GetSafeHitReactionDistance(Vector3 direction, float requestedDistance)
    {
        if (requestedDistance <= 0f) return 0f;

        if (hitReactionObstacleLayer.value == 0) return requestedDistance;

        Vector3 castOrigin = rigidbody.position + Vector3.up * hitReactionCheckHeight;

        if (Physics.SphereCast(castOrigin, hitReactionCheckRadius, direction, out RaycastHit hit, requestedDistance, hitReactionObstacleLayer, QueryTriggerInteraction.Ignore))
        {
            return Mathf.Max(hit.distance - hitReactionWallBuffer, 0f);
        }

        return requestedDistance;
    }

    public void NotifyHitAnimationEnded()
    {
        hitAnimationEnded = true;

        TryFinishHitReaction();
    }

    private void TryFinishHitReaction()
    {
        if (IsDead) return;

        if (!(currentState is PlayerHitState)) return;

        if (!hitAnimationEnded) return;

        if (isHitReactionMotionActive) return;

        ChangeState(new PlayerIdleState());
    }
    #endregion

    public Vector3 GetSafeAttackDashDelta(Vector3 direction, float distance)
    {
        if (distance <= 0f) return Vector3.zero;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return Vector3.zero;

        direction.Normalize();

        Vector3 castOrigin = rigidbody.position + Vector3.up * attackDashCheckHeight;

        float allowedDistance = distance;

        if (Physics.SphereCast(castOrigin, attackDashCheckRadius, direction, out RaycastHit hit, distance, attackDashObstacleLayer, QueryTriggerInteraction.Ignore))
        {
            allowedDistance = Mathf.Max(hit.distance - attackDashWallBuffer, 0f);
        }

        return direction * allowedDistance;
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            IsGrounded = true;
        }
    }

    public Transform FindNearestEnemyInRange(float range)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, range, enemyLayer);

        Transform bestTarget = null;
        float closestSqrDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            Enemy_Stat enemyStat = col.GetComponentInParent<Enemy_Stat>();
            if (enemyStat == null) continue;
            if (enemyStat.currentHP <= 0) continue;

            Vector3 diff = enemyStat.transform.position - transform.position;
            diff.y = 0f;

            float sqrDistance = diff.sqrMagnitude;

            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                bestTarget = enemyStat.transform;
            }
        }

        return bestTarget;
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

