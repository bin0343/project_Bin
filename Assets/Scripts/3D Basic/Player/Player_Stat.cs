using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Stat : MonoBehaviour
{
   /* STAT[] stat = new float[(int)STAT.STAT_END];   //스탯 관련 - enum을 쓰는 이유
    public void SetStat(STAT _e, float _value)
    {
        stat[(int)_e] = _e;
    }
    public float GetStat(STAT _e)
    {
        return STAT[(int)_e];
    }*/
    

    public int level = 1;
    public int exp = 0;
    public int levelUpExp = 100;

    public int maxHP = 100;
    public int currentHP = 100;

    public int maxMP = 100;
    public int currentMP = 100;

    public int attackPower = 10;
    public int defensePower = 5;

    private Canvas myCanvas;

    public Vector3 damageTextOffset = new Vector3(0, 2.5f, 0);

    private Player_Action action;

    void Start()
    {
        myCanvas = GetComponentInChildren<Canvas>(true);
        action = GetComponent<Player_Action>();
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);
        action?.OnDamageTaken();
        Debug.Log("플레이어가 피해를 입음. 남은 체력: " + currentHP);

        if (DamageTextSpawner.instance != null)
        {
            Quaternion textRotation = Camera.main.transform.rotation;
            Vector3 spawnPosition = transform.position + damageTextOffset;

            DamageTextSpawner.instance.SpawnDamageText(damage, spawnPosition, textRotation, myCanvas);
        }

        UI_Manager.Instance.UpdatePlayerStatus();
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
    }

    public void RecoverMp(int amount)
    {
        currentMP = Mathf.Clamp(currentMP + amount, 0, maxMP);
    }

    public void GainExp(int amount)
    {
        exp += amount;
        while (exp >= levelUpExp)
        {
            exp -= levelUpExp;
            LevelUp();
        }
    }

    /*public void GainGold(int amount)
    {
        Gold += amount;
    }*/

    private void LevelUp()
    {
        level++;
        levelUpExp *= 2;

        maxHP += 10;
        maxMP += 5;
        currentHP = maxHP;
        currentMP = maxMP;

        //StatPoints += 3;
    }
}
