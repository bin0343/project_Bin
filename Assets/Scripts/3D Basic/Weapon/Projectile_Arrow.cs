using UnityEngine;

public class Projectile_Arrow : MonoBehaviour
{
    private float speed = 15.0f;
    private int damage;
    private NPCTeam ownerTeam; // 누가 쐈는지 (아군이 쐈으면 적만 맞춰야 함)
    private float maxLifeTime = 5.0f; // 5초 뒤 자동 삭제

    public void Setup(Vector3 direction, int dmg, NPCTeam team)
    {
        damage = dmg;
        ownerTeam = team;

        // 화살이 바라보는 방향 설정
        transform.rotation = Quaternion.LookRotation(direction);

        // 일정 시간 후 삭제 (메모리 관리)
        Destroy(gameObject, maxLifeTime);
    }

    void Update()
    {
        // 앞으로 전진
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. 발사한 사람(자신)과는 충돌 무시 (레이어 설정으로도 가능하지만 코드로도 안전장치)
        // 여기서는 Team으로 식별

        // 2. 피격 판정
        if (ownerTeam == NPCTeam.Ally) // 아군 NPC가 쏜 경우
        {
            if (other.CompareTag("Enemy"))
            {
                HitTarget(other.gameObject);
            }
        }
        else if (ownerTeam == NPCTeam.Enemy) // 적 NPC가 쏜 경우
        {
            if (other.CompareTag("Player") || other.CompareTag("Companion"))
            {
                HitTarget(other.gameObject);
            }
        }
        // 지형(Ground/Wall)에 부딪히면 그냥 사라짐
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    private void HitTarget(GameObject target)
    {
        // 대상의 스탯 가져오기 (적 or 플레이어 or NPC)
        Enemy_Stat enemyStat = target.GetComponent<Enemy_Stat>();
        if (enemyStat != null)
        {
            enemyStat.TakeDamage(damage, AttackType.Normal);
        }
        else
        {
            // 플레이어나 아군 NPC 피격 처리는 해당 스크립트(Player_Stat 등)에 맞춰 추가
            NPC_Stat npcStat = target.GetComponent<NPC_Stat>();
            if (npcStat != null) npcStat.TakeDamage(damage);

            Player_Stat playerStat = target.GetComponent<Player_Stat>();
            // if (playerStat != null) playerStat.TakeDamage(damage); 
        }

        // 이펙트 생성 로직이 있다면 여기에 추가
        Destroy(gameObject); // 화살 삭제
    }
}