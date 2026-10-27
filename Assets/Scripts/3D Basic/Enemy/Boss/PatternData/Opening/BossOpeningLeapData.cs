using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOpeningLeap", menuName = "Enemy/Boss Opening/Opening Leap")]
public class BossOpeningLeapData : ScriptableObject
{
    [Header("점프 상승")]
    public int attackIndex = 3;
    [Min(0.1f)]
    public float takeOffDuration = 0.5f;
    [Min(1f)]
    public float hiddenHeight = 18f;

    [Header("낙하 공격 애니메이션")]
    [Range(1, 10)]
    public int fallAttackIndex = 8;
    [Min(0f)]
    public float landingRecoveryTime = 0.4f;

    [Header("경고 원")]
    [Min(0f)]
    public float trackingDuration = 1.5f;
    [Min(0f)]
    public float lockedWarningDuration = 1f;
    [Min(0.1f)]
    public float attackRadius = 3f;

    [Header("낙하")]
    [Min(0.1f)]
    public float fallDuration = 0.45f;
    [Min(0f)]
    public float landingStopDistance = 1.2f;

    [Header("공격")]
    [Min(0f)]
    public float damageMultiplier = 1.5f;
    public HitReactionType hitReactionType = HitReactionType.Knockback;

    [Header("착지 연출")]
    public BossEffectData impactEffect;

    public float shakeAmplitude = 0.35f;
    public float shakeDuration = 0.3f;
    public int shakeFrequency = 6;
}
