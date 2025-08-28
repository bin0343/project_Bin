using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Move : MonoBehaviour
{
    private Animator Animator;
    private Rigidbody Rigidbody;
    private Player_Action Action;
    private CameraArm cameraArmScript;

    [SerializeField] private float CharacterSpeed = 2.0f;
    [SerializeField] public float CharacterRunSpeed = 9.0f;
    [SerializeField] public Transform CharacterBody; // 인스펙터: Player 모델 오브젝트 연결
    [SerializeField] private Transform CameraArm;     // 인스펙터: CameraArm 피봇 오브젝트 연결
    [SerializeField] private float RotateSpeed = 2.0f;

    private bool _IsRunning = false;
    public bool IsRunning => _IsRunning;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Animator = CharacterBody.GetComponentInChildren<Animator>();
        Action = GetComponent<Player_Action>();
        cameraArmScript = CameraArm.GetComponent<CameraArm>();
    }

    void Update()
    {
        // 1인칭일 때 Character 루트 오브젝트 전체를 회전시킵니다.
        if (UI_Manager.Instance != null && UI_Manager.Instance.IsUIOpen)
            return;
        Look();
    }

    private void FixedUpdate()
    {
        if (Action != null && Action.IsDead) return;
        if (UI_Manager.Instance != null && UI_Manager.Instance.IsUIOpen)
            return;
        Move();
        Run();
        Rotate(); // 3인칭 전용 회전 처리
    }

    void Look()
    {
        if (cameraArmScript != null && cameraArmScript.FirstPersonCamera.enabled)
        {
            float mouseX = Input.GetAxis("Mouse X");
            // 이 스크립트가 붙어있는 'Character' 루트 오브젝트를 회전
            transform.Rotate(Vector3.up * mouseX);
        }
    }

    void Move()
    {
        if (Action != null && (Action.IsKick || Action.IsBuff || Action.IsAttacking)) return;

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool isMoving = moveInput.magnitude != 0;

        Animator.SetBool("IsMoving", isMoving);
        Animator.SetFloat("Horizontal", moveInput.x);
        Animator.SetFloat("Vertical", moveInput.y);

        if (!isMoving) return;

        Vector3 lookForward;
        Vector3 lookRight;

        if (cameraArmScript != null && cameraArmScript.FirstPersonCamera.enabled)
        {
            // 1인칭: 회전하는 'Character' 루트의 방향을 기준으로 이동
            lookForward = transform.forward;
            lookRight = transform.right;
        }
        else
        {
            // 3인칭: 카메라가 보는 방향을 기준으로 이동
            lookForward = new Vector3(CameraArm.forward.x, 0f, CameraArm.forward.z).normalized;
            lookRight = new Vector3(CameraArm.right.x, 0f, CameraArm.right.z).normalized;
            CharacterBody.forward = lookForward;
        }

        float adjustedSpeed = GetAdjustedSpeed(moveInput);
        Vector3 moveDir = (lookForward * moveInput.y + lookRight * moveInput.x).normalized;

        Rigidbody.MovePosition(transform.position + moveDir * Time.deltaTime * adjustedSpeed);
    }

    void Run()
    {
        if (Action != null && !Action.IsGrounded) return;
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _IsRunning = Input.GetKey(KeyCode.LeftShift) && moveInput.magnitude != 0;
        Animator.SetBool("IsRunning", _IsRunning);
    }

    float GetAdjustedSpeed(Vector2 moveInput)
    {
        float forwardWeight = 1.0f;
        float backwardWeight = 0.6f;
        float sideWeight = 0.8f;
        float directionWeight = 1.0f;

        if (Mathf.Abs(moveInput.x) > 0 && Mathf.Abs(moveInput.y) == 0) directionWeight = sideWeight;
        else if (moveInput.y < 0) directionWeight = backwardWeight;
        else if (moveInput.y > 0 && Mathf.Abs(moveInput.x) == 0) directionWeight = forwardWeight;
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
        if (cameraArmScript == null || cameraArmScript.FirstPersonCamera.enabled)
        {
            return; // 1인칭일 때는 이 함수를 실행하지 않음
        }

        // 3인칭일 때만 카메라 방향으로 캐릭터를 부드럽게 회전
        Vector3 lookDir = new Vector3(CameraArm.forward.x, 0f, CameraArm.forward.z).normalized;
        if (lookDir.sqrMagnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            CharacterBody.rotation = Quaternion.Slerp(CharacterBody.rotation, targetRotation, Time.deltaTime * RotateSpeed);
        }
    }
}
