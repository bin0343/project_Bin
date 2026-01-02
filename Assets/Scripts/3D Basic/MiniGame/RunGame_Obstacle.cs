using UnityEngine;

public class RunGame_Obstacle : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        if (RunGame_Manager.instance.isGameOver) return;

        // 왼쪽으로 이동
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // 화면 밖으로 나가면 삭제 (x좌표 -10은 상황에 맞춰 조절)
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }
}