using UnityEngine;

public class RunGame_Player : MonoBehaviour
{
    public float jumpForce = 5f;
    private Rigidbody2D rb;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 게임 오버 상태면 조작 불가
        if (RunGame_Manager.instance.isGameOver) return;

        // 스페이스바 누르면 점프
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = Vector2.zero; // 점프 전 속도 초기화 (이중 점프 방지 느낌)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 장애물에 닿으면 게임 오버
        if (collision.CompareTag("Obstacle"))
        {
            RunGame_Manager.instance.GameOver();
            Destroy(gameObject); // 플레이어 사라짐
        }
    }
}