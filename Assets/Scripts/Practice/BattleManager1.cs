using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager1 : MonoBehaviour
{
    public Transform[] TRPATH;

    public Transform TRPLAYER;
    public Transform TRMONSTER;

    [NonSerialized]
    public Monster Monster;
    [NonSerialized]
    public Character Character;

    Queue<string> Queue = new Queue<string>();  //서버 데이터를 처리할때(먼저 들어온 데이터 먼저 처리) - 동기 처리, 웹 - 비동기 처리(어싱크, 어웨이크)
    //렉걸려도 queue를 쓰면 렉걸린 순간의 입력값이 아직 안나갔기 때문에 스킬 연속기 중 렉걸려도 끊기지 않음/ 예 - 메시지1, 메시지2, 메시지3 입력후 코루틴으로 순차적으로 빼내서 표시

    bool IsCharacterReset = false;

    Action<int> ActionSkill;

    private void Awake()
    {
        //Shared.BattleManager = this;
    }

    private void Start()
    {
        ActionSkill = Shared.UIBattle.OnActionSkill;

        ActionSkill?.Invoke(100);   //ActionSkill? == 대리자에 연결되어있는 펑션이 있는지 확인하고 없으면 실행 X

        InvokeRepeating("CheckCharacterDie", 1f, 1f);

        SpawnCharacter();
        SpawnMonster();
    }

    void Update()
    {
        
    }

    void SpawnMonster()
    {
        UnityEngine.Object obj = Resources.Load("Prefabs/Practice/Monster");  //캐릭터 프리팹을 리소스 - 프리팹 폴더에서 가져오기

        GameObject go = obj as GameObject;      //메모리(공간)로 할당은 됐지만 활성화는 안됐음

        go = Instantiate(go, Vector3.zero, Quaternion.identity);        //메모리를 실제로 할당해줌.

        go.transform.SetParent(TRMONSTER);  //배틀매니저 오브젝트의 위치 ((0,0,0)으로 세팅해두는게 좋음(맵, 오브젝트 등))

        Monster = go.GetComponent<Monster>();

        Monster.transform.rotation = TRMONSTER.rotation;
        Monster.transform.position = TRMONSTER.position;
    }

    void SpawnCharacter()
    {
        
        UnityEngine.Object obj = Resources.Load("Prefabs/Practice/Character");  //캐릭터 프리팹을 리소스 - 프리팹 폴더에서 가져오기

        GameObject go = obj as GameObject;      //메모리(공간)로 할당은 됐지만 활성화는 안됐음

        go = Instantiate(go, Vector3.zero, Quaternion.identity);        //메모리를 실제로 할당해줌.

        go.transform.SetParent(transform);  //배틀매니저 오브젝트의 위치 ((0,0,0)으로 세팅해두는게 좋음(맵, 오브젝트 등))

        Character = go.GetComponent<Character>();

        Character.transform.rotation = transform.rotation;
        Character.transform.position = transform.position;
    }

    void CheckCharacterDie()
    {
        if (Character.Hp <= 0)
        {
            if (IsCharacterReset)
                return;

            IsCharacterReset = true;

            Invoke("CharacterReset", 3f);   //"(안에 있는 이름의 함수를)", 3f초 후에 호출
        }
            
    }

    void CharacterReset()
    {
        IsCharacterReset = false;

        Character.Hp = 100;
        Character.gameObject.SetActive(true);
    }
}
