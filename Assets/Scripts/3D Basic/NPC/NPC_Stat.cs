using UnityEngine;

public class NPC_Stat : MonoBehaviour
{
    [Header("연동할 NPC 데이터")]
    public NPC_Data npcData;

    // [수정] 플레이어처럼 Base와 Equipment 분리
    // NPC_Manager에 저장된 '순수 성장 스탯'을 여기에 담습니다.
    public float[] baseStats = new float[(int)STAT.STAT_COUNT];

    // 장비로 인해 추가된 스탯을 여기에 담습니다. (저장 안 됨, 장착 시 실시간 반영)
    private float[] equipmentStats = new float[(int)STAT.STAT_COUNT];

    // [수정] 플레이어와 동일한 프로퍼티 구조 (Base + Equipment)
    public int maxHP { get { return (int)(GetBaseStat(STAT.HP) + equipmentStats[(int)STAT.HP]); } }
    public int maxMP { get { return (int)(GetBaseStat(STAT.MP) + equipmentStats[(int)STAT.MP]); } }
    public int attackPower { get { return (int)(GetBaseStat(STAT.Attack) + equipmentStats[(int)STAT.Attack]); } }
    public int defensePower { get { return (int)(GetBaseStat(STAT.Defense) + equipmentStats[(int)STAT.Defense]); } }

    public int currentHP;
    public int currentMP;

    // 상태 관리
    public bool isDead = false;

    private void Start()
    {
        InitializeFromManager();
    }

    // 1. 초기화: NPCManager에서 '순수 스탯'만 가져옵니다.
    public void InitializeFromManager()
    {
        if (npcData == null) return;

        // 매니저로부터 저장된 상태 가져오기
        NPCStatus savedStatus = NPC_Manager.instance.GetNPCStatus(npcData.NPCID, npcData);

        // [수정] 저장된 스탯을 'baseStats'에 복사합니다.
        // (savedStatus.currentStats는 매니저 입장에서의 '현재 스탯'이자 '순수 스탯'입니다)
        System.Array.Copy(savedStatus.currentStats, baseStats, savedStatus.currentStats.Length);

        // 장비 스탯 초기화 (새로 생성되었으므로 0부터 시작)
        // 만약 '기본 장비'가 있다면 여기서 장착 로직을 호출하거나, 
        // 별도의 EquipmentManager가 Start에서 AddEquipmentStat을 호출해줄 것입니다.
        System.Array.Clear(equipmentStats, 0, equipmentStats.Length);

        // 체력 초기화
        currentHP = maxHP;
        currentMP = maxMP;

        Debug.Log($"{npcData.NPCName} 배치 완료. Lv.{savedStatus.level} (HP: {currentHP}, ATK: {attackPower})");
    }

    // 스탯 가져오기 헬퍼 (순수 스탯)
    public float GetBaseStat(STAT type)
    {
        return baseStats[(int)type];
    }

    // [추가] 장비 스탯 더하기 (Player_Stat과 동일 로직)
    public void AddEquipmentStat(STAT type, float value)
    {
        // HP 장비 착용 시 현재 체력 비율 유지 로직 (선택 사항)
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

    // [추가] 장비 스탯 빼기
    public void RemoveEquipmentStat(STAT type, float value)
    {
        if (type == STAT.HP)
        {
            float oldMax = maxHP;
            equipmentStats[(int)type] -= value;
            float newMax = maxHP;

            // 장비 해제 시 현재 체력이 최대 체력을 넘지 않도록 조정
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

    // 2. 피해 입기
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        // TODO: 데미지 텍스트 띄우기 (DamageTextSpawner 연동)

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // 3. 경험치 획득 및 성장 반영
    public void GainExp(int amount)
    {
        if (npcData == null) return;

        // 매니저에게 경험치 추가 요청 (레벨업 및 스탯 상승은 매니저 내부에서 처리됨)
        NPC_Manager.instance.AddExperience(npcData.NPCID, amount, npcData);

        // [중요] 매니저 쪽에서 레벨업으로 'baseStats'가 올랐을 수 있으니 다시 동기화
        NPCStatus updatedStatus = NPC_Manager.instance.GetNPCStatus(npcData.NPCID, npcData);
        System.Array.Copy(updatedStatus.currentStats, baseStats, updatedStatus.currentStats.Length);

        // 레벨업 시 체력 회복 등의 처리가 필요하면 여기서 추가
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{npcData.NPCName} 전투 불능!");
        gameObject.SetActive(false);
    }
}