using UnityEngine;

public class NPC_AnimationEvents : MonoBehaviour
{
    private NPCBase npcBase;

    void Start()
    {
        // 부모 오브젝트에 있는 NPCBase 스크립트를 찾습니다.
        npcBase = GetComponentInParent<NPCBase>();
    }

    // ▼▼▼ 오류가 났던 그 함수 이름 그대로 만들어줍니다 ▼▼▼
    public void OnAttackAnimationEnd()
    {
        // 공격 애니메이션이 끝났으니, NPCBase에게 "공격 끝났다"고 알립니다.
        if (npcBase != null)
        {
            npcBase.OnAttackFinished();
        }
    }

    // (참고) 플레이어 애니메이션에 다른 이벤트들도 심어져 있다면 
    // 오류 방지를 위해 빈 함수라도 만들어두는 것이 좋습니다.
    public void AttackStart() { }
    public void AttackEnd() { }
    public void OnAttackCombo() { }
    public void EnableAttackHitbox() { } // 필요하다면 공격 판정 켜는 로직 추가
    public void DisableAttackHitbox() { }
    public void CameraShake_Attack() { }
}