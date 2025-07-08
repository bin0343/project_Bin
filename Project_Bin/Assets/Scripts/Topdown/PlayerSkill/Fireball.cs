using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 15;
    public float lifeTime = 2f;

    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            GreenSlimeControl enemy = other.GetComponent<GreenSlimeControl>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}