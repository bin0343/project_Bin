using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuffSkill", menuName = "Skill/Buff")]
public class Skill_Buff : Skill_Base
{
    public int attackIncreaseAmount;
    public GameObject auraPrefab;

    protected override void ApplyEffect(GameObject user, Vector3 targetPosition)
    {
        Character_Stat stat = user.GetComponentInChildren<Character_Stat>();

        if (stat == null)
        {
            Debug.LogError("[Skill_Buff] Character_Stat을 찾지 못했습니다. user: " + user.name);
            return;
        }

        stat.AddEquipmentStat(STAT.Attack, attackIncreaseAmount);

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

        stat.StartCoroutine(RemoveBuffAfterDuration(stat));
    }

    private IEnumerator RemoveBuffAfterDuration(Character_Stat stat)
    {
        yield return new WaitForSeconds(duration);
        stat.RemoveEquipmentStat(STAT.Attack, attackIncreaseAmount);
    }
}
