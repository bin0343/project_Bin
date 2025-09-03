using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GreenSlimeControl : MonoBehaviour
{
    public float moveSpeed = 2f;
    protected Animator animator;
    protected Vector2 direction;
    protected bool IsDead = false;
    protected bool IsMoving = false;
    private bool IsKnockback = false;
    protected Rigidbody2D rb;

    public int contactDamage = 10;

    protected SpriteRenderer SpriteRenderer;

    public GameObject itemPrefab;
    public GameObject HpBar;
    private Slider HpSlider;
    private bool hpBarVisible = false;

    protected EnemyStat stat;
    [SerializeField] private int expReward = 20;
    [SerializeField] private int GoldReward = 20;


    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        stat = GetComponent<EnemyStat>();

        StartCoroutine(MoveRoutine());

        if (HpBar != null)
        {
            HpSlider = HpBar.GetComponentInChildren<Slider>();
            if (HpSlider != null)
            {
                HpSlider.maxValue = stat.MaxHp;
                HpSlider.value = stat.MaxHp;
            }
            HpBar.SetActive(false);
        }
    }

    protected virtual void Update()
    {
        if (IsKnockback) return;

        if (!IsDead && IsMoving)
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }

        animator.SetBool("IsMoving", IsMoving && !IsDead);
    }

    protected IEnumerator MoveRoutine()
    {
        while (true)
        {
            IsMoving = false;
            yield return new WaitForSeconds(Random.Range(1f, 2f));
            PickRandomDirection();
            IsMoving = true;
            yield return new WaitForSeconds(Random.Range(2f, 4f)); // 2~4초마다 방향 전환
        }
    }

    protected virtual void PickRandomDirection()
    {
        float x = Random.Range(-1f, 2f);
        float y = Random.Range(-1f, 2f);
        direction = new Vector2(x, y).normalized;
    }

    public virtual void TakeDamage(int attackerAttack, Vector2 hitSource)
    {
        if (IsDead) return;

        int damage = stat.TakeDamage(attackerAttack);

        FindObjectOfType<DamageTextSpawner_2D>().SpawnEnemyDamageText(transform.position + Vector3.up, damage);

        if (HpBar != null && HpSlider != null)
        {
            if (!hpBarVisible)
            {
                HpBar.SetActive(true);
                hpBarVisible = true;
            }
            HpSlider.maxValue = stat.MaxHp;
            HpSlider.value = stat.CurrentHp;
        }

        if (stat.IsDead())
        {
            Die();
        }
        else
        {
            StartCoroutine(KnockbackRoutine(hitSource));
            StartCoroutine(HitEffect());
        }
    }

    protected virtual void Die()
    {
        if (IsDead) return; 

        IsDead = true;
        IsMoving = false;

        StopAllCoroutines();
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        if (HpBar != null)
        {
            HpBar.SetActive(false);
        }

        DropItem();

        animator.SetTrigger("IsDead");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerStat stat = player.GetComponent<PlayerStat>();
            if (stat != null)
            {
                stat.GainExp(expReward);
                stat.GainGold(GoldReward);
            }
        }
    }

    void DropItem()
    {
        float dropChance = 1f;
        if (Random.value < dropChance)
        {
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }
    }

    public void OnDeathAnimationEnd()
    {
        Destroy(gameObject);
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
    }

    protected IEnumerator HitEffect()
    {
        for (int i = 0; i < 3; i++)
        {
            SpriteRenderer.color = new Color(1, 1, 1, 0.3f); // 반투명
            yield return new WaitForSeconds(0.1f);
            SpriteRenderer.color = new Color(1, 1, 1, 1f); // 원래대로
            yield return new WaitForSeconds(0.1f);
        }
    }

    protected IEnumerator KnockbackRoutine(Vector2 hitSource)
    {
        IsKnockback = true;

        Vector2 KnockDir = (rb.position - hitSource).normalized;
        float knockDuration = 0.2f;
        float knockSpeed = 20f;
        float timer = 0f;

        while (timer < knockDuration)
        {
            rb.MovePosition(rb.position + KnockDir * knockSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        IsKnockback = false;
    }
}
