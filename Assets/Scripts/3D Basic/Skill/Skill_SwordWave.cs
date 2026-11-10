using UnityEngine;

[CreateAssetMenu(fileName = "NewSwordWaveSkill", menuName = "Skill/Sword Wave")]
public class Skill_SwordWave : Skill_Base
{
    [Header("검기 데미지")]
    [SerializeField, Min(0f)]
    private float attackMultiplier = 1.3f;

    [SerializeField, Min(0)]
    private int bonusDamage = 0;

    [Header("검기 이동")]
    [SerializeField, Min(0.1f)]
    private float projectileSpeed = 18f;

    [SerializeField, Min(0.1f)]
    private float projectileRange = 12f;

    [SerializeField]
    private Vector3 spawnOffset = new Vector3(0f, 1.2f, 1.3f);

    [Header("검기 타격 판정")]
    [SerializeField]
    private Vector3 hitboxSize = new Vector3(2.5f, 2f, 0.6f);

    [Header("검기 프리팹")]
    [SerializeField]
    private GameObject projectilePrefab;

    [Header("장애물")]
    [SerializeField]
    private LayerMask obstacleLayer;

    protected override void ApplyEffect(GameObject user, Vector3 targetPosition)
    {
        if (user == null || projectilePrefab == null) return;

        Player_Action player = user.GetComponent<Player_Action>();

        if (player == null) player = user.GetComponentInParent<Player_Action>();

        Animator animator = user.GetComponentInChildren<Animator>();

        Transform modelTransform = animator != null ? animator.transform : user.transform;

        // 기존 근접 캐릭터의 자동 타겟팅 활용
        Vector3 direction = modelTransform.forward;

        if (player != null)
        {
            Transform target = player.FindNearestEnemyInRange(projectileRange);

            if (target != null)
            {
                direction = target.position - modelTransform.position;
            }
        }

        // 검기는 수평으로 발사
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) direction = user.transform.forward;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) direction = Vector3.forward;

        direction.Normalize();

        // 캐릭터 외형을 공격 방향으로 회전
        modelTransform.rotation = Quaternion.LookRotation(direction);

        Vector3 spawnPosition = modelTransform.position + modelTransform.TransformDirection(spawnOffset);

        Quaternion spawnRotation = Quaternion.LookRotation(direction);

        // 기존 Skill_Base 공통 공격력 계산
        int skillAttackPower = CalculateSkillDamage(user, attackMultiplier, bonusDamage);

        GameObject waveObject = Instantiate(projectilePrefab, spawnPosition, spawnRotation);

        SkillSwordWaveProjectile projectile = waveObject.GetComponent<SkillSwordWaveProjectile>();

        if (projectile == null)
        {
            Debug.LogError("[SwordWave] 프리팹에 SkillSwordWaveProjectile이 없습니다.");

            Destroy(waveObject);
            return;
        }

        LayerMask targetLayer = player != null ? player.enemyLayer : LayerMask.GetMask("Enemy");

        projectile.Initialize(skillAttackPower, projectileSpeed, projectileRange, hitboxSize, targetLayer, obstacleLayer);
    }
}