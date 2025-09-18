using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAttackSkill", menuName = "Skill/Attack")]
public class Skill_Attack : Skill_Base
{
    [Header("Attack Skill Properties")]
    public int damageAmount = 30; // 스킬의 기본 피해량
    public float attackRadius = 5f; // 공격 범위 반경
    public float damageDelay = 0.5f; //스킬 딜레이(실제 피해까지)

    public GameObject rangePrefab;
    public GameObject effectPrefab;

    protected override void ApplyEffect(GameObject user)
    {
        user.GetComponent<MonoBehaviour>().StartCoroutine(AttackCoroutine(user));
    }

    private IEnumerator AttackCoroutine(GameObject user)        //코루틴을 적용하면 안쓰는 애들이 있으니까 상속구조에 위배(나중에 코루틴을 또 쓰는 스킬이 나온다면 수정) 지금은 없는게 나음.
    {
        /*//1. 스킬 범위 표시
        if (rangePrefab != null)
        {
            GameObject indicator = Instantiate(rangePrefab, user.transform.position, user.transform.rotation);
            float diameter = attackRadius * 2f;
            indicator.transform.localScale = new Vector3(diameter, 0.01f, diameter);
            Destroy(indicator, damageDelay + 0.1f);
        }*/

        //2. 실제 피해까지 대기
        yield return new WaitForSeconds(damageDelay);

        //3. 공격 효과 생성
        if (effectPrefab != null)
        {
            yield return new WaitForSeconds(1f);
            GameObject effect = Instantiate(effectPrefab, user.transform.position, user.transform.rotation);
            Destroy(effect, 2f);
        }

        //4. 공격 범위 내의 적에게 피해
        Collider[] enemies = Physics.OverlapSphere(user.transform.position, attackRadius);

        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Enemy_Stat enemyStat = enemy.GetComponent<Enemy_Stat>();
                if (enemyStat != null)
                {
                    enemyStat.TakeDamage(damageAmount, AttackType.None);
                }
            }
        }
    }
}
