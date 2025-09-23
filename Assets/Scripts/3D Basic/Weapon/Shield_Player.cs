using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield_Player : MonoBehaviour
{
    private Player_Action playerAction; // 플레이어의 애니메이션 등을 제어하기 위한 참조
    private Collider defenseCollider;
    private Animator animator;

    void Awake()
    {
        // 부모나 최상위 오브젝트에서 Player_Action 스크립트를 찾아옴
        playerAction = GetComponentInParent<Player_Action>();
        defenseCollider = GetComponent<Collider>();
        animator = GetComponentInParent<Animator>();

        // 게임 시작 시에는 방어 판정이 없어야 하므로 콜라이더를 비활성화
        if (defenseCollider != null)
        {
            defenseCollider.enabled = false;
        }
    }

    // PlayerShieldState에서 호출하여 콜라이더를 켜고 끌 함수
    public void SetActiveShield(bool isActive)
    {
        if (defenseCollider != null)
        {
            defenseCollider.enabled = isActive;
        }
    }

    // 방어에 성공했을 때 Weapon_Brute 스크립트가 호출할 함수
    public void OnParrySuccess()
    {
        Debug.Log("플레이어: 방어 성공! 리액션 애니메이션 재생!");
        // Player_Action 스크립트에 있는 방어 리액션 함수를 호출
        if (animator != null)
        {
            animator.SetTrigger("ParrySuccess");
        }
        // 여기에 사운드 효과나 파티클 효과를 추가할 수 있습니다.
    }
}
