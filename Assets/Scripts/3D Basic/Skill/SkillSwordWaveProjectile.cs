using System.Collections.Generic;
using UnityEngine;

public class SkillSwordWaveProjectile : MonoBehaviour
{
    private int attackPower;
    private float speed;
    private float maxRange;
    private float traveledDistance;

    private Vector3 hitboxSize;
    private Vector3 moveDirection;

    private LayerMask enemyLayer;
    private LayerMask obstacleLayer;

    private bool initialized;

    private readonly HashSet<Enemy_Stat> hitEnemies =
        new HashSet<Enemy_Stat>();

    public void Initialize(int attack, float moveSpeed, float range, Vector3 size, LayerMask targetLayer, LayerMask wallLayer)
    {
        attackPower = Mathf.Max(0, attack);
        speed = Mathf.Max(0.1f, moveSpeed);
        maxRange = Mathf.Max(0.1f, range);

        hitboxSize = new Vector3(Mathf.Max(0.1f, size.x), Mathf.Max(0.1f, size.y), Mathf.Max(0.1f, size.z));

        enemyLayer = targetLayer;
        obstacleLayer = wallLayer;

        moveDirection = transform.forward.normalized;
        traveledDistance = 0f;

        hitEnemies.Clear();
        initialized = true;

        // 시작 위치부터 적이 겹쳐 있을 수 있으므로 확인
        CheckOverlapAt(transform.position);
    }

    private void Update()
    {
        if (!initialized) return;

        float remainingDistance = maxRange - traveledDistance;

        if (remainingDistance <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // 이번 프레임에서 이동할 거리
        float moveDistance = Mathf.Min(speed * Time.deltaTime, remainingDistance);

        bool hitWall = false;

        // 벽을 통과하지 않도록 이동 거리 제한
        if (Physics.Raycast(transform.position, moveDirection, out RaycastHit wallHit, moveDistance, obstacleLayer, QueryTriggerInteraction.Ignore))
        {
            moveDistance = wallHit.distance;
            hitWall = true;
        }

        // 이동 경로에 있는 모든 적 검색
        if (moveDistance > 0f)
        {
            RaycastHit[] hits = Physics.BoxCastAll(transform.position, hitboxSize * 0.5f, moveDirection, transform.rotation, moveDistance, enemyLayer, QueryTriggerInteraction.Collide);

            foreach (RaycastHit hit in hits)
            {
                TryHitEnemy(hit.collider);
            }

            // 검기 실제 이동
            transform.position += moveDirection * moveDistance;

            traveledDistance += moveDistance;

            // 도착 위치에서 겹치는 적도 검사
            CheckOverlapAt(transform.position);
        }

        // 벽에 닿거나 최대 사거리에 도달하면 제거
        if (hitWall || traveledDistance >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    private void CheckOverlapAt(Vector3 center)
    {
        Collider[] colliders = Physics.OverlapBox(center, hitboxSize * 0.5f, transform.rotation, enemyLayer, QueryTriggerInteraction.Collide);

        foreach (Collider col in colliders)
        {
            TryHitEnemy(col);
        }
    }

    private void TryHitEnemy(Collider col)
    {
        Enemy_Stat enemy = col.GetComponentInParent<Enemy_Stat>();

        if (enemy == null) return;
        if (enemy.currentHP <= 0) return;

        // 같은 검기로 이미 공격한 적은 다시 공격하지 않음
        if (!hitEnemies.Add(enemy)) return;

        // 공통 방어력 계산 방식 사용
        int finalDamage = CombatDamageCalculator.CalculateFinalDamage(attackPower, enemy.defensePower);

        // 기존 Skill 타입은 1.5초 강제 스턴 적용
        enemy.TakeDamage(finalDamage, AttackType.Skill);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Gizmos.DrawWireCube(Vector3.zero, hitboxSize == Vector3.zero ? new Vector3(2.5f, 2f, 0.6f) : hitboxSize);

        Gizmos.matrix = oldMatrix;
    }
}