using UnityEngine;

public class PlayerAttackState : PlayerBaseState, IStateAnimationEvents
{
    public static int comboStep = 0;

    //상태 진입 직후 애니메이션 전환이 완료되었는지 확인하는 플래그
    private bool isTransitionFinished = false;

    private float previousCurveValue = 0f;  //커브 이전값 저장하고 얼마나 차이나는지 확인
    private float currentDashDistance = 0f;
    private float dashStartNormalizedTime = 0f;

    private bool comboWindowOpend = false;
    private bool comboConsumed = false;

    protected override PlayerAnimState GetAnimState()
    {
        switch (comboStep)
        {
            case 1: return PlayerAnimState.Attack1;
            case 2: return PlayerAnimState.Attack2;
            case 3: return PlayerAnimState.Attack3;
            default: return PlayerAnimState.Idle;
        }
    }

    public override void Enter(Player_Action player)
    {
        Player_Equipment myEquipment = player.GetComponent<Player_Equipment>();
        if (myEquipment != null)
        {
            myEquipment.EnterCombatState();
        }

        Transform targetEnemy = FindEnemyWithinMaxRange(player);

        if (targetEnemy != null)
        {
            Vector3 targetDir = targetEnemy.position - player.transform.position;
            targetDir.y = 0f;

            if (targetDir.sqrMagnitude > 0.001f)
            {
                player.animator.transform.rotation = Quaternion.LookRotation(targetDir);
            }

            float distanceToEnemy = Vector3.Distance(player.transform.position, targetEnemy.position);

            float desiredDashDist = distanceToEnemy - player.attackDashStopDistance;
            currentDashDistance = Mathf.Clamp(desiredDashDist, 0f, player.attackMoveDistance);
        }
        else
        {
            /*if (Cursor.visible || Cursor.lockState == CursorLockMode.None)
            {
                player.move.LookAtMouse();
            }
            else
            {
                player.move.AlignToCameraForward();
            }*/

            currentDashDistance = 0f;
        }

        player.CanRotate = false;

        isTransitionFinished = false;
        previousCurveValue = 0f;

        comboStep++;
        if (comboStep > 3)
        {
            comboStep = 1;
        }
        player.SetComboStep(comboStep);
        player.canReceiveInput = false;
        base.Enter(player);
    }

    public override void Execute(Player_Action player)
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && player.IsGrounded && !player.IsPointerOverUI())
        {
            if (Account_Manager.instance.TryUseStamina(Account_Manager.instance.rollStaminaCost))
            {
                player.ChangeState(new PlayerRollState());
                return; 
            }
        }

        if (!isTransitionFinished)
        {
            AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
            int expectedAnimHash = Animator.StringToHash(GetAnimState().ToString());

            if (stateInfo.shortNameHash == expectedAnimHash)
            {
                isTransitionFinished = true;

                float normalizedTime = Mathf.Clamp01(stateInfo.normalizedTime);
                dashStartNormalizedTime = normalizedTime;
                previousCurveValue = 0f;
            }
        }

        if (isTransitionFinished)
        {
            AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = Mathf.Clamp01(stateInfo.normalizedTime);
            float rawDashCurveTime = Mathf.Clamp01(normalizedTime - dashStartNormalizedTime);
            float dashCurveTime = Mathf.Clamp01(rawDashCurveTime * player.attackDashCurveSpeed);
            if (player.attackMoveCurves != null && player.attackMoveCurves.Length >= comboStep)
            {
                AnimationCurve currentCurve = player.attackMoveCurves[comboStep - 1];

                if (currentCurve != null && currentCurve.keys.Length > 0)
                {
                    float currentCurveValue = currentCurve.Evaluate(dashCurveTime);
                    float delta = currentCurveValue - previousCurveValue;

                    if (delta > 0)
                    {
                        Vector3 dashDirection = player.animator.transform.forward;
                        dashDirection.y = 0;
                        dashDirection.Normalize();

                        Vector3 moveDelta = dashDirection * delta * currentDashDistance;
                        player.rigidbody.MovePosition(player.rigidbody.position + moveDelta);
                    }

                    previousCurveValue = currentCurveValue;
                }
            }
        }

        if (Input.GetMouseButtonDown(0) && !player.IsPointerOverUI())
        {
            player.comboQueued = true;
            player.lastComboInputTime = Time.time;

            if (TryConsumeCombo(player))
            {
                return;
            }
        }

        if (player.comboQueued && Time.time - player.lastComboInputTime > player.comboInputBufferTime)
        {
            player.comboQueued = false;
        }
    }

    public override void Exit(Player_Action player)
    {
        player.CanRotate = true;
        player.animEvents?.EndAttackTrail();
        player.currentWeapon?.ForceStopTrail();
        player.canReceiveInput = false;
        player.IsAttacking = false;

        player.rigidbody.velocity = new Vector3(0, player.rigidbody.velocity.y, 0);
    }

    public static void ResetCombo()
    {
        comboStep = 0;
    }

    public void OnAnimationEvent(Player_Action.AnimationEventType eventType, Player_Action player)
    {
        if (eventType == Player_Action.AnimationEventType.COMBO_WINDOW_OPEN)
        {
            comboWindowOpend = true;
            player.canReceiveInput = true;

            TryConsumeCombo(player);
            return;
        }

        if (!isTransitionFinished)
        {
            return;
        }

        if (eventType == Player_Action.AnimationEventType.ATTACK_ANIMATION_END)
        {
            if (TryConsumeCombo(player))
            {
                return;
            }

            player.comboQueued = false;
            ResetCombo();

            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                player.ChangeState(new PlayerMoveState());
            }
            else
            {
                player.ChangeState(new PlayerIdleState());
            }
        }
    }


    private Transform FindEnemyWithinMaxRange(Player_Action player)
    {
        Collider[] colliders = Physics.OverlapSphere(player.transform.position, player.attackMoveDistance, player.enemyLayer);

        Transform bestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            if (!col.CompareTag("Enemy")) continue;

            Enemy_Stat enemyStat = col.GetComponent<Enemy_Stat>();
            if (enemyStat == null || enemyStat.currentHP <= 0) continue;

            float distance = Vector3.Distance(player.transform.position, col.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestTarget = col.transform;
            }
        }

        return bestTarget;
    }

    private bool TryConsumeCombo(Player_Action player)
    {
        if (comboConsumed) return false;
        if (!comboWindowOpend) return false;
        if (!player.comboQueued) return false;

        comboConsumed = true;
        player.comboQueued = false;
        player.canReceiveInput = false;

        player.ChangeState(new PlayerAttackState());
        return true;
    }
}
