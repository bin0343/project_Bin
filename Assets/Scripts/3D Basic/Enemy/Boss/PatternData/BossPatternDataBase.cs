using System.Collections.Generic;
using UnityEngine;

public enum BossPatternDistance
{
    Close,
    Far,
    Both
}

public enum BossPatternType
{
    BasicMelee,
    Dash,
    Ranged,
    Leap,
    Spin
}

[System.Serializable]
public class BossEffectData
{
    [Tooltip("생성할 이펙트 프리팹")]
    public GameObject prefab;

    [Min(0f)]
    [Tooltip("이펙트 유지 시간")]
    public float lifetime = 2f;

    [Min(0f)]
    [Tooltip("바닥과 겹치지 않게 올릴 높이")]
    public float groundOffset = 0.05f;

    [Min(0.01f)]
    [Tooltip("프리팹 Scale 1일 때의 기본 반경")]
    public float baseRadius = 1f;

    [Tooltip("이펙트 방향 추가 보정")]
    public Vector3 rotationOffset;
}

public abstract class BossPatternDataBase : ScriptableObject
{
    [Header("패턴 기본 정보")]
    public string patternName;

    [TextArea] public string description;

    [Header("사용 조건")]
    [Tooltip("패턴을 사용하는 페이즈")]
    public List<BossPhase> usablePhases = new List<BossPhase>();
    [Tooltip("패턴 사용 가능한 거리")]
    public BossPatternDistance usableDistance = BossPatternDistance.Both;

    [Header("패턴 선택 설정")]
    [Min(0f)]
    [Tooltip("선택 가중치(이 패턴을 쓸 가중치)")]
    public float selectionWeight = 1f;
    [Min(0f)]
    [Tooltip("패턴 쿨타임")]
    public float cooldown = 3f;

    [Header("연계 가중치")]
    [Tooltip("직전 패턴에 따라 선택 가중치를 높일지")]
    public bool patternWeightBonus = false;
    [Tooltip("가중치 보너스 적용 직전 패턴")]
    public BossPatternType previousPatternType = BossPatternType.Dash;
    [Min(1f)]
    [Tooltip("가중치 보너스 배율")]
    public float patternWeightMultiplier = 5f;

    [Header("공격 애니메이션")]
    [Tooltip("애니메이터에 전달할 공격 번호")]
    [Range(1, 10)] public int attackIndex = 1;
    [Tooltip("공격 시작부터 패턴 종료까지 걸리는 시간")]
    [Min(0.1f)] public float animationTime = 1.5f;

    [Header("공격 효과")]
    [Tooltip("보스 공격력 데미지 비율")]
    [Min(0f)] public float damageMultiplier = 1f;
    [Tooltip("플레이어 피격 반응")]
    public HitReactionType hitReactionType = HitReactionType.Normal;

    public abstract BossPatternType PatternType { get; }
}
