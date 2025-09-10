using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Stat : MonoBehaviour
{
    public int Level = 1;
    public int Exp = 0;
    public int LevelUpExp = 100;

    public int MaxHP = 100;
    public int CurrentHP = 100;

    public int MaxMP = 100;
    public int CurrentMP = 100;

    public int AttackPower = 10;
    public int DefensePower = 5;

    private Canvas myCanvas;

    public Vector3 damageTextOffset = new Vector3(0, 2.5f, 0);

    void Start()
    {
        myCanvas = GetComponentInChildren<Canvas>(true);
    }

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        CurrentHP = Mathf.Max(CurrentHP, 0);
        Debug.Log("플레이어가 피해를 입음. 남은 체력: " + CurrentHP);

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
        CurrentHP = Mathf.Clamp(CurrentHP + amount, 0, MaxHP);
    }

    public void RecoverMp(int amount)
    {
        CurrentMP = Mathf.Clamp(CurrentMP + amount, 0, MaxMP);
    }

    public void GainExp(int amount)
    {
        Exp += amount;
        while (Exp >= LevelUpExp)
        {
            Exp -= LevelUpExp;
            LevelUp();
        }
    }

    /*public void GainGold(int amount)
    {
        Gold += amount;
    }*/

    private void LevelUp()
    {
        Level++;
        LevelUpExp *= 2;

        MaxHP += 10;
        MaxMP += 5;
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;

        //StatPoints += 3;
    }
}
