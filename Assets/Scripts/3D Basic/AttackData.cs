using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackEffectType
{
    None,
    Normal,
    Knockback
}

[CreateAssetMenu(fileName = "New Attack Data", menuName = "Data/Attack Data")]
public class AttackData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("공격의 종류 (이 값에 따라 Enemy_Stat에서 다른 효과를 적용)")]
    public AttackType attackType = AttackType.Normal;

    [Header("데미지 설정")]
    [Tooltip("플레이어 데미지 배율(기본 1.0f)")]
    public float damageMultiplier = 1.0f;

    [Tooltip("배율 계산 후 추가될 고정 데미지")]
    public int bonusDamage = 0;

    // [Header("부가 효과")]    
    // public GameObject hitEffectPrefab;
    // public AudioClip hitSound;
}
