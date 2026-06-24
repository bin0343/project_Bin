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

    private bool isHitboxActive = false;
    private HashSet<Collider> hitTargets = new HashSet<Collider>();

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
            PerformAttackCheck();
        }
    }

    public void EnableHitbox()
    {
        isHitboxActive = true;
        hitTargets.Clear();
    }

    public void DisableHitbox()
    {
        isHitboxActive = false;
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
                    hitTargets.Add(collider);

                    int attackPower = (enemyStat != null) ? enemyStat.attackPower : 10;
                    int damage = Mathf.Max(attackPower - targetStat.defensePower, 1);

                    targetStat.TakeDamage(damage);

                    Player_Action playerAction = collider.GetComponentInParent<Player_Action>();
                    if (playerAction != null)
                    {
                        playerAction.OnDamageTaken();
                    }

                    Debug.Log($"아군 피격! (받은 데미지: {damage}, 남은체력: {targetStat.currentHP})");
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
    }
}