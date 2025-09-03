using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAttackSkill", menuName = "Skill/Attack")]
public class Skill_Attack : Skill_Base
{
    public int attackIncreaseAmount;
    public GameObject auraPrefab;

    protected override void ApplyEffect(GameObject user)
    {
        Player_Stat stat = user.GetComponent<Player_Stat>();
        stat.AttackPower += attackIncreaseAmount;

        Vector3 offset = new Vector3(0f, 0.5f, -0.7f);
        GameObject aura = Instantiate(auraPrefab, user.transform.position + offset, Quaternion.identity, user.transform);
        Destroy(aura, duration);

        stat.StartCoroutine(RemoveBuffAfterDuration(stat));
    }

    private IEnumerator RemoveBuffAfterDuration(Player_Stat stat)
    {
        yield return new WaitForSeconds(duration);
        stat.AttackPower -= attackIncreaseAmount;
    }
}
