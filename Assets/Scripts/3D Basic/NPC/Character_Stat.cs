using System;
using UnityEngine;

public class Character_Stat : MonoBehaviour
{
    [Header("연동할 NPC 데이터")]
    public Character_Data characterData;

    public float[] baseStats = new float[(int)STAT.STAT_COUNT];

    private float[] equipmentStats = new float[(int)STAT.STAT_COUNT];

    public int maxHP { get { return (int)(GetBaseStat(STAT.HP) + equipmentStats[(int)STAT.HP]); } }
    public int attackPower { get { return (int)(GetBaseStat(STAT.Attack) + equipmentStats[(int)STAT.Attack]); } }
    public int defensePower { get { return (int)(GetBaseStat(STAT.Defense) + equipmentStats[(int)STAT.Defense]); } }

    public int currentHP;

    public bool isDead = false;


    private void Start()
    {
        if (characterData != null)
        {
            InitializeFromManager();
        }
    }

    public void InitializeFromManager()
    {
        if (characterData == null) return;
        if (Character_Manager.instance == null) return;

        CharacterStatus savedStatus = Character_Manager.instance.GetCharacterStatus(characterData.characterID, characterData);

        if (savedStatus != null)
        {
            if (savedStatus.currentStats != null && savedStatus.currentStats.Length == baseStats.Length)
            {
                System.Array.Copy(savedStatus.currentStats, baseStats, savedStatus.currentStats.Length);
            }

            System.Array.Clear(equipmentStats, 0, equipmentStats.Length);

            currentHP = maxHP;

            Debug.Log($"{characterData.characterName} 배치 완료. Lv.{savedStatus.level} (HP: {currentHP}, ATK: {attackPower})");
        }
    }

    public void SetCharacter(Character_Data data)
    {
        characterData = data;
        InitializeFromManager(); // 매니저에서 레벨, 경험치 불러오기
    }

    public float GetBaseStat(STAT type)
    {
        return baseStats[(int)type];
    }

    public void AddEquipmentStat(STAT type, float value)
    {
        if (type == STAT.HP)
        {
            float oldMax = maxHP;
            equipmentStats[(int)type] += value;
            float newMax = maxHP;

            if (oldMax > 0 && currentHP > 0)
            {
                currentHP = (int)(currentHP * (newMax / oldMax));
            }
            else
            {
                currentHP += (int)value;
            }
        }
        else
        {
            equipmentStats[(int)type] += value;
        }
    }
    public void RemoveEquipmentStat(STAT type, float value)
    {
        if (type == STAT.HP)
        {
            float oldMax = maxHP;
            equipmentStats[(int)type] -= value;
            float newMax = maxHP;

            if (currentHP > newMax)
            {
                currentHP = (int)newMax;
            }
        }
        else
        {
            equipmentStats[(int)type] -= value;
        }
    }

    public void TakeDamage(int damage, Transform attacker = null)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        // TODO: 나중에 UI_Manager.instance.UpdatePlayerStatus(this) 등을 호출하여 화면 아래 메인 체력바 깎기
        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void GainExp(int amount)
    {
        if (characterData == null) return;

        Character_Manager.instance.AddExperience(characterData.characterID, amount, characterData);

        CharacterStatus updatedStatus = Character_Manager.instance.GetCharacterStatus(characterData.characterID, characterData);
        System.Array.Copy(updatedStatus.currentStats, baseStats, updatedStatus.currentStats.Length);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{characterData.characterName} 전투 불능!");
        // TODO: 캐릭터가 죽으면 다른 파티원으로 강제 태그(교체)되는 로직 추가 예정
    }

    public void RefreshStatsFromManager()
    {
        CharacterStatus status = Character_Manager.instance.GetCharacterStatus(characterData.characterID, characterData);

        if (status.currentStats != null)
        {
            System.Array.Copy(status.currentStats, baseStats, status.currentStats.Length);
        }

        currentHP = maxHP;

        Debug.Log($"{characterData.characterName} 스탯 갱신 완료: Lv.{status.level}");
    }
}