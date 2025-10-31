using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Stat : MonoBehaviour
{
    public float[] baseStats = new float[(int)STAT.STAT_COUNT];
    //private float[] baseStats = new float[(int)STAT.STAT_COUNT];
    private float[] equipmentStats = new float[(int)STAT.STAT_COUNT];

    public void SetStat(STAT type, float value)
    {
        baseStats[(int)type] = value;
    }

    public float GetStat(STAT type)
    {
        return baseStats[(int)type];
    }
    public void AddEquipmentStat(STAT type, float value)
    {
        equipmentStats[(int)type] += value;
    }
    public void RemoveEquipmentStat(STAT type, float value)
    {
        equipmentStats[(int)type] -= value;
    }

    public int maxHP { get { return (int)(GetStat(STAT.HP) + equipmentStats[(int)STAT.HP]); } }
    public int maxMP { get { return (int)(GetStat(STAT.MP) + equipmentStats[(int)STAT.MP]); } }
    public int attackPower { get { return (int)(GetStat(STAT.Attack) + equipmentStats[(int)STAT.Attack]); } }
    public int defensePower { get { return (int)(GetStat(STAT.Defense) + equipmentStats[(int)STAT.Defense]); } }

    public int level = 1;
    public int exp = 0;
    public int levelUpExp = 100;
    public int currentHP = 100;
    public int currentMP = 100;
    public int gold;

    private Canvas myCanvas;

    public Vector3 damageTextOffset = new Vector3(0, 2.5f, 0);

    private Player_Action action;

    void Start()
    {
        myCanvas = GetComponentInChildren<Canvas>(true);
        action = GetComponent<Player_Action>();

        currentHP = maxHP;
        currentMP = maxMP;
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

        SetStat(STAT.HP, GetStat(STAT.HP) + 10);
        SetStat(STAT.MP, GetStat(STAT.MP) + 5);
        currentHP = maxHP;
        currentMP = maxMP;

        //StatPoints += 3;
    }
}
