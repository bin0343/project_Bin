using System.Collections;
using UnityEngine;

public class NPC_Archer : NPCBase
{
    [Header("Archer Settings")]
    public GameObject arrowPrefab; // 화살 프리팹
    public Transform firePoint;    // 발사 위치 (활 끝부분 or 손)

    protected override IEnumerator AttackRoutine()
    {
        animator.SetTrigger("IsAttack"); // 활 쏘는 애니메이션

        // 화살 발사 타이밍까지 대기 (애니메이션에 따라 조절, 예: 0.3초 뒤 발사)
        yield return new WaitForSeconds(0.3f);

        if (currentTarget != null)
        {
            // 발사 방향 계산 (타겟의 가슴 쪽을 향하도록)
            Vector3 targetPos = currentTarget.position + Vector3.up * 1.0f;
            Vector3 dir = (targetPos - firePoint.position).normalized;

            // 화살 생성
            GameObject arrowObj = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
            Projectile_Arrow arrowScript = arrowObj.GetComponent<Projectile_Arrow>();

            // 화살 정보 주입 (공격력은 NPC 스탯 기반)
            int damage = GetComponent<NPC_Stat>().attackPower;
            arrowScript.Setup(dir, damage, currentTeam);
        }

        // 후딜레이 (다음 행동까지 대기)
        yield return new WaitForSeconds(1.0f);
    }
}