using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject monsterPrefab;    //스폰 몬스터 프리팹
    public int maxCount = 5;    //최대 스폰 수
    public float spawnRadius = 10f; //스폰 반경
    public float respawnTime = 5f;  //리스폰까지 걸리는 시간

    private List<GameObject> spawnedMonsters = new List<GameObject>();

    private void Start()
    {
        for (int i = 0; i < maxCount; i++)
        {
            SpawnMonster();
        }
    }

    public void SpawnMonster()
    {
        if (monsterPrefab == null) return;

        Vector3 spawnPos = GetRandomPointOnNavMesh();

        GameObject newMonster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);

        spawnedMonsters.Add(newMonster);

        EnemyBase enemyScript = newMonster.GetComponent<EnemyBase>();
        enemyScript.SetSpawner(this);
        if (enemyScript != null )
        {
            StartCoroutine(CheckMonsterStatus(newMonster));
        }
    }

    public void OnMonsterDead(GameObject deadMonster)
    {
        if (spawnedMonsters.Contains(deadMonster))
        {
            spawnedMonsters.Remove(deadMonster);
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTime);
        // 현재 마릿수가 최대치보다 적을 때만 스폰 (혹시 모르니 체크)
        if (spawnedMonsters.Count < maxCount)
        {
            SpawnMonster();
        }
    }

    IEnumerator CheckMonsterStatus(GameObject monster)
    {
        while (monster != null)
        {
            // 몬스터가 비활성화되거나 EnemyBase의 isDead가 true인지 확인
            var enemyBase = monster.GetComponent<EnemyBase>();
            if (enemyBase != null && enemyBase.isDead)
            {
                // 죽음 처리 후 루프 종료
                OnMonsterDead(monster);
                yield break;
            }
            yield return new WaitForSeconds(1f); // 1초마다 확인
        }
        // 몬스터 오브젝트가 파괴되었다면(null) 죽은 것으로 간주
        OnMonsterDead(null);
    }

    Vector3 GetRandomPointOnNavMesh()
    {
        for (int i = 0; i < 30; i++) // 최대 30번 시도
        {
            Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
            randomPos += transform.position;

            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return transform.position; // 실패하면 스포너 위치 리턴
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
#endif
}
