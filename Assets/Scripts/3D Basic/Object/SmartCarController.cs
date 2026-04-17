using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class SmartCarController : MonoBehaviour
{
    [Header("이 차가 따라갈 차선(웨이포인트들)")]
    public Transform[] waypoints;

    [Header("자동차 주행 속도")]
    public float speed = 10f; // SetSpeedBased 덕분에 이 값이 '진짜 속도'가 됩니다.

    void Start()
    {
        if (waypoints.Length == 0) return;
        StartDriving();
    }

    void StartDriving()
    {
        // 1. 내 현재 위치에서 가장 가까운 웨이포인트 번호 찾기
        int startIndex = FindClosestWaypointIndex();

        // 2. 가장 가까운 포인트부터 시작하도록 경로 배열 재구성
        List<Vector3> myPath = new List<Vector3>();
        for (int i = startIndex; i < waypoints.Length; i++)
            myPath.Add(waypoints[i].position);
        for (int i = 0; i < startIndex; i++)
            myPath.Add(waypoints[i].position);

        // 3. DOTween으로 주행 시작
        transform.DOPath(myPath.ToArray(), speed, PathType.CatmullRom)
                 .SetOptions(false, AxisConstraint.None, AxisConstraint.Z) // ★추가됨: 차가 옆으로 눕는 현상(Z축 회전) 방지
                 .SetSpeedBased()
                 .SetLookAt(0.01f)
                 .SetEase(Ease.Linear)
                 .SetLoops(-1, LoopType.Restart);
    }

    // 가장 가까운 웨이포인트를 찾는 함수
    int FindClosestWaypointIndex()
    {
        int closestIndex = 0;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < waypoints.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, waypoints[i].position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestIndex = i;
            }
        }
        return closestIndex;
    }
}