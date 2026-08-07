using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRollState : PlayerBaseState
{
    private float rollTimer;

    [Header("하이브리드 대시 기획 수치 조절")]
    private float maxRollSpeed = 12f;       // 내가 원하는 대시 순간 속도
    private float dashLoopDuration;

    private Vector3 dashDirection;
    private bool isAttackBuffered = false;

    public static int consecutiveDashCount = 0; // 현재 연속 대시 횟수 추적
    public static float cooldownEndTime = 0f;    // 쿨타임이 종료되는 절대 시간 타임스탬프
    public static float lastDashEndTime = 0f;    // 직전 대시가 완전히 끝난 절대 시간

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticDashVariables()
    {
        consecutiveDashCount = 0;
        cooldownEndTime = 0f;
        lastDashEndTime = 0f;
    }

    public static void ResetInternalCooldown()
    {
        consecutiveDashCount = 0;
        cooldownEndTime = 0f;
        lastDashEndTime = -999f;
    }

    private bool isDashBuffered = false;         // 현재 대시 중 Shift 선입력 버퍼

    public static bool CanDash => Time.unscaledTime >= cooldownEndTime;

    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Roll;

    public override void Enter(Player_Action player)
    {
        player.currentWeapon?.ForceStopTrail();
        player.IsAttacking = false;

        if (!CanDash)
        {
            player.ChangeState(new PlayerIdleState());
            return;
        }

        if (Time.unscaledTime - lastDashEndTime > 0.5f)
        {
            consecutiveDashCount = 0;
        }

        consecutiveDashCount++;

        PlayerAttackState.ResetCombo();
        player.IsInvincible = true;
        rollTimer = 0f;
        isAttackBuffered = false;
        isDashBuffered = false; // 선입력 버퍼 리셋

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (moveInput.magnitude > 0.01f)
        {
            Transform camTransform = Camera.main.transform;
            Vector3 lookForward = new Vector3(camTransform.forward.x, 0f, camTransform.forward.z).normalized;
            Vector3 lookRight = new Vector3(camTransform.right.x, 0f, camTransform.right.z).normalized;
            dashDirection = (lookForward * moveInput.y + lookRight * moveInput.x).normalized;
            player.move.CharacterBody.rotation = Quaternion.LookRotation(dashDirection);
        }
        else
        {
            dashDirection = player.move.CharacterBody.forward;
        }

        maxRollSpeed = 12f;
        dashLoopDuration = 0.35f;

        base.Enter(player);

        player.animator.CrossFadeInFixedTime("Avoid_Dash", 0.1f);
    }

    public override void Execute(Player_Action player)
    {
        rollTimer += Time.deltaTime;
        player.currentWeapon?.ForceStopTrail();

        if (Input.GetMouseButtonDown(0) && !player.IsPointerOverUI())
        {
            isAttackBuffered = true;
        }

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1)) && rollTimer > 0.05f)
        {
            isDashBuffered = true;
        }

        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        if (rollTimer > 0.1f && (stateInfo.IsName("Avoid_Dash") || stateInfo.IsName("Avoid_Dashing")))
        {
            if (player.animator.GetInteger("ActionState") == 14)
            {
                player.animator.SetInteger("ActionState", -1);
            }
        }

        if (rollTimer < dashLoopDuration)
        {
            player.move.ForceMove(dashDirection, maxRollSpeed);
        }
        else
        {
            player.rigidbody.velocity = new Vector3(0, player.rigidbody.velocity.y, 0);
            lastDashEndTime = Time.unscaledTime;

            if (consecutiveDashCount >= 2)
            {
                cooldownEndTime = Time.unscaledTime + player.DashInternalCooldown;
                consecutiveDashCount = 0;
                isDashBuffered = false; 
            }

            if (isDashBuffered && CanDash)
            {
                isDashBuffered = false;
                if (Account_Manager.Instance.TryUseStamina(Account_Manager.Instance.rollStaminaCost))
                {
                    player.ChangeState(new PlayerRollState());
                    return;
                }
            }

            if (isAttackBuffered)
            {
                player.ChangeState(new PlayerAttackState());
                return;
            }

            Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (moveInput.magnitude > 0.01f)
                player.ChangeState(new PlayerMoveState());
            else
                player.ChangeState(new PlayerIdleState());
        }
    }

    public override void Exit(Player_Action player)
    {
        player.IsInvincible = false;
    }
}