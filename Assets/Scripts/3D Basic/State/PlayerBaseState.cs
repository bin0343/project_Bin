using UnityEngine;

public enum PlayerAnimState
{
    Idle,   //0
    Hit,   //1
    Run,    //2
    Attack1,    //3
    Attack2,    //4
    Attack3,    //5
    RunningAttack,  //6
    Jump,   //7
    RunningJump,   //8 
    Sit,    //9
    SitMove,    //10
    Kick,   //11
    Shield, //12
    Dead,    //13
    Roll    //14
}

public abstract class PlayerBaseState : IPlayerState
{
    protected abstract PlayerAnimState GetAnimState();
    public virtual void Enter(Player_Action player)
    {
        player.animator.SetInteger("ActionState", (int)GetAnimState());
    }

    public abstract void Execute(Player_Action player);

    public abstract void Exit(Player_Action player);

    protected virtual void HandleCommonSkillInput(Player_Action player)     //스킬 입력 공통
    {
        if (player.TryGetUsableSkillInput(out int skillIndex))
        {
            player.HandleSkillInput(skillIndex);
        }
    }

    protected virtual void HandleCommonItemInput(Player_Action player)      //아이템 입력 공통
    {
        KeyCode[] itemKeys = { KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V };

        for (int i = 0; i < 4; i++) // 퀵슬롯은 4개로 가정
        {
            if (Input.GetKeyDown(itemKeys[i]))
            {
                player.HandleItemInput(i);
                return;
            }
        }
    }

    protected virtual void HandleMovementInput(Player_Action player, float speedModifier = 1.0f)
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (moveInput.magnitude > 0)
        {
            float speed = player.move.GetAdjustedSpeed(moveInput) * speedModifier;
            player.move.HandleMovement(moveInput, speed);
            player.move.HandleRotation();
        }
    }
}
