using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerWeaponAttackMode
{
    Melee,      //근거리
    Ranged      //원거리
}

public class Weapon_Player : MonoBehaviour
{
    [Header("공격 방식")]
    [SerializeField] private PlayerWeaponAttackMode attackMode = PlayerWeaponAttackMode.Melee;

    public bool IsRanged
    {
        get { return attackMode == PlayerWeaponAttackMode.Ranged; }
    }

    [Header("원거리 공격 설정")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileRange = 15f;
    [SerializeField] private float projectileSpeed = 20f;

    [Tooltip("0.5면 몸통 중앙, 0.7이면 가슴~머리 쪽")]
    [SerializeField, Range(0f, 1f)] private float targetHeightRatio = 0.65f;

    public float RangedAttackRange
    {
        get { return projectileRange; }
    }

    [Header("Weapon Components")]
    [SerializeField] private TrailRenderer slashTrail;

    [Header("히트 박스 설정")]
    [Tooltip("타격 판정 박스의 크기 (가로, 높이, 깊이)")]
    [SerializeField] private Vector3 hitboxSize = new Vector3(2f, 2f, 2f);
    [Tooltip("플레이어 위치 기준 판정 박스의 오프셋")]
    [SerializeField] private Vector3 hitboxOffset = new Vector3(0, 1f, 1.5f);
    [Tooltip("적 감지 레이어 마스크")]
    [SerializeField] private LayerMask enemyLayer;

    private bool usePerfectEvadeBonusThisHitbox = false;
    private bool perfectEvadeBonusHitApplied = false;
    private float cachedPerfectEvadeDamageMultiplier = 1f;

    private bool isHitboxActive = false;

    private HashSet<Collider> hitEnemies = new HashSet<Collider>();

    private Player_Action playerAction;

    void Awake()
    {
        if (slashTrail == null) slashTrail = GetComponentInChildren<TrailRenderer>();
        Collider attackCollider = GetComponent<Collider>();
        if (attackCollider != null) attackCollider.enabled = false;
        playerAction = GetComponentInParent<Player_Action>();
        StopTrail();
    }

    private void Update()
    {
        if (isHitboxActive)
        {
            PerformAttackCheck();
        }
    }

    public void EnableHitbox()
    {
        isHitboxActive = true;
        hitEnemies.Clear();

        cachedPerfectEvadeDamageMultiplier = 1f;
        usePerfectEvadeBonusThisHitbox = false;
        perfectEvadeBonusHitApplied = false;

        if (playerAction != null)
        {
            cachedPerfectEvadeDamageMultiplier = playerAction.GetPerfectEvadeAttackMultiplier();
            usePerfectEvadeBonusThisHitbox = cachedPerfectEvadeDamageMultiplier > 1f;
        }
    }

    public void DisableHitbox()
    {
        isHitboxActive = false;

        if (perfectEvadeBonusHitApplied && playerAction != null)
        {
            playerAction.ConsumePerfectEvadeAttackBonus();
        }

        usePerfectEvadeBonusThisHitbox = false;
        perfectEvadeBonusHitApplied = false;
        cachedPerfectEvadeDamageMultiplier = 1f;
    }

    private void PerformAttackCheck()
    {
        if (playerAction == null || playerAction.animator == null) return;

        Transform modelTransform = playerAction.animator.transform;

        // 플레이어의 현재 위치와 바라보는 방향을 기준으로 타격 박스의 중심점 계산
        Vector3 center = modelTransform.transform.position + modelTransform.transform.TransformDirection(hitboxOffset);

        Collider[] colliders = Physics.OverlapBox(center, hitboxSize / 2f, modelTransform.transform.rotation, enemyLayer);

        foreach (Collider collider in colliders)
        {
            if (hitEnemies.Contains(collider)) continue;

            if (collider.TryGetComponent<Enemy_Stat>(out Enemy_Stat enemyStat))
            {
                hitEnemies.Add(collider);

                Character_Stat playerStat = playerAction.stat;
                if (playerStat != null)
                {
                    int damage = Mathf.Max(playerStat.attackPower - enemyStat.defensePower, 1);

                    bool isPerfectEvadeBouns = usePerfectEvadeBonusThisHitbox;

                    if (isPerfectEvadeBouns)
                    {
                        damage = Mathf.RoundToInt(damage * cachedPerfectEvadeDamageMultiplier);
                        damage = Mathf.Max(damage, 1);
                        perfectEvadeBonusHitApplied = true;
                    }

                    AttackType currentAttackType = (playerAction.currentComboStep == 3)
                        ? AttackType.Knockback
                        : AttackType.Normal;

                    enemyStat.TakeDamage(damage, currentAttackType, isPerfectEvadeBouns);

                    if (CameraShakeManager.instance != null)
                    {
                        if (currentAttackType == AttackType.Knockback)
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
                        Vector3 hitPoint = collider.ClosestPoint(center);

                        Vector3 hitDirection = enemyStat.transform.position - modelTransform.position;
                        hitDirection.y = 0f;

                        bool isCriticalHit = currentAttackType == AttackType.Knockback;

                        HitVFXManager.instance.PlayEnemyHit(hitPoint, hitDirection, isCriticalHit);
                    }
                }
            }
        }
    }

    public void StartTrail()
    {
        if (slashTrail != null) slashTrail.emitting = true;
    }

    public void StopTrail()
    {
        if (slashTrail != null) slashTrail.emitting = false;
    }

    public void ForceStopTrail() //피격시
    {
        if (slashTrail != null)
        {
            slashTrail.emitting = false;
            slashTrail.Clear();
        }
    }

    public void ExecuteAttackFrame()
    {
        if (IsRanged)
        {
            FireProjectile();
        }
        else
        {
            EnableHitbox();
        }
    }

    public void EndAttackFrame()
    {
        if (!IsRanged)
        {
            DisableHitbox();
        }
    }

    #region 투사체 발사
    private void FireProjectile()
    {
        if (playerAction == null) return;
        if (projectilePrefab == null) return;

        Transform modelTransform = playerAction.animator.transform;
        Transform shootPoint = firePoint != null ? firePoint : transform;

        Transform targetTransform = playerAction.FindNearestEnemyInRange(projectileRange);
        Enemy_Stat targetStat = null;

        if (targetTransform != null)
        {
            targetStat = targetTransform.GetComponentInParent<Enemy_Stat>();
        }

        Vector3 startPosition = shootPoint.position;
        Vector3 destination;

        if (targetStat != null)
        {
            destination = GetEnemyAimPoint(targetStat);
        }
        else
        {
            Vector3 forward = modelTransform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.001f)
            {
                forward = transform.forward;
            }

            forward.Normalize();
            destination = startPosition + forward * projectileRange;
        }

        Vector3 shootDirection = destination - startPosition;

        if (shootDirection.sqrMagnitude < 0.001f)
        {
            shootDirection = modelTransform.forward;
        }

        GameObject projectileObject = Instantiate(
            projectilePrefab,
            startPosition,
            Quaternion.LookRotation(shootDirection.normalized)
        );

        PlayerProjectile projectile = projectileObject.GetComponent<PlayerProjectile>();

        if (projectile != null)
        {
            AttackType currentAttackType = playerAction.currentComboStep == 3
                ? AttackType.Knockback
                : AttackType.Normal;

            float perfectEvadeMultiplier = playerAction.GetPerfectEvadeAttackMultiplier();
            bool usePerfectEvadeBonus = perfectEvadeMultiplier > 1f;

            projectile.Init(
                playerAction,
                targetStat,
                destination,
                projectileSpeed,
                currentAttackType,
                usePerfectEvadeBonus,
                perfectEvadeMultiplier,
                targetHeightRatio
            );
        }
    }
    #endregion

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

            return new Vector3(
                bounds.center.x,
                y,
                bounds.center.z
            );
        }

        return enemyStat.transform.position + Vector3.up * 1.2f;
    }

    private void OnDrawGizmos()
    {
        if (playerAction != null) playerAction = GetComponentInParent<Player_Action>();
        if (playerAction == null) return;

        Transform modelTransform = playerAction.animator.transform;
        // 공격 판정이 켜져있을 땐 붉은색, 꺼져있을 땐 녹색으로 표시
        Gizmos.color = isHitboxActive ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.2f);

        Vector3 center = modelTransform.transform.position + modelTransform.transform.TransformDirection(hitboxOffset);
        Gizmos.matrix = Matrix4x4.TRS(center, modelTransform.transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, hitboxSize);
    }
}
