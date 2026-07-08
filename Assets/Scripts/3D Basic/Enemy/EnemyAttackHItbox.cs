using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackHItbox : MonoBehaviour
{
    [Header("히트 박스 설정")]
    [Tooltip("타격 판정 박스의 크기 (가로, 높이, 깊이)")]
    [SerializeField] private Vector3 hitboxSize = new Vector3(2f, 2f, 2f);

    [Tooltip("몬스터 위치 기준 박스의 오프셋 (보통 앞쪽으로)")]
    [SerializeField] private Vector3 hitboxOffset = new Vector3(0, 1f, 1.5f);

    [Tooltip("공격 대상 레이어 (Player 등)")]
    [SerializeField] private LayerMask targetLayer;

    [Header("극한 회피 판정")]
    [Tooltip("실제 공격 판정보다 넓은 회피 판정")]
    [SerializeField] private Vector3 perfectEvadeExtraSize = new Vector3(1.5f, 0.5f, 1.5f);
    [Tooltip("극한회피 판정 박스를 적 전방으로 더 밀고 싶을 때 사용")]
    [SerializeField] private float perfectEvadeForwardOffset = 0f;
    [Tooltip("극한회피 판정 사용 여부")]
    [SerializeField] private bool usePerfectEvadeCheck = true;

    private bool isHitboxActive = false;
    private HashSet<Collider> hitTargets = new HashSet<Collider>();
    private HashSet<Player_Action> perfectEvadeTargets = new HashSet<Player_Action>();

    private EnemyBase enemyBase;
    private Enemy_Stat enemyStat;

    private void Awake()
    {
        enemyBase = GetComponentInParent<EnemyBase>();
        enemyStat = GetComponentInParent<Enemy_Stat>();
    }

    private void Update()
    {
        if (isHitboxActive)
        {
            PerformPerfectEvadeCheck();
            PerformAttackCheck();
        }
    }

    public void EnableHitbox()
    {
        isHitboxActive = true;
        hitTargets.Clear();
        perfectEvadeTargets.Clear();
    }

    public void DisableHitbox()
    {
        isHitboxActive = false;
        perfectEvadeTargets.Clear();
    }

    private void PerformPerfectEvadeCheck()
    {
        if (!usePerfectEvadeCheck) return;
        if (enemyBase == null) return;

        Transform rootTransform = enemyBase.transform;
        Vector3 center = rootTransform.position + rootTransform.TransformDirection(hitboxOffset);
        center += rootTransform.forward * perfectEvadeForwardOffset;

        Vector3 perfectSize = hitboxSize + perfectEvadeExtraSize;

        Collider[] colliders = Physics.OverlapBox(center, perfectSize / 2f, rootTransform.rotation, targetLayer);

        foreach (Collider collider in colliders)
        {
            Player_Action playerAction = collider.GetComponentInParent<Player_Action>();
            if (playerAction == null) continue;

            if (perfectEvadeTargets.Contains(playerAction)) continue;

            // 적 공격 판정이 켜져 있는 동안 플레이어가 회피 상태라면 극한회피 성공
            if (playerAction.IsRollingState() && playerAction.IsInvincible)
            {
                perfectEvadeTargets.Add(playerAction);
                playerAction.TriggerPerfectEvade();
            }
        }
    }

    private void PerformAttackCheck()
    {
        if (enemyBase == null) return;

        Transform rootTransform = enemyBase.transform;
        Vector3 center = rootTransform.position + rootTransform.TransformDirection(hitboxOffset);

        Collider[] colliders = Physics.OverlapBox(center, hitboxSize / 2f, rootTransform.rotation, targetLayer);

        foreach (Collider collider in colliders)
        {
            if (hitTargets.Contains(collider)) continue;

            if (collider.CompareTag("Player") || collider.CompareTag("Companion"))
            {
                Character_Stat targetStat = collider.GetComponentInParent<Character_Stat>();
                if (targetStat != null)
                {
                    Player_Action playerAction = collider.GetComponentInParent<Player_Action>();

                    if (playerAction != null && playerAction.IsInvincible)
                    {
                        // 회피 무적 중이면 데미지는 주지 않고, 이번 공격 판정에서는 이미 처리한 대상으로 본다.
                        hitTargets.Add(collider);
                        continue;
                    }

                    hitTargets.Add(collider);

                    int attackPower = (enemyStat != null) ? enemyStat.attackPower : 10;
                    float damageMultiplier = 1f;
                    if (enemyBase != null)
                    {
                        damageMultiplier = enemyBase.CurrentDamageMultiplier;
                    }

                    int finalAttackPower = Mathf.RoundToInt(attackPower * damageMultiplier);
                    int damage = Mathf.Max(finalAttackPower - targetStat.defensePower, 1);

                    targetStat.TakeDamage(damage, rootTransform);

                    if (HitVFXManager.instance != null)
                    {
                        Vector3 hitPoint = collider.ClosestPoint(center);

                        float minHitHeight = targetStat.transform.position.y + 0.9f;
                        if (hitPoint.y < minHitHeight)
                        {
                            hitPoint.y = minHitHeight;
                        }

                        Vector3 hitDirection = targetStat.transform.position - rootTransform.position;
                        hitDirection.y = 0f;

                        HitVFXManager.instance.PlayPlayerHit(hitPoint, hitDirection);
                    }

                    if (playerAction != null)
                    {
                        HitReactionType reactionType = HitReactionType.Normal;

                        if (enemyBase != null)
                        {
                            reactionType = enemyBase.CurrentHitReactionType;
                        }

                        playerAction.OnDamageTaken(reactionType, rootTransform.position);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (enemyBase == null) enemyBase = GetComponentInParent<EnemyBase>();
        if (enemyBase == null) return;

        Transform rootTransform = enemyBase.transform;

        Gizmos.color = isHitboxActive ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.2f);
        Vector3 center = rootTransform.position + rootTransform.TransformDirection(hitboxOffset);

        Gizmos.matrix = Matrix4x4.TRS(center, rootTransform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, hitboxSize);

        if (usePerfectEvadeCheck)
        {
            Gizmos.color = new Color(0f, 0.7f, 1f, 0.25f);
            Vector3 perfectCenter = center + rootTransform.forward * perfectEvadeForwardOffset;
            Gizmos.matrix = Matrix4x4.TRS(perfectCenter, rootTransform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, hitboxSize + perfectEvadeExtraSize);
        }
    }
}