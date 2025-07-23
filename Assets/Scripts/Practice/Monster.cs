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
    public int Hp = 100;

    string CurAni = "";

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

    public void OnHit(int _Attack)
    {
        Hp += _Attack;

        if (Hp <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void OnAnimMationStart(string _Ani)
    {
        Debug.Log("OnAnimationStart : " + _Ani);
    }

    public void OnAnimMationIng(string _Ani)
    {
        if (_Ani == "Attack")
        {
            //공격거리면 hit

            //Vector3.Lerp();       선형고간? 보간?
            //Vector3.Slerp();      곡선고간? 보간?

            //AI.Character.Hp -= Atk;
        }
    }

    public void OnAnimMationEnd(string _Ani)
    {
        Debug.Log("OnAnimationEnd : " + _Ani);

        if (CurAni == _Ani)
        {
            //if (CurAni == "Attack")

        }
    }
}
