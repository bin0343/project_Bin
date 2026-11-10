using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillEffectEntity : MonoBehaviour
{
    private int damageAmount;
    private Vector3 hitboxSize;
    private float damageDelay;
    private LayerMask enemyLayer;
    private HashSet<Enemy_Stat> hitEnemies = new HashSet<Enemy_Stat>();

    public void Setup(int damage, Vector3 size, float delay, LayerMask layer)
    {
        this.damageAmount = damage;
        this.hitboxSize = size;
        this.damageDelay = delay;
        this.enemyLayer = layer;

        StartCoroutine(ExecuteSkillSequence());
    }

    private IEnumerator ExecuteSkillSequence()
    {
        //비주얼적 대기 시간
        yield return new WaitForSeconds(damageDelay);

        //캐릭터와 별개로 스킬이 직접 스캔해서 타격
        Collider[] colliders = Physics.OverlapBox(transform.position, hitboxSize / 2f, transform.rotation, enemyLayer);

        foreach (Collider col in colliders)
        {
            Enemy_Stat enemy = col.GetComponentInParent<Enemy_Stat>();

            if (enemy == null) continue;
            if (enemy.currentHP <= 0) continue;

            // 같은 몬스터를 중복으로 공격하지 않음
            if (!hitEnemies.Add(enemy)) continue;

            int finalDamage = CombatDamageCalculator.CalculateFinalDamage(damageAmount, enemy.defensePower);

            enemy.TakeDamage(finalDamage, AttackType.Normal);
        }

        Destroy(gameObject, 2.5f);  //파티클 잔상 소멸 시간
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.4f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, hitboxSize);
    }
}
