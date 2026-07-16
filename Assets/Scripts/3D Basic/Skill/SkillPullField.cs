using System;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.AI;

public class SkillPullField : MonoBehaviour
{
    private Player_Action player;

    private int damageAmount;
    private float duration;
    private float pullRadius;
    private float pullSpeed;
    private float centerStopDistance;
    private float damageInterval;

    private LayerMask enemyLayer;

    private PullDamageMode damageMode;
    private bool singleHitExecuted;
    private bool initialized;
    private bool finished;

    private float elapsedTime;

    private readonly Dictionary<Enemy_Stat, float> nextDamageTimes = new Dictionary<Enemy_Stat, float>();

    private readonly HashSet<Enemy_Stat> processedThisFrame = new HashSet<Enemy_Stat>();    //여러번 끌리기 방지

    private readonly Dictionary<NavMeshAgent, bool> originalStoppedStates = new Dictionary<NavMeshAgent, bool>();

    public void Setup(Player_Action player, int damageAmount, float duration, float pullRadius, float pullSpeed, float centerStopDistance, LayerMask enemyLayer, PullDamageMode damageMode, float damageInterval)
    {
        this.player = player;
        this.damageAmount = damageAmount;
        this.duration = duration;
        this.pullRadius = pullRadius;
        this.pullSpeed = pullSpeed;
        this.centerStopDistance = centerStopDistance;
        this.enemyLayer = enemyLayer;
        this.damageMode = damageMode;
        this.damageInterval = Mathf.Max(damageInterval, 0.05f);

        elapsedTime = 0f;
        initialized = true;
        finished = false;
        singleHitExecuted = false;

        if (damageMode == PullDamageMode.SingleHit)
        {
            ExecuteSingleHitDamage();
        }
    }

    private void Update()
    {
        if (!initialized) return;
        if (finished) return;

        elapsedTime += Time.deltaTime;

        ProcessEnemies();

        if (elapsedTime >= duration)
        {
            FinishField();
        }
    }

    private void ProcessEnemies()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pullRadius, enemyLayer);

        processedThisFrame.Clear();

        foreach (Collider col in colliders)
        {
            Enemy_Stat enemy = col.GetComponentInParent<Enemy_Stat>();

            if (enemy == null) continue;
            if (enemy.currentHP <= 0) continue;

            if (!processedThisFrame.Add(enemy)) continue;

            PullEnemy(enemy);

            if (damageMode == PullDamageMode.Continuous)
            {
                TryApplyContinuousDamage(enemy);
            }
        }
    }

    private void PullEnemy(Enemy_Stat enemy)
    {
        Vector3 toCenter = transform.position - enemy.transform.position;
        toCenter.y = 0f;

        float stopDistanceSqr = centerStopDistance * centerStopDistance;

        if (toCenter.sqrMagnitude <= stopDistanceSqr) return;

        float distance = toCenter.magnitude;

        float moveDistatnce = MathF.Min(pullSpeed * Time.deltaTime, distance - centerStopDistance);

        Vector3 moveDelta = toCenter.normalized * moveDistatnce;

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            if (!originalStoppedStates.ContainsKey(agent))
            {
                originalStoppedStates.Add(agent, agent.isStopped);
            }

            agent.isStopped = true;
            agent.Move(moveDelta);
        }
        else
        {
            enemy.transform.position += moveDelta;
        }
    }

    private void TryApplyContinuousDamage(Enemy_Stat enemy)
    {
        if (nextDamageTimes.TryGetValue(enemy, out float nextDamageTime))
        {
            if (elapsedTime < nextDamageTime) return;
        }

        ApplyDamage(enemy);

        nextDamageTimes[enemy] = elapsedTime + damageInterval;
    }

    private void ExecuteSingleHitDamage()
    {
        if (singleHitExecuted) return;

        singleHitExecuted = true;

        Collider[] colliders = Physics.OverlapSphere(transform.position, pullRadius, enemyLayer);

        processedThisFrame.Clear();

        foreach (Collider col in colliders)
        {
            Enemy_Stat enemy = col.GetComponentInParent<Enemy_Stat>();

            if (enemy == null) continue;
            if (enemy.currentHP <= 0) continue;

            if (!processedThisFrame.Add(enemy)) continue;

            ApplyDamage(enemy);
        }
    }

    private void ApplyDamage(Enemy_Stat enemy)
    {
        int finalDamage = Mathf.Max(damageAmount - enemy.defensePower, 1);

        enemy.TakeDamage(finalDamage, AttackType.Skill);
    }

    private void FinishField()
    {
        if (finished) return;

        finished = true;

        RestoreNavMeshAgents();

        Destroy(gameObject);
    }

    private void RestoreNavMeshAgents()
    {
        foreach (var pair in originalStoppedStates)
        {
            NavMeshAgent agent = pair.Key;

            if (agent == null) continue;
            if (!agent.enabled) continue;
            if (!agent.isOnNavMesh) continue;

            agent.isStopped = pair.Value;
        }

        originalStoppedStates.Clear();
    }

    private void OnDestroy()
    {
        RestoreNavMeshAgents();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.2f);

        Gizmos.DrawSphere(transform.position, pullRadius);

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}
