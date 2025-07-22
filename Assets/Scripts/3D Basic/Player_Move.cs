using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Move : MonoBehaviour
{
    private Animator Animator;
    private Rigidbody Rigidbody;
    //private Vector2 MouseInput;
    //private float CameraPitch = 0f;

    [SerializeField]
    private float CharacterSpeed = 2.0f; // 캐릭터 속도
    [SerializeField]
    private float CharacterRunSpeed = 9.0f; // 달리기 속도
    [SerializeField]
    private Transform CharacterBody; // 메인 캐릭터
    [SerializeField]
    private Transform CameraArm; // 메인 캐릭터의 카메라
    [SerializeField]
    private Transform CharacterRoot;    //캐릭터 상위오브젝트
    [SerializeField]
    private float RotateSpeed = 2.0f;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Animator = CharacterBody.GetComponentInChildren<Animator>();
    }

    void Update()
    {
        //LookAround();
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
    }

    void Move()
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool IsMoving = moveInput.magnitude != 0;
        bool IsRunning = Input.GetKey(KeyCode.LeftAlt) && IsMoving;
        Animator.SetBool("IsRunning", IsRunning);
        Animator.SetBool("IsMoving", IsMoving);

        float currentSpeed = IsRunning ? CharacterRunSpeed : CharacterSpeed;

        float forwardWeight = 1.0f;
        float backwardWeight = 0.6f;
        float sideWeight = 0.8f;

        float directionSpeedWeight = 1.0f;

        if (Mathf.Abs(moveInput.x) > 0 && Mathf.Abs(moveInput.y) == 0)
        {
            // 좌우 이동
            directionSpeedWeight = sideWeight;
        }
        else if (moveInput.y < 0)
        {
            // 뒤로 이동
            directionSpeedWeight = backwardWeight;
        }
        else if (moveInput.y > 0 && Mathf.Abs(moveInput.x) == 0)
        {
            // 정면 이동
            directionSpeedWeight = forwardWeight;
        }
        else if (moveInput.x != 0 && moveInput.y != 0)
        {
            // 대각선이면 평균값 또는 보수적 가중치 사용
            float vertical = moveInput.y > 0 ? forwardWeight : backwardWeight;
            directionSpeedWeight = (sideWeight + vertical) / 2f;
        }

        float adjustedSpeed = currentSpeed * directionSpeedWeight;

        Animator.SetFloat("Horizontal", moveInput.x);
        Animator.SetFloat("Vertical", moveInput.y);

        if (IsMoving)
        {
            Vector3 lookForward = new Vector3(CameraArm.forward.x, 0f, CameraArm.forward.z).normalized;
            Vector3 lookRight = new Vector3(CameraArm.right.x, 0f, CameraArm.right.z).normalized;
            Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;

            CharacterBody.forward = lookForward; // 이동할 때 이동방향 바라보게 세팅
            transform.position += moveDir * Time.deltaTime * adjustedSpeed; // 이동
        }
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
