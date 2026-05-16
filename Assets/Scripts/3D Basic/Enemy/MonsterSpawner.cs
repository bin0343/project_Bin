using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject monsterPrefab;
    public Transform[] spawnPoints;

    [Header("스폰 시작 옵션")]
    public bool spawnOnStart = false;   //이벤트 용인지 체크(시작 시 스폰이면 체크)

    [Header("스폰 방식")]
    public bool isInfiniteSpawn = false;    //무한 스폰
    public int totalMaxSpawn = 10;      //총 스폰 몬스터 수 

    [Header("웨이브 설정")]
    public int maxAliveAtOnce = 5;      //필드 동시 존재 최대 마리
    public float spawnDelay = 2f;       //몬스터 등장 간격

    private int currentTotalSpawned = 0;
    private List<GameObject> activeMonsters = new List<GameObject>();
    private bool isSpawningActive = false;

    private void Start()
    {
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        if (isSpawningActive) return;

        isSpawningActive = true;
        StartCoroutine(SpawnRoutine());
        Debug.Log("몬스터 스폰 이벤트가 시작되었습니다!");
    }

    public void StopSpawning()
    {
        isSpawningActive = false;
        StopAllCoroutines();
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawningActive)
        {
            activeMonsters.RemoveAll(monster => monster == null);

            if (!isInfiniteSpawn && currentTotalSpawned >= totalMaxSpawn)
            {
                if (activeMonsters.Count == 0)
                {
                    Debug.Log("모든 몬스터 웨이브 처치 완료!");
                    StopSpawning();
                }

                yield return null;
                continue; 
            }

            if (activeMonsters.Count < maxAliveAtOnce)
            {
                SpawnMonster();
                yield return new WaitForSeconds(spawnDelay);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private void SpawnMonster()
    {
        if (monsterPrefab == null || spawnPoints.Length == 0) return;

        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject newMonster = Instantiate(monsterPrefab, randomPoint.position, randomPoint.rotation);
        activeMonsters.Add(newMonster);

        currentTotalSpawned++;
    }
}
