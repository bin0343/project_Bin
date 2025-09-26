using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldStateController : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Weapon_EnemyDefense defense = animator.GetComponentInChildren<Weapon_EnemyDefense>(true);       //animator가 계속 100% 살아있을 지 
        if (defense != null)
        {
            defense.SetActiveDefense(false);
        }
    }
}
