using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoSingleton<Monster>       //선언법
{
    public GameObject GOCHARACTER;

    public float Def;
    public float Atk;
    
    // Start is called before the first frame update
    void Start()
    {
        float hp = Def - Character.Instance.atk;        //사용법
        //Shared.SceneMgr.adsf                      //사용법
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if(GOCHARACTER.activeSelf == false)
            {
                GOCHARACTER.SetActive(true);
            }
        }
    }
}
