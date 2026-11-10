using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSpinAttackRuntime : MonoBehaviour
{
    private Player_Action owner;
    private Transform modelTransform;

    private float attackDuration;
    private int totalHitCount;
    private float attackRadius;
    private int damagePerHit;

    private LayerMask enemyLayer;

    private int requiredHitCount;
    private float defenseReductionRate;
    private float defenseReductionDuration;

    private Coroutine spinCoroutine;
    private GameObject activeEffect;

    public bool IsRunning => spinCoroutine != null;

    // 숨긴 캐릭터 Renderer와 기존 상태 보관
    private Renderer[] characterRenderers;
    private bool[] originalRendererStates;

    // 스킬 전체에서 몬스터별 적중 횟수 기록
    private readonly Dictionary<Enemy_Stat, int> hitCounts =
        new Dictionary<Enemy_Stat, int>();

    // 한 번의 타격에서 중복 피해 방지
    private readonly HashSet<Enemy_Stat> hitThisTick =
        new HashSet<Enemy_Stat>();

    public void Begin(
        Player_Action player,
        float duration,
        int hitCount,
        float radius,
        int damage,
        LayerMask layer,
        int requiredHits,
        float reductionRate,
        float reductionDuration,
        GameObject effectPrefab,
        Vector3 effectOffset)
    {
        // 이전 실행이 남아 있다면 정리
        StopSpin();

        owner = player;

        if (owner == null) return;

        modelTransform = owner.animator != null ? owner.animator.transform : owner.transform;

        attackDuration = Mathf.Max(0.1f, duration);
        totalHitCount = Mathf.Max(1, hitCount);
        attackRadius = Mathf.Max(0.1f, radius);
        damagePerHit = Mathf.Max(0, damage);

        enemyLayer = layer;

        requiredHitCount = Mathf.Max(1, requiredHits);
        defenseReductionRate = Mathf.Clamp01(reductionRate);
        defenseReductionDuration = Mathf.Max(0f, reductionDuration);

        hitCounts.Clear();

        if (effectPrefab == null)
        {
            Debug.LogError("[SpinAttack] 토네이도 VFX 프리팹이 없습니다.");
            return;
        }

        activeEffect = Instantiate(effectPrefab, owner.transform);
        activeEffect.transform.localPosition = effectOffset;
        activeEffect.transform.localRotation = effectPrefab.transform.localRotation;

        // 토네이도가 생성된 다음 캐릭터 외형 숨김
        HideCharacterVisuals();

        // 반복 공격 시작
        spinCoroutine = StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        // 지속시간을 타격 횟수로 나누기
        float hitInterval = attackDuration / totalHitCount;

        // 첫 타격은 살짝 뒤에 발생
        yield return new WaitForSeconds(hitInterval * 0.5f);

        for (int i = 0; i < totalHitCount; i++)
        {
            if (!CanContinueSpin()) break;

            // 범위 공격 1회 실행
            ExecuteHitTick();

            // 마지막 타격 이후에는 추가 간격 대기 불필요
            if (i < totalHitCount - 1)
            {
                yield return new WaitForSeconds(hitInterval);
            }
        }

        // 남은 연출 시간
        yield return new WaitForSeconds(hitInterval * 0.5f);

        spinCoroutine = null;
        StopSpin();
    }

    private bool CanContinueSpin()
    {
        if (owner == null) return false;
        if (owner.IsDead) return false;
        if (!owner.gameObject.activeInHierarchy) return false;

        if (!(owner.currentState is PlayerTornadoState)) return false;

        if (owner.IsControlLocked) return false;

        return true;
    }

    private void ExecuteHitTick()
    {
        if (owner == null) return;

        // 캐릭터 현재 위치를 기준으로 판정
        Vector3 center = owner.transform.position + Vector3.up;

        Collider[] colliders = Physics.OverlapSphere(center, attackRadius, enemyLayer, QueryTriggerInteraction.Collide);

        hitThisTick.Clear();

        foreach (Collider col in colliders)
        {
            Enemy_Stat enemy = col.GetComponentInParent<Enemy_Stat>();

            if (enemy == null) continue;
            if (enemy.currentHP <= 0) continue;

            // 한 타격에서 동일 몬스터 중복 피해 방지
            if (!hitThisTick.Add(enemy)) continue;

            ApplyHit(enemy);
        }
    }

    private void ApplyHit(Enemy_Stat enemy)
    {
        // 몬스터별 기존 적중 횟수 가져오기
        int currentHitCount = 0;

        hitCounts.TryGetValue(enemy, out currentHitCount);

        // 적중 횟수 증가
        currentHitCount++;

        hitCounts[enemy] = currentHitCount;

        // 방어력 계산
        int finalDamage = CombatDamageCalculator.CalculateFinalDamage(damagePerHit, enemy.defensePower);

        // 회전 공격은 반복 스턴을 발생시키지 않도록 None 사용
        enemy.TakeDamage(finalDamage, AttackType.None);

        // 지정 횟수에 도달하면 방어력 감소
        if (currentHitCount == requiredHitCount && enemy.currentHP > 0)
        {
            enemy.ApplyDefenseReduction(defenseReductionRate, defenseReductionDuration);
        }

        Debug.Log($"[SpinAttack] {enemy.EnemyName} " + $"{currentHitCount}회 적중 / " + $"데미지: {finalDamage}");
    }

    private void Update()
    {
        if (spinCoroutine == null) return;

        // 스킬 사용 도중 회피, 피격, 사망 등으로 상태가 바뀌면 중단
        if (!CanContinueSpin())
        {
            StopSpin();
        }
    }

    public void StopSpin()
    {
        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
            spinCoroutine = null;
        }

        ClearEffect();

        // 캐릭터 외형 복구
        RestoreCharacterVisuals();

        hitCounts.Clear();
    }

    private void ClearEffect()
    {
        if (activeEffect != null)
        {
            Destroy(activeEffect);
            activeEffect = null;
        }
    }

    private void HideCharacterVisuals()
    {
        if (modelTransform == null) return;

        characterRenderers = modelTransform.GetComponentsInChildren<Renderer>(true);

        originalRendererStates = new bool[characterRenderers.Length];

        for (int i = 0; i < characterRenderers.Length; i++)
        {
            Renderer renderer = characterRenderers[i];

            if (renderer == null) continue;

            // 기존 상태 저장
            originalRendererStates[i] = renderer.enabled;

            // 외형 숨김
            renderer.enabled = false;
        }
    }

    private void RestoreCharacterVisuals()
    {
        if (characterRenderers == null) return;

        for (int i = 0; i < characterRenderers.Length; i++)
        {
            Renderer renderer = characterRenderers[i];

            if (renderer == null) continue;

            // 스킬 사용 전 상태로 복구
            renderer.enabled = originalRendererStates[i];
        }

        characterRenderers = null;
        originalRendererStates = null;
    }

    private void OnDisable()
    {
        StopSpin();
    }

    private void OnDrawGizmosSelected()
    {
        Player_Action player = GetComponent<Player_Action>();

        if (player == null || player.assignedSkills == null)
            return;

        foreach (Skill_Base skill in player.assignedSkills)
        {
            if (skill is Skill_SpinAttack spinSkill)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawWireSphere(transform.position + Vector3.up, spinSkill.AttackRadius);

                break;
            }
        }
    }
}