
using UnityEngine;

public class AIMonster : MonoBehaviour
{
    protected AI AI = AI.AI_CREATE;

    public Monster Monster;


    public void State()
    {
        switch (AI)
        {
            case AI.AI_CREATE:
                Create();
                break;
            case AI.AI_SEARCH:
                Search();
                break;
            case AI.AI_MOVE:
                Move();
                break;
            case AI.AI_RESET:
                Reset();
                break;
        }
    }

    protected virtual void Create()
    {
        //연출효과(무적상태)

        AI = AI.AI_SEARCH;
    }

    protected virtual void Search()
    {
        /*float distance = Vector3.Distance(Monster.transform.position, Shared.BattleManager.TRPATH[Index].position); //길찾기 (distance안에 오면 다음길 찾고 다음길 찾겠다.)

        if (distance < 3f)
        {
            CharacterMove = true;
        }
        else
            CharacterMove = false;

        *//*if (CharacterMove == false) //!CharacterMove !~~ 이게 not, 가장 빠름.
        {
            float distance = Vector3.Distance(Monster.transform.position, TRPATH[Index].position);
        }*//*

        if (distance < 1f)
        {
            *//*if (Shared.BattleManager.TRPATH.Length > Index)
                Index++;
            else
                Index = 0;*//*
        }

        //적찾기
        //방황하기

        AI = AI.AI_MOVE;*/
    }

    protected virtual void Move()
    {
        /*if (!CharacterMove)
        {
            transform.LookAt(Shared.BattleManager.TRPATH[Index].position);

            Monster.Move(Shared.BattleManager.TRPATH[Index].position);   //목표지점 이동
        }
        else //도착하면? 공격
        {
            transform.LookAt(Shared.BattleManager.Character.transform.position);

            float dis = Vector3.Distance(Monster.transform.position, Shared.BattleManager.Character.transform.position);

            if (dis < 3f)
                Monster.Move(Shared.BattleManager.Character.transform.position);
            else
                Attack();       //공격을한것일뿐. 맞은건 아님(상대가 피할 수 있기 때문)

            Monster.Move(Shared.BattleManager.Character.transform.position);
        }*/


        AI = AI.AI_SEARCH;
    }

    void Attack()
    {
        //공격처리
        //1. 애니메이션 충돌 폴리건을 이용한 직접충돌 방법
        //2. 범위는 거리로 체크, 캐릭터가 이미 타겟이므로 애니메이션
        //3. 애니메이션시 hp감소(이벤트 추가, 캐릭터 가져와서 감소)
        //Monster.SetAnimation("Attack");
    }

    protected virtual void Reset()
    {
        AI = AI.AI_SEARCH;
    }
}
