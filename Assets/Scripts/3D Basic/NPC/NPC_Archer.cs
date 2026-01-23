using System.Collections;
using UnityEngine;

public class NPC_Archer : NPCBase
{
    [Header("Archer Settings")]
    public GameObject arrowPrefab; // 화살 프리팹
    public Transform firePoint;    // 발사 위치 (활 끝부분 or 손)

    [Header("애니메이션 타이밍")]
    [Tooltip("활 당기기의 길이 (초)")]
    public float drawDuration = 1.017f;

    [Tooltip("2번 모션(발사) 시작 후 화살이 나가는 미세 딜레이 (필요시 0.05f 등으로 설정)")]
    public float shotTimeOffset = 0.2f;

    [Tooltip("발사 후 다음 행동까지 걸리는 시간")]
    public float recoveryTime = 1.0f;

    protected override IEnumerator AttackRoutine()
    {
        lastAttackTime = Time.time;

        animator.SetTrigger("IsAttack"); // 활 당기는 애니메이션

        float timer = 0f;
        while (timer < drawDuration)
        {
            timer += Time.deltaTime;
            // 당기는 도중 타겟을 계속 바라보게 함 (선택사항)
            if (currentTarget != null) transform.LookAt(currentTarget);
            yield return null;
        }
        // 당기는 도중 타겟이 죽거나 사라졌는지 체크
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            // 타겟이 없으면 쏘지 않고 IDLE로 복귀
            yield break;
        }

        animator.SetTrigger("IsShoot");

        if (shotTimeOffset > 0) yield return new WaitForSeconds(shotTimeOffset);

        FireArrow();
        
        // 후딜레이 (다음 행동까지 대기)
        //yield return new WaitForSeconds(1.0f);
    }

    private void FireArrow()
    {
        if (currentTarget == null) return;
        if (arrowPrefab == null || firePoint == null) return;

        // 발사 방향 계산
        Vector3 targetPos = currentTarget.position + Vector3.up * 1.5f;
        Vector3 dir = (targetPos - firePoint.position).normalized;

        // 화살 생성
        GameObject arrowObj = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        Projectile_Arrow arrowScript = arrowObj.GetComponent<Projectile_Arrow>();

        if (arrowScript != null)
        {
            int damage = GetComponent<NPC_Stat>().attackPower;
            arrowScript.Setup(dir, damage, currentTeam);
        }
    }
}