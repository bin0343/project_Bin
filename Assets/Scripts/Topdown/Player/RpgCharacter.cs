using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RpgCharacter : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 move;

    private Animator animator;

    private bool isAttacking = false;

    private string movePriority = "";

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

        if (h != 0)
        {
            spriteRenderer.flipX = (h < 0);
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
            
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        animator.SetBool("IsAttacking", true);

        yield return new WaitForSeconds(0.3f);

        animator.SetBool("IsAttacking", false);
        isAttacking = false;
    }
}
