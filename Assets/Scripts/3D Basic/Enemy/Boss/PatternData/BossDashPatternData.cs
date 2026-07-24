using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDashPattern", menuName = "Enemy/Boss Pattern/Dash")]
public class BossDashPatternData : BossPatternDataBase
{
    public override BossPatternType PatternType
    {
        get { return BossPatternType.Dash; }
    }

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

    [Header("돌진 연출")]
    [Tooltip("돌진할 때 Animator 공격 애니메이션을 재생할지")]
    public bool playDashAnimation = true;
    [Tooltip("돌진 선딜 동안 바닥 경고선을 표시할지")]
    public bool showDashTelegraph = true;
    [Min(0.05f)]
    [Tooltip("바닥에 표시할 돌진 경고선의 너비")]
    public float dashTelegraphWidth = 1.5f;
}
