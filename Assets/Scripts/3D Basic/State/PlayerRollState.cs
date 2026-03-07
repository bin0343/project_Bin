using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRollState : PlayerBaseState
{
    private float rollTimer;
    private float rollDuration = 1.167f;
    private float rollSpeed = 7f;      // 구르기 이동 속도 (수치 조절 필요)
    private Vector3 rollDirection;

    private bool isAttackBuffered = false; //선입력

    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Roll;

    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : Roll");
        base.Enter(player);

        PlayerAttackState.ResetCombo();

        player.IsInvincible = true;
        rollTimer = 0f;

        isAttackBuffered = false;

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (moveInput.magnitude > 0.01f)
        {
            // 움직임 입력이 있다면 카메라가 바라보는 방향을 기준으로 굴러갈 방향 계산
            Transform camTransform = Camera.main.transform;
            Vector3 lookForward = new Vector3(camTransform.forward.x, 0f, camTransform.forward.z).normalized;
            Vector3 lookRight = new Vector3(camTransform.right.x, 0f, camTransform.right.z).normalized;

            rollDirection = (lookForward * moveInput.y + lookRight * moveInput.x).normalized;

            player.move.CharacterBody.rotation = Quaternion.LookRotation(rollDirection);
        }
        else
        {
            // 가만히 서있다가 구르면 캐릭터가 현재 바라보는 정면 방향으로 구릅니다.
            rollDirection = player.move.CharacterBody.forward;
        }
    }

    public override void Execute(Player_Action player)
    {
        rollTimer += Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && !player.IsPointerOverUI())
        {
            isAttackBuffered = true;
        }

        player.rigidbody.velocity = new Vector3(
            rollDirection.x * rollSpeed,
            player.rigidbody.velocity.y, // Y축(중력/낙하)은 자연스럽게 유지
            rollDirection.z * rollSpeed
        );

        if (rollTimer >= rollDuration)
        {
            // 구르기 종료 시 미끄러짐 방지를 위해 x, z 속도를 0으로 잡아줍니다.
            player.rigidbody.velocity = new Vector3(0, player.rigidbody.velocity.y, 0);

            if (isAttackBuffered)
            {
                player.ChangeState(new PlayerAttackState());
                return;
            }

            Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            // 움직임 입력이 있으면 Move, 없으면 Idle로 복귀
            if (moveInput.magnitude > 0.01f)
            {
                player.ChangeState(new PlayerMoveState());
            }
            else
            {
                player.ChangeState(new PlayerIdleState());
            }
        }
    }

    public override void Exit(Player_Action player)
    {
        player.IsInvincible = false;
        Debug.Log("상태 이탈 : Roll");
    }
}
