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

    [Header("데미지 텍스트")]
    [SerializeField] private bool showDamageText = true;
    [SerializeField] private Canvas damageTextCanvas;
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0f, 0.35f, 0f);
    [SerializeField] private float damageTextCameraForwardOffset = 0.45f;
    [SerializeField] private bool useColliderForDamageTextPosition = true;
    [SerializeField] private Color damageTextColor = Color.white;

    private Collider characterCollider;


    private void Awake()
    {
        CacheDamageTextReferences();

        if (characterData != null)
        {
            InitializeFromManager();
        }
    }

    private void Start()
    {
        CacheDamageTextReferences();
    }

    private void CacheDamageTextReferences()
    {
        if (characterCollider == null)
        {
            characterCollider = GetComponentInChildren<Collider>();
        }

        if (damageTextCanvas == null)
        {
            damageTextCanvas = GetComponentInChildren<Canvas>(true);
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

        ShowDamageText(damage, attacker);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void ShowDamageText(int damage, Transform attacker)
    {
        if (!showDamageText) return;
        if (DamageTextSpawner.instance == null) return;

        CacheDamageTextReferences();

        if (damageTextCanvas == null)
        {
            Debug.LogWarning($"{name}에 데미지 텍스트용 Canvas가 없습니다. Character 하위에 World Space Canvas를 하나 만들어주세요.");
            return;
        }

        Quaternion textRotation = Camera.main != null ? Camera.main.transform.rotation : Quaternion.identity;
        Vector3 spawnPosition = GetDamageTextSpawnPosition(attacker);

        DamageTextSpawner.instance.SpawnDamageText(
            damage,
            spawnPosition,
            textRotation,
            damageTextCanvas,
            false,
            damageTextColor
        );
    }

    private Vector3 GetDamageTextSpawnPosition(Transform attacker)
    {
        Vector3 basePosition = transform.position + damageTextOffset;

        if (useColliderForDamageTextPosition && characterCollider != null)
        {
            Bounds bounds = characterCollider.bounds;
            basePosition = bounds.center;
            basePosition.y = bounds.max.y + damageTextOffset.y;
        }

        Vector3 pushDirection = Vector3.zero;

        if (Camera.main != null)
        {
            pushDirection = Camera.main.transform.position - transform.position;
            pushDirection.y = 0f;
        }

        if (pushDirection.sqrMagnitude < 0.001f && attacker != null)
        {
            pushDirection = transform.position - attacker.position;
            pushDirection.y = 0f;
        }

        if (pushDirection.sqrMagnitude < 0.001f)
        {
            pushDirection = transform.forward;
        }

        pushDirection.Normalize();

        return basePosition + pushDirection * damageTextCameraForwardOffset;
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