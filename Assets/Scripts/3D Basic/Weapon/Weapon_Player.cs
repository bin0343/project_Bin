using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Player : MonoBehaviour
{
    [Header("Weapon Components")]
    [SerializeField] private TrailRenderer slashTrail;

    [Header("히트 박스 설정")]
    [Tooltip("타격 판정 박스의 크기 (가로, 높이, 깊이)")]
    [SerializeField] private Vector3 hitboxSize = new Vector3(2f, 2f, 2f);

    [Tooltip("플레이어 위치 기준 판정 박스의 오프셋")]
    [SerializeField] private Vector3 hitboxOffset = new Vector3(0, 1f, 1.5f);

    [Tooltip("적 감지 레이어 마스크")]
    [SerializeField] private LayerMask enemyLayer;

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
    }

    public void DisableHitbox()
    {
        isHitboxActive = false;
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

                    AttackType currentAttackType = (playerAction.currentComboStep == 3)
                        ? AttackType.Knockback
                        : AttackType.Normal;

                    enemyStat.TakeDamage(damage, currentAttackType);

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
