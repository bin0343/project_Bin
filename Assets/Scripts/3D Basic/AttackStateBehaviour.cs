using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackStateBehaviour : StateMachineBehaviour
{
    private TrailRenderer slashTrail;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (slashTrail == null)
        {
            slashTrail = animator.GetComponentInChildren<TrailRenderer>();
        }

        if (slashTrail != null)
        {
            slashTrail.emitting = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (slashTrail != null)
        {
            slashTrail.emitting = false;
        }
    }
}
