using UnityEngine;

public enum PlayerAnimState
{
    Idle,   //0
    Walk,   //1
    Run,    //2
    Attack1,    //3
    Attack2,    //4
    Attack3,    //5
    RunningAttack,  //6
    Jump,   //7
    Sit,    //8
    Kick,   //9
    Shield, //10
    Dead    //11
}

public abstract class PlayerBaseState : IPlayerState
{
    protected abstract PlayerAnimState GetAnimState();
    public virtual void Enter(Player_Action player)
    {
        player.Animator.SetInteger("ActionState", (int)GetAnimState());
    }

    public abstract void Execute(Player_Action player);

    public abstract void Exit(Player_Action player);

    protected virtual void HandleCommonSkillInput(Player_Action player)     //스킬 입력 공통
    {
        for (int i = 0; i < player.playerSkills.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.F1 + i) && player.IsGrounded)
            {
                player.HandleSkillInput(i);
                return;
            }
        }
    }

    protected virtual void HandleCommonItemInput(Player_Action player)      //아이템 입력 공통
    {
        for (int i = 0; i < 4; i++) // 퀵슬롯은 4개로 가정
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                // Player_Action의 아이템 처리 함수 호출
                player.HandleItemInput(i);
                return;
            }
        }
    }
}
