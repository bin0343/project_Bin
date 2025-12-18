using System;
using UnityEngine;

public class NPC_Stat : MonoBehaviour
{
    [Header("연동할 NPC 데이터")]
    public NPC_Data npcData;

    public float[] baseStats = new float[(int)STAT.STAT_COUNT];

    private float[] equipmentStats = new float[(int)STAT.STAT_COUNT];

    public int maxHP { get { return (int)(GetBaseStat(STAT.HP) + equipmentStats[(int)STAT.HP]); } }
    public int maxMP { get { return (int)(GetBaseStat(STAT.MP) + equipmentStats[(int)STAT.MP]); } }
    public int attackPower { get { return (int)(GetBaseStat(STAT.Attack) + equipmentStats[(int)STAT.Attack]); } }
    public int defensePower { get { return (int)(GetBaseStat(STAT.Defense) + equipmentStats[(int)STAT.Defense]); } }

    public int currentHP;
    public int currentMP;
    private CompanionHpBar hpBar;

    // 상태 관리
    public bool isDead = false;

    private NPCBase npcBase;

    private void Start()
    {
        hpBar = GetComponentInChildren<CompanionHpBar>();
        if (hpBar != null)
        {
            hpBar.Setup(this);
        }
        npcBase = GetComponent<NPCBase>();
        if (npcData != null)
        {
            InitializeFromManager();
        }
    }

    public void InitializeFromManager()
    {
        if (npcData == null) return;

        NPCStatus savedStatus = NPC_Manager.instance.GetNPCStatus(npcData.NPCID, npcData);

        System.Array.Copy(savedStatus.currentStats, baseStats, savedStatus.currentStats.Length);

        System.Array.Clear(equipmentStats, 0, equipmentStats.Length);

        currentHP = maxHP;
        currentMP = maxMP;

        Debug.Log($"{npcData.NPCName} 배치 완료. Lv.{savedStatus.level} (HP: {currentHP}, ATK: {attackPower})");

        if (hpBar != null)
        {
            hpBar.Setup(this);
        }
    }

    // [추가] 외부(BattleManager)에서 데이터를 주입하고 초기화하는 함수
    public void SetCharacter(NPC_Data data)
    {
        npcData = data;

        // 체력바 등 컴포넌트가 아직 연결 안 됐을 수 있으니 안전하게 호출
        if (hpBar == null) hpBar = GetComponentInChildren<CompanionHpBar>();
        if (npcBase == null) npcBase = GetComponent<NPCBase>();

        InitializeFromManager(); // 매니저에서 레벨, 경험치 불러오기

        // 체력바에도 다시 연결 (확실하게 하기 위해)
        if (hpBar != null) hpBar.Setup(this);
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

        if (hpBar != null)
        {
            hpBar.UpdateHpBar();
        }

        if (attacker != null && currentHP > 0)
        {
            GetComponent<NPCBase>()?.OnDamageTaken(attacker);
        }

        if (currentHP <= 0)
        {
            isDead = true;
            npcBase.OnDeath();
        }
    }

    public void GainExp(int amount)
    {
        if (npcData == null) return;

        NPC_Manager.instance.AddExperience(npcData.NPCID, amount, npcData);

        NPCStatus updatedStatus = NPC_Manager.instance.GetNPCStatus(npcData.NPCID, npcData);
        System.Array.Copy(updatedStatus.currentStats, baseStats, updatedStatus.currentStats.Length);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{npcData.NPCName} 전투 불능!");
        gameObject.SetActive(false);
    }
}