using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    public GameObject attackHitbox;
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 move;
    private Animator animator;
    private static GameObject instance;

    private bool IsDead = false;
    private bool IsAttacking = false;
    public bool Isinvincible = false;
    private bool IsKnockback = false;

    private string movePriority = "";
    private SpriteRenderer spriteRenderer;
    
    private int currentGold;

    public int fireballCost = 10;

    public float InvincibleDuration = 1f;

    public GameObject HpBar;
    private Slider HpSlider;
    public GameObject MpBar;
    public Slider MpSlider;
    public Slider ExpSlider;

    //스킬
    public GameObject fireballPrefab;
    public Transform firePoint; // 발사 위치
    public float fireballCooldown = 1.5f;
    private float lastFireTime = -999f;
    private Vector2 lastMoveDir = Vector2.right;
    public SkillUIController fireballUI;

    private PlayerStat Stat;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Stat = GetComponent<PlayerStat>();
        attackHitbox.SetActive(false);
        //Shared.SceneMANAGER.Init(Stat);

        if (HpBar != null)
        {
            HpSlider = HpBar.GetComponentInChildren<Slider>();
            if (HpSlider != null)
            {
                HpSlider.maxValue = Stat.MaxHp;
                HpSlider.value = Stat.currentHp;
            }
        }

        if (MpBar != null)
        {
            if (MpSlider != null)
            {
                MpSlider.maxValue = Stat.MaxMp;
                MpSlider.value = Stat.currentMp;
            }
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = gameObject;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // 이미 있는 경우 새로 생긴 건 파괴
        }
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        move = new Vector2(h, v).normalized;


        if (h != 0 && v != 0)
        {
            if (movePriority == "")
            {
                movePriority = Mathf.Abs(h) > Mathf.Abs(v) ? "Horizontal" : "Vertical";
            }
        }
        else if (h != 0)
        {
            movePriority = "Horizontal";
        }
        else if (v != 0)
        {
            movePriority = "Vertical";
        }
        else
        {
            movePriority = "";
        }

        if (movePriority == "Horizontal")
        {
            animator.SetFloat("Horizontal", h);
            animator.SetFloat("Vertical", 0);
        }
        else if (movePriority == "Vertical")
        {
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", v);
        }
        else
        {
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", 0);
        }

        animator.SetBool("IsMoving", move.magnitude > 0);

        if (!IsDead && h != 0)
        {
            spriteRenderer.flipX = (h < 0);
        }

        if (Input.GetKeyDown(KeyCode.Space) && !IsAttacking)
        {
            StartCoroutine(AttackRoutine());
        }

        if (move != Vector2.zero)
        {
            lastMoveDir = move;

            Vector3 offset = new Vector3(lastMoveDir.x, lastMoveDir.y, 0).normalized * 0.5f;
            firePoint.localPosition = offset;

            float angle = Mathf.Atan2(lastMoveDir.y, lastMoveDir.x) * Mathf.Rad2Deg;
            firePoint.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (Input.GetKeyDown(KeyCode.F) && Time.time >= lastFireTime + fireballCooldown)
        {
            UseFireball();
        }
        GainExp();
    }

    public void GainExp()
    {
        ExpSlider.maxValue = Stat.LevelUpExp;
        ExpSlider.value = Stat.Exp;
    }

    public void TakeDamage(int damage, Vector2 hitSource)
    {
        if (IsDead || Isinvincible) return;

        int Damage = Stat.TakeDamage(damage);
        FindObjectOfType<DamageTextSpawner>().SpawnPlayerDamageText(transform.position + Vector3.up, Damage);

        if (HpBar != null && HpSlider != null)
        {
            HpSlider.value = Stat.currentHp;
        }

        if (Stat.currentHp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }

        if (!IsDead)
        {
            StartCoroutine(KnockbackRoutine(hitSource));
            StartCoroutine(HitEffect());
        }
    }

    public void Heal(int amount)
    {
        if (IsDead) return;

        Stat.Heal(amount);

        if (HpSlider != null)
        {
            HpSlider.value = Stat.currentHp;
        }

        Debug.Log($"회복됨! 현재 체력 : {Stat.currentHp}");
    }

    public void RecoverMp(int amount)
    {
        Stat.RecoverMp(amount);

        if (MpSlider != null)
        {
            MpSlider.value = Stat.currentMp;
        }

        Debug.Log($"MP 회복! 현재 MP : {Stat.currentMp}");
    }

    void UseFireball()
    {
        if (Stat.currentMp < fireballCost)
        {
            Debug.Log("MP가 부족합니다!");
            return;
        }

        Stat.currentMp -= fireballCost;
        MpSlider.value = Stat.currentMp;

        lastFireTime = Time.time;

        Vector2 dir = lastMoveDir;
        if (dir == Vector2.zero) dir = Vector2.right; // 정지 상태일 경우 오른쪽

        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        fireball.GetComponent<Fireball>().SetDirection(dir);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        fireball.transform.rotation = Quaternion.Euler(0, 0, angle);

        fireballUI.SetCooldown(fireballCooldown);
    }

    void FixedUpdate()
    {
        if (!IsDead && !IsKnockback)
        {
            rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);
        }
    }

    IEnumerator AttackRoutine()
    {
        IsAttacking = true;
        animator.SetBool("IsAttacking", true);

        attackHitbox.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        attackHitbox.SetActive(false);
        animator.SetBool("IsAttacking", false);
        IsAttacking = false;
    }

    IEnumerator InvincibilityRoutine()
    {
        Isinvincible = true;
        // 피격시 이펙트나 깜빡임 처리 가능
        yield return new WaitForSeconds(InvincibleDuration);
        Isinvincible = false;
    }

    void Die()
    {
        Debug.Log("플레이어 사망!");
        StopAllCoroutines();
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        if (HpBar != null)
        {
            HpBar.SetActive(false);
        }

        IsDead = true;
        animator.SetTrigger("IsDead");
    }

    private IEnumerator HitEffect()
    {
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.3f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator KnockbackRoutine(Vector2 hitSource)
    {
        IsKnockback = true;

        Vector2 knockDir = ((Vector2)transform.position - hitSource).normalized;
        float knockDuration = 0.2f;
        float knockSpeed = 20f;
        float timer = 0f;

        while (timer < knockDuration)
        {
            rb.MovePosition(rb.position + knockDir * knockSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        IsKnockback = false;
    }
}
