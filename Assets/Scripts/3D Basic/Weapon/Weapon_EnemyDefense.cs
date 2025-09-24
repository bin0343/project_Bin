using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_EnemyDefense : MonoBehaviour
{
    private EnemyBase enemyBase; // 플레이어의 애니메이션 등을 제어하기 위한 참조
    private Collider defenseCollider;
    private Animator animator;

    void Awake()
    {
        enemyBase = GetComponentInParent<EnemyBase>();
        defenseCollider = GetComponent<Collider>();
        animator = GetComponentInParent<Animator>();

        if (defenseCollider != null)
        {
            defenseCollider.enabled = false;
        }
    }

    public void SetActiveDefense(bool isActive)
    {
        if (defenseCollider != null)
        {
            defenseCollider.enabled = isActive;
        }
    }

    public void OnParrySuccess()
    {
        Debug.Log("플레이어: 방어 성공! 리액션 애니메이션 재생!");
        if (animator != null)
        {
            animator.SetTrigger("ParrySuccess");
        }
        // 여기에 사운드 효과나 파티클 효과를 추가할 수 있습니다.
    }
}
