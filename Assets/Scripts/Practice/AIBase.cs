using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBase
{//상태패턴 fsm
    protected AI AI = AI.AI_CREATE;

    protected Characters Character;

    public void Init(Characters _Character)
    {
        Character = _Character;
    }

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
        //길찾기
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
