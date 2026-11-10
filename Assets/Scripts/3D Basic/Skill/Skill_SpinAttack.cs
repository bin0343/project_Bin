using UnityEngine;

[CreateAssetMenu(fileName = "NewSpinAttackSkill", menuName = "Skill/Spin Attack")]
public class Skill_SpinAttack : Skill_Base
{
    [Header("회전 공격 설정")]
    [SerializeField, Min(0.1f)]
    private float attackRadius = 2.5f;
    [SerializeField, Range(1, 20)]
    private int totalHitCount = 8;

    [Header("토네이도 이동")]
    [SerializeField, Min(0f)]
    private float tornadoMoveSpeed = 8f;
    public float TornadoMoveSpeed => tornadoMoveSpeed;

    [Header("타격당 데미지")]
    [SerializeField, Min(0f)]
    private float attackMultiplier = 0.35f;
    [SerializeField, Min(0)]
    private int bonusDamage = 0;

    [Header("방어력 감소")]
    [SerializeField, Min(1)]
    private int requiredHitCount = 5;
    [SerializeField, Range(0f, 1f)]
    private float defenseReductionRate = 0.25f;
    [SerializeField, Min(0.1f)]
    private float defenseReductionDuration = 5f;

    [Header("회전 이펙트")]
    [SerializeField]
    private GameObject spinEffectPrefab;
    [SerializeField]
    private Vector3 effectOffset = new Vector3(0f, 0.8f, 0f);

    public float AttackRadius => attackRadius;

    protected override void ApplyEffect(GameObject user, Vector3 targetPosition)
    {
        if (user == null) return;

        Player_Action player = user.GetComponent<Player_Action>();

        if (player == null) player = user.GetComponentInParent<Player_Action>();

        if (player == null)
        {
            Debug.LogError("[SpinAttack] Player_Action을 찾지 못했습니다.");
            return;
        }

        if (duration <= 0f)
        {
            Debug.LogError("[SpinAttack] Duration을 설정해 주세요.");
            return;
        }

        // 타격 1회당 공격력 계산
        int damagePerHit = CalculateSkillDamage(user, attackMultiplier, bonusDamage);

        // 회전 공격 실행 컴포넌트 가져오기
        SkillSpinAttackRuntime runtime = player.GetComponent<SkillSpinAttackRuntime>();

        if (runtime == null)
        {
            runtime = player.gameObject.AddComponent<SkillSpinAttackRuntime>();
        }

        // 실제 회전 공격 시작
        runtime.Begin(
            player,
            duration,
            totalHitCount,
            attackRadius,
            damagePerHit,
            player.enemyLayer,
            requiredHitCount,
            defenseReductionRate,
            defenseReductionDuration,
            spinEffectPrefab,
            effectOffset
        );
    }
}