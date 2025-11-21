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

    [SerializeField] public float CharacterRunSpeed = 9.0f;
    [SerializeField] public Transform CharacterBody; 
    [SerializeField] private Transform CameraArm; 
    [SerializeField] private float RotateSpeed = 5.0f;

    private Transform currentReference;

    [SerializeField] private LayerMask groundLayer;

    private Coroutine rotationCoroutine;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Animator = CharacterBody.GetComponentInChildren<Animator>();
        //Action = GetComponent<Player_Action>();
        cameraArmScript = CameraArm.GetComponent<CameraArm>();
        currentReference = CameraArm;

        if (groundLayer == 0) groundLayer = -1;
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
        /*if (moveInput.magnitude == 0) return;

        Vector3 lookForward;
        Vector3 lookRight;

        lookForward = new Vector3(CameraArm.forward.x, 0f, CameraArm.forward.z).normalized;
        lookRight = new Vector3(CameraArm.right.x, 0f, CameraArm.right.z).normalized;

        Vector3 moveDir = (lookForward * moveInput.y + lookRight * moveInput.x).normalized;

        Rigidbody.MovePosition(transform.position + moveDir * Time.deltaTime * speed);

        // 3인칭일 때, moveDir (실제 움직이는 방향)을 바라보도록 회전
        if (moveDir.sqrMagnitude > 0f) // sqrMagnitude는 0보다 클 때만 (즉, 움직임이 있을 때만)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            CharacterBody.rotation = Quaternion.Slerp(CharacterBody.rotation, targetRotation, Time.deltaTime * RotateSpeed);
        }*/

        if (moveInput.magnitude == 0) return;

        // [!] 수정: CameraArm 대신 currentReference를 사용
        // 만약 currentReference가 없으면 기본값으로 CameraArm 사용
        Transform refTransform = currentReference != null ? currentReference : CameraArm;

        Vector3 lookForward = new Vector3(refTransform.forward.x, 0f, refTransform.forward.z).normalized;
        Vector3 lookRight = new Vector3(refTransform.right.x, 0f, refTransform.right.z).normalized;

        Vector3 moveDir = (lookForward * moveInput.y + lookRight * moveInput.x).normalized;

        Rigidbody.MovePosition(transform.position + moveDir * Time.deltaTime * speed);

        // 회전 로직 (1인칭 아닐 때만)
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

    public void SetReferenceTransform(Transform newReference)
    {
        currentReference = newReference;
        Debug.Log($"이동 기준이 {newReference.name}로 변경되었습니다.");
    }

    public void LookAtMouse()
    {
        /*Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            Vector3 targetPoint = hit.point;
            Vector3 lookPoint = new Vector3(targetPoint.x, transform.position.y, targetPoint.z);

            CharacterBody.LookAt(lookPoint);
        }
        else
        {
            Plane groundPlane = new Plane(Vector3.up, transform.position);
            float enter;
            if (groundPlane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 lookPoint = new Vector3(hitPoint.x, transform.position.y, hitPoint.z);
                CharacterBody.LookAt(lookPoint);
            }
        }*/

        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        rotationCoroutine = StartCoroutine(RotateToMouseCoroutine());
    }

    private IEnumerator RotateToMouseCoroutine()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;
        bool hasHit = false;

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            targetPoint = hit.point;
            hasHit = true;
        }
        else
        {
            Plane groundPlane = new Plane(Vector3.up, transform.position);
            float enter;
            if (groundPlane.Raycast(ray, out enter))
            {
                targetPoint = ray.GetPoint(enter);
                hasHit = true;
            }
        }

        if (hasHit)
        {
            // 2. 목표 회전값 계산
            Vector3 direction = (targetPoint - transform.position).normalized;
            direction.y = 0; // 수직 회전 방지

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // 3. 회전 루프 (목표 각도와 거의 비슷해질 때까지)
                // 20.0f는 매우 빠른 회전 속도입니다. (일반 이동 회전보다 훨씬 빠름)
                float attackRotateSpeed = 20.0f;

                while (Quaternion.Angle(CharacterBody.rotation, targetRotation) > 1.0f)
                {
                    CharacterBody.rotation = Quaternion.Slerp(
                        CharacterBody.rotation,
                        targetRotation,
                        Time.deltaTime * attackRotateSpeed
                    );
                    yield return null; // 다음 프레임까지 대기
                }

                // 4. 루프가 끝나면 깔끔하게 목표 각도로 확정
                CharacterBody.rotation = targetRotation;
            }
        }

        rotationCoroutine = null;
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
