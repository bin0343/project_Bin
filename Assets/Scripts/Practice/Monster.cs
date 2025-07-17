using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour       //선언법
{
    [SerializeField]
    protected Animator ANIMATIOR;
    [SerializeField]
    protected SpriteRenderer SR;

    public AIMonster AI;

    //string CurAni = "";

    void Start()
    {
        //float hp = Def - Characters.Instance.atk;        //사용법
        //Shared.SceneMgr.adsf                      //사용법
    }

    private void FixedUpdate()
    {
        
    }

    public void State()
    {
        switch (AI)
        {
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(Vector3 _Move)
    {
        transform.position = Vector3.MoveTowards(transform.position, _Move, 0.1f);
    }
}
