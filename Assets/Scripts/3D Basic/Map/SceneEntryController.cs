using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SceneEntryController : MonoBehaviour
{
    void Start()
    {
        // 1. 메모장에 적힌 이름이 있는지 확인 ("Spawn_FromSchool" 같은 거)
        if (!string.IsNullOrEmpty(SceneTransferManager.TargetSpawnName))
        {
            // 2. 그 이름을 가진 오브젝트(스폰 포인트)를 씬에서 찾음
            GameObject spawnPoint = GameObject.Find(SceneTransferManager.TargetSpawnName);

            if (spawnPoint != null)
            {
                // 3. 플레이어 찾기
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    // NavMeshAgent가 켜져 있으면 강제 이동이 안 될 수 있으므로 잠시 끔
                    NavMeshAgent agent = player.GetComponent<NavMeshAgent>();
                    if (agent != null) agent.enabled = false;

                    // [이동] 플레이어를 스폰 포인트 위치로 순간이동
                    player.transform.position = spawnPoint.transform.position;
                    player.transform.rotation = spawnPoint.transform.rotation; // 바라보는 방향도 맞춤

                    if (agent != null) agent.enabled = true;

                    Debug.Log($"플레이어를 {SceneTransferManager.TargetSpawnName} 위치로 이동시켰습니다.");
                }
            }
            else
            {
                Debug.LogWarning($"이름이 '{SceneTransferManager.TargetSpawnName}'인 스폰 포인트를 찾을 수 없습니다!");
            }

            // 4. 사용한 메모는 지워줌 (다음 이동을 위해)
            SceneTransferManager.TargetSpawnName = "";
        }
    }
}
