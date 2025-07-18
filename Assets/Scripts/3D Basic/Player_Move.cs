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
    private float CharacterSpeed = 5.0f; // 캐릭터 속도
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
        Animator.SetBool("IsMoving", IsMoving);

        if (IsMoving)
        {
            Vector3 lookForward = new Vector3(CameraArm.forward.x, 0f, CameraArm.forward.z).normalized;
            Vector3 lookRight = new Vector3(CameraArm.right.x, 0f, CameraArm.right.z).normalized;
            Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;

            CharacterBody.forward = lookForward; // 이동할 때 이동방향 바라보게 세팅
            transform.position += moveDir * Time.deltaTime * CharacterSpeed; // 이동
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
