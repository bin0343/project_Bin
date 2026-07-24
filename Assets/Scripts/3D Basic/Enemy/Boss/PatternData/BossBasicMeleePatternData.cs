using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBasicMeleePattern", menuName = "Enemy/Boss Pattern/Basic Melee")]
public class BossBasicMeleePatternData : BossPatternDataBase
{
    public override BossPatternType PatternType
    {
        get { return BossPatternType.BasicMelee; }
    }
}
