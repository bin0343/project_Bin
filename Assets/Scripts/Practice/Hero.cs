using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    public GameObject hero;
    public Animator animator;
    public GameObject visual;
    public bool isGround = false;

    float movespeed = 0.01f;
    float jumpspeed = 5f;
    //public LayerMask groundLayer;
    //public Transform groundCheck;

    public Rigidbody2D rb;
    public bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

        //공격1
        if (Input.GetKeyDown(KeyCode.A))
        {
            animator.SetTrigger("Attack1");
        }
        //공격2
        if (Input.GetKeyDown(KeyCode.S))
        {
            animator.SetTrigger("Attack2");
        }

        //이동
        if (Input.GetKey(KeyCode.RightArrow))
        {
            animator.SetBool("Run", true);
            visual.transform.localScale = new Vector3(1f, 1f, 1f);
            hero.transform.position += new Vector3(movespeed, 0f, 0f);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            animator.SetBool("Run", true);
            visual.transform.localScale = new Vector3(-1f, 1f, 1f);
            hero.transform.position -= new Vector3(movespeed, 0f, 0f);
        }
        else
        {
            animator.SetBool("Run", false);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && isGround)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpspeed);
            isGround = false;
            animator.SetTrigger("Jump");
        }
        else
        {
            animator.SetBool("IsJumping", !isGround);
        }

        



        //뛰기
        /*if (Input.GetKey(KeyCode.LeftControl))
        {
            animator.SetBool("Run", true);
        }
        else
        {
            animator.SetBool("Run", false);
        }*/
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGround = true;
            animator.SetBool("IsJumping", false);
        }
    }
}
