using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private Player_Action owner;
    private Enemy_Stat target;

    private Vector3 fixedDestination;
    private float speed;
    private AttackType attackType;

    private bool usePerfectEvadeBonus;
    private float perfectEvadeMultiplier;
    private float targetHeightRatio;

    private bool finished = false;

    public void Init(Player_Action owner, Enemy_Stat target, Vector3 destination, float speed, AttackType attackType, bool usePerfectEvadeBonus, float perfectEvadeMultiplier, float targetHeightRatio)
    {
        this.owner = owner;
        this.target = target;
        this.fixedDestination = destination;
        this.speed = speed;
        this.attackType = attackType;
        this.usePerfectEvadeBonus = usePerfectEvadeBonus;
        this.perfectEvadeMultiplier = perfectEvadeMultiplier;
        this.targetHeightRatio = targetHeightRatio;
    }

    private void Update()
    {
        if (finished) return;  

        Vector3 destination = GetCurrentDestination();
        Vector3 direction = destination - transform.position;

        if (direction.sqrMagnitude <= 0.04f)
        {
            Arrive(destination);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction.normalized);
        }
    }

    private Vector3 GetCurrentDestination()
    {
        if (target != null && target.currentHP > 0)
        {
            return GetEnemyAimPoint(target);
        }

        return fixedDestination;
    }

    private void Arrive(Vector3 hitPoint)
    {
        finished = true;

        if (target != null && target.currentHP > 0)
        {
            ApplyDamage(hitPoint);
        }

        Destroy(gameObject);
    }

    private void ApplyDamage(Vector3 hitPoint)
    {
        if (owner == null) return;
        if (owner.stat == null) return;
        if (target == null) return;

        int damage = Mathf.Max(owner.stat.attackPower - target.defensePower, 1);

        if (usePerfectEvadeBonus)
        {
            damage = Mathf.RoundToInt(damage * perfectEvadeMultiplier);
            damage = Mathf.Max(damage, 1);
        }

        target.TakeDamage(damage, attackType, usePerfectEvadeBonus);

        if (usePerfectEvadeBonus)
        {
            owner.ConsumePerfectEvadeAttackBonus();
        }

        if (CameraShakeManager.instance != null)
        {
            if (attackType == AttackType.Knockback)
            {
                CameraShakeManager.instance.ShakeKnockbackHit();
            }
            else
            {
                CameraShakeManager.instance.ShakeNormalHit();
            }
        }

        if (HitVFXManager.instance != null)
        {
            Vector3 hitDirection = target.transform.position - owner.transform.position;
            hitDirection.y = 0;

            bool isCriticalHit = attackType == AttackType.Knockback;

            HitVFXManager.instance.PlayEnemyHit(hitPoint, hitDirection, isCriticalHit);
        }
    }

    private Vector3 GetEnemyAimPoint(Enemy_Stat enemyStat)
    {
        Collider col = enemyStat.GetComponent<Collider>();

        if (col == null)
        {
            col = enemyStat.GetComponentInChildren<Collider>();
        }

        if (col != null)
        {
            Bounds bounds = col.bounds;

            float y = Mathf.Lerp(bounds.min.y, bounds.max.y, targetHeightRatio);

            return new Vector3(bounds.center.x, y, bounds.center.z);
        }

        return enemyStat.transform.position + Vector3.up * 1.2f;
    }
}
