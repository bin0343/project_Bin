using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuffSkill", menuName = "Skill/Buff")]
public class Skill_Buff : Skill_Base
{
    public int attackIncreaseAmount;
    public GameObject auraPrefab;

    protected override void ApplyEffect(GameObject user)
    {
        Character_Stat stat = user.GetComponent<Character_Stat>();
        stat.AddEquipmentStat(STAT.Attack, attackIncreaseAmount);

        Vector3 offset = new Vector3(0f, 0.5f, -0.7f);
        GameObject aura = Instantiate(auraPrefab, user.transform.position + offset, Quaternion.identity, user.transform);
        Destroy(aura, duration);

        stat.StartCoroutine(RemoveBuffAfterDuration(stat));
    }

    private IEnumerator RemoveBuffAfterDuration(Character_Stat stat)
    {
        yield return new WaitForSeconds(duration);
        stat.RemoveEquipmentStat(STAT.Attack, attackIncreaseAmount);
    }
}
