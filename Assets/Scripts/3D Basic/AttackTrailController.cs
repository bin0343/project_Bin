using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AttackTrailController : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        TrailRenderer slashTrail = animator.GetComponentInChildren<TrailRenderer>();
        if (slashTrail != null)
        {
            slashTrail.emitting = false;
            //return;
        }

        Collider collider = animator.GetComponentInChildren<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }
}
