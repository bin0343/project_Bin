using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FarmingStageRewardData", menuName = "Farming/Stage Reward Data")]
public class FarmingStageRewardData : ScriptableObject
{
    [Header("AP ºñ¿ë")]
    [Min(1)] public int baseAPCost = 40;

    [Header("È¹µæ °¡´É º¸»ó")]
    public List<FarmingRewardEntry> rewards = new List<FarmingRewardEntry>();
}