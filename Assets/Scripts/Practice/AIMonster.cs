
using UnityEngine;

public class AIMonster : MonoBehaviour
{
    public Transform[] TRPATH;

    protected AI AI = AI.AI_CREATE;

    public Monster Monster;

    int Index = 0;

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
        float distance = Vector3.Distance(Monster.transform.position, TRPATH[Index].position); //길찾기 (distance안에 오면 다음길 찾고 다음길 찾겠다.)

        if (distance < 1f)
        {
            if(TRPATH.Length > Index)
                Index++;
            else
                Index = 0;
        }

        //적찾기
        //방황하기

        AI = AI.AI_MOVE;
    }

    protected virtual void Move()
    {

        //목표지점 이동
        //도착하면? 공격

        AI = AI.AI_RESET;
    }

    protected virtual void Reset()
    {
        AI = AI.AI_SEARCH;
    }
}
