using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuffSkill", menuName = "Skill/Buff")]
public class Skill_Buff : Skill_Base
{
    [Header("공격력 버프")]
    [SerializeField, Min(0f)]
    private float attackIncreasePercent = 20f;

    [SerializeField]
    private GameObject auraPrefab;

    protected override void ApplyEffect(GameObject user, Vector3 targetPosition)
    {
        if (user == null) return;

        Character_Stat stat = user.GetComponent<Character_Stat>();

        if (stat == null) stat = user.GetComponentInParent<Character_Stat>();

        if (stat == null) stat = user.GetComponentInChildren<Character_Stat>();

        if (stat == null)
        {
            Debug.LogError("[Skill_Buff] Character_Stat을 찾지 못했습니다. user: " + user.name);

            return;
        }

        int currentAttackPower = stat.attackPower;

        int attackIncreaseAmount = Mathf.RoundToInt(currentAttackPower * (attackIncreasePercent / 100f));

        stat.AddEquipmentStat(STAT.Attack, attackIncreaseAmount);

        Debug.Log($"[Attack Buff] 공격력 {currentAttackPower} → {stat.attackPower} " + $"(+{attackIncreasePercent}% / +{attackIncreaseAmount})");

        if (auraPrefab != null)
        {
            GameObject aura = Instantiate(auraPrefab, stat.transform);

            aura.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            aura.transform.localRotation = Quaternion.identity;

            Destroy(aura, duration);
        }
        else
        {
            Debug.LogWarning("[Skill_Buff] auraPrefab이 비어 있습니다.");
        }

        if (BattleManager.instance != null)
        {
            BattleManager.instance.StartCoroutine(RemoveBuffAfterDuration(stat, attackIncreaseAmount));
        }
        else
        {
            Debug.LogWarning("[Skill_Buff] BattleManager가 없어 버프 종료 타이머를 실행할 수 없습니다.");
        }
    }

    private IEnumerator RemoveBuffAfterDuration(Character_Stat stat, int appliedIncreaseAmount)
    {
        yield return new WaitForSeconds(duration);

        if (stat == null) yield break;

        stat.RemoveEquipmentStat(STAT.Attack, appliedIncreaseAmount);

        Debug.Log($"[Attack Buff] 버프 종료 / 공격력: {stat.attackPower}");
    }
}