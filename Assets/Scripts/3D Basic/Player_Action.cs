using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Action : MonoBehaviour
{
    [SerializeField]
    private GameObject Player;
    private Animator Animator;
    private float IdleTimer = 0f;
    private float IdleDelay = 5f;

    // Start is called before the first frame update
    void Start()
    {
        Animator = Player.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Shield();
        Attack();
        Idle();
        Sit();
        Jump();
    }
    
    void Attack()
    {
        if (Animator == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Animator.SetTrigger("IsAttacking");
        }
        /*else
        {
            Animator.SetTrigger("IsAttacking");
        }*/
    }

    void Shield()
    {
        if (Animator == null) return;

        if (Input.GetMouseButton(1))
        {
            Animator.SetBool("IsShield", true);
        }
        else
        {
            Animator.SetBool("IsShield", false);
        }
    }

    void Idle()
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle")) // Idle_SubSM에 태그 "Idle"을 붙여두자
        {
            IdleTimer += Time.deltaTime;
            if (IdleTimer >= IdleDelay)
            {
                int rand = Random.Range(1, 4); // 1~3
                Animator.SetInteger("RandomIdleIndex", rand);
                IdleTimer = 0;
            }
        }
        else
        {
            IdleTimer = 0;
            Animator.SetInteger("RandomIdleIndex", 0);
        }
    }

    void Sit()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Debug.Log("SitTrigger 발동");
            Animator.SetTrigger("SitTrigger");
            Animator.SetBool("IsSitting", true);
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            Debug.Log("IsSitting 해제");
            Animator.SetBool("IsSitting", false);
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Animator.SetTrigger("IsJump");
        }
    }
}
