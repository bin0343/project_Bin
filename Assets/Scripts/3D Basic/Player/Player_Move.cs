using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Move : MonoBehaviour
{
    private Animator Animator;
    private Rigidbody Rigidbody;

    [SerializeField]
    private float CharacterSpeed = 2.0f; // 캐릭터 속도
    [SerializeField]
    public float CharacterRunSpeed = 9.0f; // 달리기 속도
    [SerializeField]
    public Transform CharacterBody; // 메인 캐릭터
    [SerializeField]
    private Transform CameraArm; // 메인 캐릭터의 카메라
    [SerializeField]
    private Transform CharacterRoot;    //캐릭터 상위오브젝트
    [SerializeField]
    private float RotateSpeed = 2.0f;

    private bool _IsRunning = false;
    public bool IsMoving = false;
    public bool IsRunning => _IsRunning;
    private Player_Action Action;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Animator = CharacterBody.GetComponentInChildren<Animator>();
        Action = GetComponent<Player_Action>();
    }

    void Update()
    {
        //LookAround();
    }

    private void FixedUpdate()
    {
        if (Action.IsDead) return;
        Move();
        Run();
        Rotate();
    }

    void Move()
    {
        if (Action.IsKick || Action.IsBuff)
            return;
        if (Action != null && Action.IsAttacking)
            return;

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool isMoving = moveInput.magnitude != 0;

        Animator.SetBool("IsMoving", isMoving);
        Animator.SetFloat("Horizontal", moveInput.x);
        Animator.SetFloat("Vertical", moveInput.y);

        if (!isMoving) return;

        float adjustedSpeed = GetAdjustedSpeed(moveInput);

        Vector3 lookForward = new Vector3(CameraArm.forward.x, 0f, CameraArm.forward.z).normalized;
        Vector3 lookRight = new Vector3(CameraArm.right.x, 0f, CameraArm.right.z).normalized;
        Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;

        CharacterBody.forward = lookForward;

        Rigidbody.MovePosition(transform.position + moveDir * Time.deltaTime * adjustedSpeed);
    }

    void Run()
    {
        if (!Action.IsGrounded) return;
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool IsMoving = moveInput.magnitude != 0;
        _IsRunning = Input.GetKey(KeyCode.LeftShift) && IsMoving;

        Animator.SetBool("IsRunning", _IsRunning);
    }

    float GetAdjustedSpeed(Vector2 moveInput)
    {
        float forwardWeight = 1.0f;
        float backwardWeight = 0.6f;
        float sideWeight = 0.8f;

        float directionWeight = 1.0f;

        if (Mathf.Abs(moveInput.x) > 0 && Mathf.Abs(moveInput.y) == 0)
        {
            directionWeight = sideWeight;
        }
        else if (moveInput.y < 0)
        {
            directionWeight = backwardWeight;
        }
        else if (moveInput.y > 0 && Mathf.Abs(moveInput.x) == 0)
        {
            directionWeight = forwardWeight;
        }
        else if (moveInput.x != 0 && moveInput.y != 0)
        {
            float vertical = moveInput.y > 0 ? forwardWeight : backwardWeight;
            directionWeight = (sideWeight + vertical) / 2f;
        }

        float baseSpeed = _IsRunning ? CharacterRunSpeed : CharacterSpeed;
        return baseSpeed * directionWeight;
    }

    void Rotate()
    {
        Vector3 lookDir = new Vector3(CameraArm.forward.x, 0f, CameraArm.forward.z).normalized;

        if (lookDir.sqrMagnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            CharacterBody.rotation = Quaternion.Slerp(CharacterBody.rotation, targetRotation, Time.deltaTime * RotateSpeed);
        }
    }
}
