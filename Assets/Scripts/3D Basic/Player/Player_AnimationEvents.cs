using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_AnimationEvents : MonoBehaviour
{
    Player_Action Action;

    void Start()
    {
        Action = GetComponentInParent<Player_Action>();
    }

    public void KickEnd()
    {
        Action.IsKick = false;
    }

    public void BuffEnd()
    {
        Action.IsBuff = false;
    }
}
