using UnityEngine;

public class PlayerTornadoState : PlayerBaseState
{
    private readonly SkillHolder skill;

    private SkillSpinAttackRuntime runtime;

    private float remainingTime;
    private float moveSpeed;

    private bool previousCanRotate;

    public PlayerTornadoState(SkillHolder skill)
    {
        this.skill = skill;
    }

    protected override PlayerAnimState GetAnimState()
    {
        return PlayerAnimState.Idle;
    }

    public override void Enter(Player_Action player)
    {
        Skill_SpinAttack spinData = skill.SkillData as Skill_SpinAttack;

        if (spinData == null) return;

        remainingTime = spinData.duration;
        moveSpeed = spinData.TornadoMoveSpeed;

        previousCanRotate = player.CanRotate;
        player.CanRotate = false;

        // 기존 애니메이션 이벤트 실행 대기값 제거
        player.activeCastingSkill = null;

        // 애니메이션을 기다리지 않고 즉시 스킬 실행
        skill.Use(player.gameObject);

        runtime = player.GetComponent<SkillSpinAttackRuntime>();

        Debug.Log("[Tornado] 토네이도 상태 시작");
    }

    public override void Execute(Player_Action player)
    {
        // 스킬 실행부가 없어졌거나 중단되었다면 복귀
        if (runtime == null || !runtime.IsRunning)
        {
            Finish(player);
            return;
        }

        // 기존 Player_Move를 사용한 WASD 이동
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (player.move != null)
        {
            player.move.HandleMovement(moveInput, moveSpeed);
        }

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            Finish(player);
        }
    }

    private void Finish(Player_Action player)
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (input.sqrMagnitude > 0.01f)
        {
            player.ChangeState(new PlayerMoveState());
        }
        else
        {
            player.ChangeState(new PlayerIdleState());
        }
    }

    public override void Exit(Player_Action player)
    {
        // 상태가 끝나면 공격과 이펙트 정리
        runtime?.StopSpin();

        if (player.move != null)
        {
            player.move.ForceMove(Vector3.zero, 0f);
        }

        player.CanRotate = previousCanRotate;

        Debug.Log("[Tornado] 토네이도 상태 종료");
    }
}