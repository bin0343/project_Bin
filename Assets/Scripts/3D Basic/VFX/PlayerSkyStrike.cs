using System.Collections.Generic;
using UnityEngine;

public enum SkyStrikeMode
{
    InstantImpact,  // 낙하 과정 없이 바로 범위 공격
    FallingObject   // 상공에서 낙하한 뒤 범위 공격
}

public class PlayerSkyStrike : MonoBehaviour
{
    [Header("착지 이펙트")]
    [SerializeField] private GameObject impactVFXPrefab;

    [Tooltip("자동 삭제 기능이 없는 착지 이펙트의 생존 시간")]
    [SerializeField] private float impactVFXLifeTime = 1.5f;

    [Header("공격 연출 방식")]
    [SerializeField]
    private SkyStrikeMode strikeMode =
    SkyStrikeMode.FallingObject;

    [Tooltip("낙하 모드에서만 표시되는 물방울 등의 오브젝트")]
    [SerializeField] private GameObject fallingVisual;

    [Header("공격 범위 미리보기")]
    [Tooltip("에디터에서 확인할 광역 공격 반경")]
    [SerializeField] private float previewAttackRadius = 3f;

    private Player_Action player;
    private Vector3 impactPoint;

    private float fallSpeed;
    private float attackRadius;
    private LayerMask enemyLayer;

    private bool usePerfectEvadeBonus;
    private float perfectEvadeMultiplier;

    private bool hasImpacted;

    public void Init(Player_Action player, Vector3 impactPoint, float fallSpeed, float attackRadius, LayerMask enemyLayer, bool usePerfectEvadeBonus, float perfectEvadeMultiplier)
    {
        this.player = player;
        this.impactPoint = impactPoint;
        this.fallSpeed = fallSpeed;
        this.attackRadius = attackRadius;
        this.enemyLayer = enemyLayer;
        this.usePerfectEvadeBonus = usePerfectEvadeBonus;
        this.perfectEvadeMultiplier = perfectEvadeMultiplier;

        hasImpacted = false;

        if (fallingVisual != null)
        {
            fallingVisual.SetActive(
                strikeMode == SkyStrikeMode.FallingObject
            );
        }

        if (strikeMode == SkyStrikeMode.InstantImpact)
        {
            transform.position = impactPoint;
            Impact();
        }
    }

    private void Update()
    {
        if (hasImpacted) return;

        if (strikeMode != SkyStrikeMode.FallingObject)
            return;

        transform.position = Vector3.MoveTowards(transform.position, impactPoint, fallSpeed * Time.deltaTime);

        if ((transform.position - impactPoint).sqrMagnitude <= 0.01f)
        {
            Impact();
        }
    }

    private void Impact()
    {
        if (hasImpacted) return;

        hasImpacted = true;
        transform.position = impactPoint;

        PlayImpactVFX();

        Collider[] colliders = Physics.OverlapSphere(impactPoint, attackRadius, enemyLayer);

        HashSet<Enemy_Stat> damagedEnemies = new HashSet<Enemy_Stat>();

        bool hitAnyEnemy = false;

        foreach (Collider col in colliders)
        {
            Enemy_Stat enemy = col.GetComponentInParent<Enemy_Stat>();

            if (enemy == null) continue;
            if (enemy.currentHP <= 0) continue;
            if (!damagedEnemies.Add(enemy)) continue;

            ApplyDamage(enemy, col);

            hitAnyEnemy = true;
        }

        if (hitAnyEnemy)
        {
            if (usePerfectEvadeBonus && player != null)
            {
                player.ConsumePerfectEvadeAttackBonus();
            }

            if (CameraShakeManager.instance != null)
            {
                CameraShakeManager.instance.ShakeKnockbackHit();
            }
        }

        Destroy(gameObject);
    }

    private void ApplyDamage(Enemy_Stat enemy, Collider hitCollider)
    {
        if (player == null || player.stat == null) return;

        int damage = Mathf.Max(player.stat.attackPower - enemy.defensePower, 1);

        if (usePerfectEvadeBonus)
        {
            damage = Mathf.RoundToInt(damage * perfectEvadeMultiplier);

            damage = Mathf.Max(damage, 1);
        }

        // 3타이므로 범위 안의 적들에게 넉백 적용 여부
        enemy.TakeDamage(damage, AttackType.Normal, usePerfectEvadeBonus);

        if (HitVFXManager.instance != null)
        {
            Vector3 hitPoint = hitCollider.ClosestPoint(impactPoint);

            Vector3 hitDirection = enemy.transform.position - impactPoint;

            hitDirection.y = 0f;

            HitVFXManager.instance.PlayEnemyHit(hitPoint, hitDirection, true);
        }
    }

    private void PlayImpactVFX()
    {
        if (impactVFXPrefab == null) return;

        GameObject effect = Instantiate(impactVFXPrefab, impactPoint, Quaternion.identity);

        if (impactVFXLifeTime > 0f)
        {
            Destroy(effect, impactVFXLifeTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        float radius = Application.isPlaying && attackRadius > 0f
         ? attackRadius
         : previewAttackRadius;

        Vector3 center = Application.isPlaying
            ? impactPoint
            : transform.position;

        Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.25f);
        Gizmos.DrawSphere(center, radius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, radius);

    }
}
