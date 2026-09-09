using UnityEngine;

public class RoadLaneData : MonoBehaviour
{
    [Header("차선")]
    [Min(1)]
    public int laneCount = 1;

    [Min(0.5f)]
    public float laneWidth = 3.2f;

    [Header("차선 변경")]
    public bool allowLaneChange = false;
}