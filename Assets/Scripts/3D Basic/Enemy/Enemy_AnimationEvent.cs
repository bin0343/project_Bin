using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_AnimationEvent : MonoBehaviour
{
    public Animator animator;

    public void OnDeathAnimationEnd()
    {
        // 시체 확인 가능 상태로 전환
        //gameObject.AddComponent<CorpseInteractable>().Setup(dropTable);
    }
}
