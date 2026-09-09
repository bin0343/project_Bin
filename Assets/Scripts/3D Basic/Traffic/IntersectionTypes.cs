using System;
using UnityEngine;
using UnityEngine.Splines;

public enum IntersectionTurnType
{
    Straight,
    Left,
    Right
}

[Serializable]
public class IntersectionPathChoice
{
    public SplineContainer path;

    public IntersectionTurnType turnType;

    [Min(0)]
    public int movementId;
}