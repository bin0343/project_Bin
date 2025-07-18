
using UnityEngine;

public class AIMonster : MonoBehaviour
{
    public Transform[] TRPATH;

    protected AI AI = AI.AI_CREATE;

    public Monster Monster;
    public Character Character;

    bool CharacterMove = false;

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

        if (distance < 3f)
        {
            CharacterMove = true;
        }
        else
            CharacterMove = false;

        /*if (CharacterMove == false) //!CharacterMove !~~ 이게 not, 가장 빠름.
        {
            float distance = Vector3.Distance(Monster.transform.position, TRPATH[Index].position);
        }*/

        if (distance < 1f)
        {
            if (TRPATH.Length > Index)
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
        if (!CharacterMove)
        {
            transform.LookAt(TRPATH[Index].position);

            Monster.Move(TRPATH[Index].position);
        }
        else //도착하면? 공격
        {
            transform.LookAt(Character.transform.position);

            Monster.Move(Character.transform.position);
        }


        AI = AI.AI_SEARCH;
    }

    protected virtual void Reset()
    {
        AI = AI.AI_SEARCH;
    }
}
