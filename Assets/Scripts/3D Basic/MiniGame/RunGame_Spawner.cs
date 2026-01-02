using UnityEngine;

public class RunGame_Spawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float spawnInterval = 2.0f;
    private float timer = 0f;

    void Update()
    {
        if (RunGame_Manager.instance.isGameOver) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnObstacle();
            timer = 0f;

            // 시간이 지날수록 조금씩 더 빨리 나오게 하기 (선택사항)
            if (spawnInterval > 0.8f) spawnInterval -= 0.05f;
        }
    }

    void SpawnObstacle()
    {
        // 생성 위치는 스포너의 위치
        Instantiate(obstaclePrefab, transform.position, Quaternion.identity);
    }
}