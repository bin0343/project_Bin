using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Move : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    private CameraArm cameraArmScript;

    //[SerializeField] private float CharacterSpeed = 2.0f;
    [SerializeField] public float CharacterRunSpeed = 9.0f;
    [SerializeField] public Transform CharacterBody; 
    [SerializeField] private Transform CameraArm; 
    [SerializeField] private float RotateSpeed = 2.0f;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Animator = CharacterBody.GetComponentInChildren<Animator>();
        //Action = GetComponent<Player_Action>();
        cameraArmScript = CameraArm.GetComponent<CameraArm>();

    }

    void Update()
    {
        if (UI_Manager.Instance != null && UI_Manager.Instance.IsUIOpen)
            return;
        Look();
    }

    private void FixedUpdate()
    {
        /*IPlayerState actionState = Action.currentState; // 가독성을 위해 현재 Action 상태를 가져옴

        // Action의 현재 상태가 공격 관련 상태이거나 죽었다면 이동 로직을 실행하지 않음
        if (actionState is PlayerAttackState || actionState is PlayerRunningAttackState || actionState is PlayerDeadState)
        {
            return;
        }
        if (UI_Manager.Instance != null && UI_Manager.Instance.IsUIOpen)
            return;
        //Move();
        //Run();
        //Rotate(); // 3인칭 전용 회전 처리
        currentMoveState?.Execute(this);*/
    }

    void Look()
    {
        /*if (cameraArmScript != null && cameraArmScript.FirstPersonCamera.enabled)
        {
            float mouseX = Input.GetAxis("Mouse X");
            // 이 스크립트가 붙어있는 'Character' 루트 오브젝트를 회전
            transform.Rotate(Vector3.up * mouseX);
        }*/
    }

    public void HandleMovement(Vector2 moveInput, float speed)
    {
        if (moveInput.magnitude == 0) return;

        Vector3 lookForward;
        Vector3 lookRight;

        /*if (cameraArmScript != null && cameraArmScript.FirstPersonCamera.enabled)
        {
            lookForward = transform.forward;
            lookRight = transform.right;
        }
        else
        {
            
            //CharacterBody.forward = lookForward;
        }*/

        lookForward = new Vector3(CameraArm.forward.x, 0f, CameraArm.forward.z).normalized;
        lookRight = new Vector3(CameraArm.right.x, 0f, CameraArm.right.z).normalized;

        //float adjustedSpeed = GetAdjustedSpeed(moveInput);
        Vector3 moveDir = (lookForward * moveInput.y + lookRight * moveInput.x).normalized;

        Rigidbody.MovePosition(transform.position + moveDir * Time.deltaTime * speed);

        /*if (cameraArmScript != null && cameraArmScript.FirstPersonCamera.enabled)
        {
            return;
        }*/

        // 3인칭일 때, moveDir (실제 움직이는 방향)을 바라보도록 회전
        if (moveDir.sqrMagnitude > 0f) // sqrMagnitude는 0보다 클 때만 (즉, 움직임이 있을 때만)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            CharacterBody.rotation = Quaternion.Slerp(CharacterBody.rotation, targetRotation, Time.deltaTime * RotateSpeed);
        }
    }

    public float GetAdjustedSpeed(Vector2 moveInput)
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

        float baseSpeed = CharacterRunSpeed;
        return baseSpeed * directionWeight;
    }

    public void HandleRotation()
    {
        /*if (cameraArmScript == null || cameraArmScript.FirstPersonCamera.enabled)
        {
            return; // 1인칭일 때는 이 함수를 실행하지 않음
        }

        Vector3 lookDir = new Vector3(CameraArm.forward.x, 0f, CameraArm.forward.z).normalized;
        if (lookDir.sqrMagnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            CharacterBody.rotation = Quaternion.Slerp(CharacterBody.rotation, targetRotation, Time.deltaTime * RotateSpeed);
        }*/
    }
}
