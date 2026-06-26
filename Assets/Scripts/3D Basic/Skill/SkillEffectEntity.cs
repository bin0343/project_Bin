using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffectEntity : MonoBehaviour
{
    private int damageAmount;
    private Vector3 hitboxSize;
    private float damageDelay;
    private LayerMask enemyLayer;
    private HashSet<Collider> hitEnemies = new HashSet<Collider>();

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
            if (hitEnemies.Contains(col)) continue;

            if (col.CompareTag("Enemy"))
            {
                hitEnemies.Add(col);
                col.GetComponent<Enemy_Stat>()?.TakeDamage(damageAmount, AttackType.Normal);
            }
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
