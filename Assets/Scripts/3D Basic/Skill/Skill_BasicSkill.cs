using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnhancedAttack", menuName = "Skill/Enhanced Attack")]
public class Skill_BasicSkill : Skill_Base
{
    [Header("Enhanced Attack Properties")]
    public int damage;
    public float knockbackForce;
    public float attackRadius;

    protected override void ApplyEffect(GameObject user) { }
}
