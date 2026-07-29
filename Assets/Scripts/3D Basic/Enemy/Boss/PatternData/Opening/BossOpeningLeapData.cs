using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOpeningLeap", menuName = "Enemy/Boss Opening/Opening Leap")]
public class BossOpeningLeapData : ScriptableObject
{
    [Header("Á¡ÇÁ »ó½Â")]
    public int attackIndex = 3;
    [Min(0.1f)]
    public float takeOffDuration = 0.5f;
    [Min(1f)]
    public float hiddenHeight = 18f;

    [Header("°æ°í ¿ø")]
    [Min(0f)]
    public float trackingDuration = 1.5f;
    [Min(0f)]
    public float lockedWarningDuration = 1f;
    [Min(0.1f)]
    public float attackRadius = 3f;

    [Header("³«ÇÏ")]
    [Min(0.1f)]
    public float fallDuration = 0.45f;
    [Min(0f)]
    public float landingStopDistance = 1.2f;

    [Header("°ø°Ý")]
    [Min(0f)]
    public float damageMultiplier = 1.5f;
    public HitReactionType hitReactionType = HitReactionType.Knockback;

    [Header("ÂøÁö ¿¬Ãâ")]
    public BossEffectData impactEffect;

    public float shakeAmplitude = 0.35f;
    public float shakeDuration = 0.3f;
    public int shakeFrequency = 6;
}
