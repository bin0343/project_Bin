using UnityEngine;

public class NPC_AnimationEvents : MonoBehaviour
{
    private NPCBase npcBase;

    // 필요하다면 오디오 소스나 무기 스크립트 등을 연결
    // public AudioSource audioSource;
    // public Enemy_Weapon currentWeapon;

    void Start()
    {
        // NPCBase나 관련 컴포넌트를 부모나 자신에게서 찾음
        npcBase = GetComponentInParent<NPCBase>();
    }

    public void CameraShakeEvent() { }
    public void CameraShake_Attack() { }
    public void BuffStart() { }
    public void BuffEnd() { }
    public void KickEnd() { }
    public void OnJumpAttackEnd() { }
    public void StartAttackTrail() { }
    public void EndAttackTrail() { }
    public void EnableAttackHitbox() { }
    public void DisableAttackHitbox() { }
    public void AttackStart() { }
    public void AttackEnd() { }
    public void OnAttackCombo() { }
    public void OnComboWindowOpen() { }
    public void OnAttackAnimationEnd() { }
    public void OnHitAnimationEnd() { }
    public void ResetRandomIdle() { }
}