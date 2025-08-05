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
        Player_Stat stat = user.GetComponent<Player_Stat>();
        stat.AttackPower += attackIncreaseAmount;

        GameObject aura = Instantiate(auraPrefab, user.transform.position, Quaternion.identity, user.transform);
        Destroy(aura, duration);

        stat.StartCoroutine(RemoveBuffAfterDuration(stat));
    }

    private IEnumerator RemoveBuffAfterDuration(Player_Stat stat)
    {
        yield return new WaitForSeconds(duration);
        stat.AttackPower -= attackIncreaseAmount;
    }
}
