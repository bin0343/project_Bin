using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAttackSkill", menuName = "Skill/AreaAttack")]
public class Skill_AreaAttack : Skill_Base
{
    [Header("Targeting Properties")]
    public float castRange = 15f; // 스킬을 사용할 수 있는 최대 사거리

    [Header("Attack Properties")]
    public float attackRadius = 5f; // 지정한 위치에 발생할 피해 범위
    public int damageAmount = 50;

    [Header("Visuals")]
    public GameObject castRangeIndicatorPrefab; // 최대 사거리 표시용 프리팹 (큰 원)
    public GameObject targetIndicatorPrefab; // 마우스 커서를 따라다닐 피해 범위 표시용 프리팹 (작은 원)
    public GameObject effectPrefab; // 실제 스킬 효과 프리팹 (예: 메테오, 번개)

    protected override void ApplyEffect(GameObject user) { }
}
