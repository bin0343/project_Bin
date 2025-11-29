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

    [SerializeField] public float CharacterRunSpeed = 12.0f;
    [SerializeField] public Transform CharacterBody; 
    [SerializeField] private Transform CameraArm; 
    [SerializeField] private float RotateSpeed = 7.0f;

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
        /*if (UI_Manager.Instance != null && UI_Manager.Instance.IsUIOpen)
            return;*/
        Look();
    }

    private void FixedUpdate()
    {
        
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

        // [수정] newReference가 null일 경우 .name을 호출하면 에러가 나므로 예외 처리
        if (newReference != null)
        {
            Debug.Log($"이동 기준이 {newReference.name}로 변경되었습니다.");
        }
        else
        {
            // null이 들어오면 "기본값"으로 변경되었다고 로그 출력
            Debug.Log("이동 기준이 기본값(CameraArm)으로 변경되었습니다.");
        }
    }

    public void LookAtMouse()
    {
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

    public void SetDailyCameraActive(bool isActive)
    {
        if (CameraArm != null)
        {
            CameraArm.gameObject.SetActive(isActive);
        }
    }

    public void HandleRotation()
    {
        
    }
}
