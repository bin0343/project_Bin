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

[CreateAssetMenu(fileName = "NewBossPattern", menuName = "Enemy/Boss Pattern Data")]
public class BossPatternData : ScriptableObject
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

    [Header("보스 실행 종류")]
    public BossPatternType patternType;

    [Header("돌진 설정")]
    [Min(0f)]
    [Tooltip("돌진으로 이동할 최대 거리")]
    public float dashDistance = 8f;
    [Min(0.01f)]
    [Tooltip("돌진 이동에 걸리는 시간")]
    public float dashDuration = 0.6f;
    [Min(0f)]
    [Tooltip("방향을 고정한 뒤 실제 돌진하기 전 대기 시간")]
    public float dashWindupTime = 0.4f;
    [Tooltip("돌진 시간에 따른 누적 이동 비율")]
    public AnimationCurve dashMoveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("공격 애니메이션")]
    [Tooltip("애니메이터에 전달할 공격 번호")]
    [Range(1, 3)] public int attackIndex = 1;
    [Tooltip("공격 시작부터 패턴 종료까지 걸리는 시간")]
    [Min(0.1f)] public float animationTime = 1.5f;

    [Header("공격 효과")]
    [Tooltip("보스 공격력 데미지 비율")]
    [Min(0f)] public float damageMultiplier = 1f;
    [Tooltip("플레이어 피격 반응")]
    public HitReactionType hitReactionType = HitReactionType.Normal;
}
