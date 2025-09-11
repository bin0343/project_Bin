using UnityEngine;

public abstract class PlayerBaseState : IPlayerState_Action
{
    public abstract void Enter(Player_Action player);

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
