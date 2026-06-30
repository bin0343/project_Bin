using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Move : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public Rigidbody Rigidbody { get; private set; }

    [SerializeField] public float CharacterRunSpeed = 12.0f;
    [SerializeField] public Transform CharacterBody;
    [SerializeField] private float RotateSpeed = 7.0f;
    [SerializeField] private LayerMask groundLayer;

    [Header("계단/턱 보정 (Step Climbing)")]
    [SerializeField] private float stepHeight = 0.4f;   // 오를 수 있는 턱의 최대 높이 (무릎 정도 높이)
    [SerializeField] private float stepSmooth = 15.0f;  // 턱을 오를 때의 부드러움

    private Coroutine rotationCoroutine;
    private Vector3 targetMoveVelocity = Vector3.zero;
    private bool isMovementCalledThisFrame = false;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Animator = CharacterBody.GetComponentInChildren<Animator>();
        if (groundLayer == 0) groundLayer = -1;
    }

    void Update() { }

    private void LateUpdate()
    {
        if (!isMovementCalledThisFrame) targetMoveVelocity = Vector3.zero;
        isMovementCalledThisFrame = false;
    }

    private void FixedUpdate()
    {
        Rigidbody.velocity = new Vector3(targetMoveVelocity.x, Rigidbody.velocity.y, targetMoveVelocity.z);

        if (targetMoveVelocity.sqrMagnitude > 0)
        {
            bool isClimbing = HandleStepClimb();

            // 2. 올라가는 중이 아니라면, 계단을 내려가는 중인지 체크하여 보정합니다.
            if (!isClimbing)
            {
                HandleStepDescend();
            }
        }
    }

    private bool HandleStepClimb()
    {
        Vector3 moveDir = new Vector3(targetMoveVelocity.x, 0, targetMoveVelocity.z).normalized;

        float rayLength = 0.5f;
        Vector3 footPos = Rigidbody.position + Vector3.up * 0.05f;

        if (Physics.Raycast(footPos, moveDir, out RaycastHit hitLower, rayLength, groundLayer))
        {
            float angle = Vector3.Angle(Vector3.up, hitLower.normal);
            if (angle > 70f)
            {
                Vector3 kneePos = Rigidbody.position + Vector3.up * stepHeight;
                if (!Physics.Raycast(kneePos, moveDir, rayLength + 0.1f, groundLayer))
                {
                    Vector3 checkPos = kneePos + moveDir * rayLength;
                    if (Physics.Raycast(checkPos, Vector3.down, out RaycastHit hitUpper, stepHeight, groundLayer))
                    {
                        Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, 0f, Rigidbody.velocity.z);

                        Vector3 targetPos = Rigidbody.position;
                        targetPos.y = hitUpper.point.y;

                        Rigidbody.MovePosition(Vector3.Lerp(Rigidbody.position, targetPos, Time.fixedDeltaTime * stepSmooth));
                        return true; // 올라가기 보정 작동함
                    }
                }
            }
        }
        return false; // 작동하지 않음
    }

    private void HandleStepDescend()
    {
        if (Rigidbody.velocity.y > 0.1f) return;

        float rayLength = stepHeight + 0.1f;
        Vector3 footPos = Rigidbody.position + Vector3.up * 0.05f;

        if (Physics.Raycast(footPos, Vector3.down, out RaycastHit hit, rayLength, groundLayer))
        {
            float groundDist = Rigidbody.position.y - hit.point.y;

            if (groundDist > 0.05f && groundDist <= stepHeight)
            {
                Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, 0f, Rigidbody.velocity.z);

                Vector3 targetPos = Rigidbody.position;
                targetPos.y = hit.point.y;

                Rigidbody.MovePosition(targetPos);
                Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, 0f, Rigidbody.velocity.z);

                Player_Action playerAction = GetComponentInParent<Player_Action>();
                if (playerAction != null)
                {
                    playerAction.IsGrounded = true;
                }
            }
        }
    }

    public void HandleMovement(Vector2 moveInput, float speed)
    {
        isMovementCalledThisFrame = true;

        if (moveInput.magnitude == 0)
        {
            targetMoveVelocity = Vector3.zero;
            return;
        }

        Transform refTransform = Camera.main.transform;

        Vector3 lookForward = new Vector3(refTransform.forward.x, 0f, refTransform.forward.z).normalized;
        Vector3 lookRight = new Vector3(refTransform.right.x, 0f, refTransform.right.z).normalized;

        Vector3 moveDir = (lookForward * moveInput.y + lookRight * moveInput.x).normalized;

        targetMoveVelocity = moveDir * speed;

        if (!GetComponentInParent<Player_Action>().CanRotate) return;

        if (moveDir.sqrMagnitude > 0f)
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

        return CharacterRunSpeed * directionWeight;
    }

    public void ForceMove(Vector3 direction, float speed)
    {
        isMovementCalledThisFrame = true;
        targetMoveVelocity = direction * speed;
    }

    public void AlignToCameraForward()
    {
        Transform camTransform = Camera.main.transform;
        Vector3 camForward = camTransform.forward;
        camForward.y = 0;

        if (camForward.sqrMagnitude > 0)
        {
            camForward.Normalize();
            CharacterBody.rotation = Quaternion.LookRotation(camForward);
        }
    }

    public void LookAtMouse()
    {
        if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
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
            Vector3 direction = (targetPoint - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                float attackRotateSpeed = 20.0f;

                while (Quaternion.Angle(CharacterBody.rotation, targetRotation) > 1.0f)
                {
                    CharacterBody.rotation = Quaternion.Slerp(CharacterBody.rotation, targetRotation, Time.deltaTime * attackRotateSpeed);
                    yield return null;
                }

                CharacterBody.rotation = targetRotation;
            }
        }
        rotationCoroutine = null;
    }

    public void HandleRotation() { }
}