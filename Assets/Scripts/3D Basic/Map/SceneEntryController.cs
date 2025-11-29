using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SceneEntryController : MonoBehaviour
{
    void Start()
    {
        if (!string.IsNullOrEmpty(SceneTransferManager.TargetSpawnName))
        {
            GameObject spawnPoint = GameObject.Find(SceneTransferManager.TargetSpawnName);

            if (spawnPoint != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    NavMeshAgent agent = player.GetComponent<NavMeshAgent>();
                    if (agent != null) agent.enabled = false;

                    player.transform.position = spawnPoint.transform.position;
                    player.transform.rotation = spawnPoint.transform.rotation; // 바라보는 방향도 맞춤

                    Transform cameraArm = player.transform.Find("CameraArm");

                    if (cameraArm == null)
                    {
                        var camScript = player.GetComponentInChildren<CameraArm>();
                        if (camScript != null) cameraArm = camScript.transform;
                    }

                    if (cameraArm != null)
                    {
                        cameraArm.rotation = spawnPoint.transform.rotation;
                    }

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
