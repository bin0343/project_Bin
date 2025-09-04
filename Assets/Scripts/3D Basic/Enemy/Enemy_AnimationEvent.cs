using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_AnimationEvent : MonoBehaviour
{
    public Animator animator;

    [Header("Attack Effect")]
    [SerializeField] private TrailRenderer slashTrail;

    void Start()
    {
        slashTrail.emitting = false;
    }

    public void StartAttackTrail()
    {
        slashTrail.emitting = true;
    }

    public void EndAttackTrail()
    {
        slashTrail.emitting = false;
    }
}
