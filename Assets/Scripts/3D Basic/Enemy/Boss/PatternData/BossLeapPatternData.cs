using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLeapPatten", menuName = "Enemy/Boss Pattern/Leap")]
public class BossLeapPatternData : BossPatternDataBase
{
    public override BossPatternType PatternType
    {
        get { return BossPatternType.Leap; }
    }

    [Header("점프 공격 설정")]
    [Tooltip("점프 공격 애니메이션을 재생할지")]
    public bool playLeapAnimation = true;
    [Min(0f)]
    [Tooltip("점프 포물선의 최대 높이")]
    public float leapHeight = 5f;
    [Min(0.1f)]
    [Tooltip("점프 시작부터 착지 충격까지 걸리는 시간")]
    public float leapTimeToImpact = 0.9f;
    [Min(0f)]
    [Tooltip("착지 후 다음 행동까지의 후딜")]
    public float leapRecoveryTime = 0.5f;
    [Min(0.1f)]
    [Tooltip("착지 범위 공격 반경")]
    public float leapAttackRadius = 3f;
    [Tooltip("착탄 지점 경고 원을 표시할지")]
    public bool showLeapTelegraph = true;
    [Min(0f)]
    [Tooltip("플레이어 중심과 보스 착지 위치 사이의 거리")]
    public float leapLandingStopDistance = 1.2f;

    [Header("점프 착지 이펙트")]
    public BossEffectData impactEffect;

    [Header("점프 착지 카메라 흔들림")]
    [Min(0f)]
    public float leapShakeAmplitude = 0.25f;
    [Min(0f)]
    public float leapShakeDuration = 0.25f;
    [Min(0)]
    public int leapShakeFrequency = 5;
}
