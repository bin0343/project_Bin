using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RpgCharacter1 : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector3 move;

    private Animator animator;
    //private SpriteRenderer sr;

    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        //sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        move = new Vector3(h, v, 0f).normalized;

        animator.SetFloat("Horizontal", h);
        animator.SetFloat("Vertical", v);
        animator.SetBool("IsMoving", move.magnitude > 0);

        if (h != 0)
        {
            //GetComponent<SpriteRenderer>().flipX = (h < 0);
            GetComponentInChildren<SpriteRenderer>().flipX = (h < 0);
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
