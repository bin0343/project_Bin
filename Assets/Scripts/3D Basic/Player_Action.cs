using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Action : MonoBehaviour
{
    [SerializeField]
    private GameObject Player;
    private Animator Animator;

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
}
