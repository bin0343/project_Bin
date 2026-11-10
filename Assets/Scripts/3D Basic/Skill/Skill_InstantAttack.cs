using UnityEngine;

[CreateAssetMenu(fileName = "NewInstantAttackSkill", menuName = "Skill/Instant Attack")]
public class Skill_InstantAttack : Skill_Base
{
    [Header("공격력 설정")]

    [Tooltip("캐릭터 공격력에 적용할 배율. 1 = 100%, 1.5 = 150%")]
    [Min(0f)] public float attackMultiplier = 1f;

    [Tooltip("공격력 배율 계산 후 추가할 고정 데미지")]
    [Min(0)] public int damageAmount = 0;

    [Header("공격 판정 설정(데이터 주도)")]
    public Vector3 hitboxSize = new Vector3(3f, 3f, 4f);

    [Tooltip("시전자 정면 기준 가상의 히트박스가 생성될 오프셋 위치 (x, y, z)")]
    public Vector3 spawnOffset = new Vector3(0f, 1f, 2f);

    [Tooltip("이펙트가 소환된 후 실제 대미지 판정이 일어날때까지의 시간")]
    public float damageDelay = 0.1f;

    [Header("Visuals")]
    [Tooltip("SkillEffectEntity 컴포넌트가 포함된 순수 파티클 프리팹")]
    public GameObject effectPrefab;

    [Header("카메라 흔들림")]
    [SerializeField] private bool useCameraShake = true;
    [SerializeField] private float shakeAmplitude = 1.0f;
    [SerializeField] private float shakeFrequency = 15f;
    [SerializeField] private float shakeDuration = 0.12f;

    protected override void ApplyEffect(GameObject user, Vector3 targetPosition)
    {
        if (user == null) return;
        if (effectPrefab == null) return;

        int skillDamage = CalculateSkillDamage(user, attackMultiplier, damageAmount);

        if (skillDamage <= 0) return;

        Transform modelTransform = user.GetComponentInChildren<Animator>().transform;
        Vector3 spawnPos = modelTransform.position + modelTransform.TransformDirection(spawnOffset);

        GameObject skillGo = Instantiate(effectPrefab, spawnPos, modelTransform.rotation);

        if (useCameraShake && CameraShakeManager.instance != null)
        {
            CameraShakeManager.instance.Shake(shakeAmplitude, shakeFrequency, shakeDuration);
        }

        SkillEffectEntity entity = skillGo.GetComponent<SkillEffectEntity>();
        if (entity == null) entity = skillGo.AddComponent<SkillEffectEntity>();

        LayerMask enemyLayer = LayerMask.GetMask("Enemy");
        entity.Setup(skillDamage, hitboxSize, damageDelay, enemyLayer);
    }
}
