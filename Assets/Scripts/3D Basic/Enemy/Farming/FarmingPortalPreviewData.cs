using System.Collections.Generic;
using UnityEngine;

public class FarmingPortalPreviewData : MonoBehaviour
{
    [Header("보상 수령 비용")]
    public int rewardAPCost = 40;

    [Header("획득 가능 아이템 미리보기")]
    public List<Item_Base> previewRewardItems = new List<Item_Base>();
}