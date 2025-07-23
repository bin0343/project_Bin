using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public Monster Monster;
    public Character Character;

    bool IsCharacterReset = false;

    private void Awake()
    {
        Shared.BattleManager = this;
    }

    private void Start()
    {
        InvokeRepeating("CheckCharacterDie", 1f, 1f);
    }

    void Update()
    {
        
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
