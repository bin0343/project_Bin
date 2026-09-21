using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerVehicleController : MonoBehaviour
{
    [Header("속도")]
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float maxForwardSpeed = 15f;
    [SerializeField] private float maxReverseSpeed = 6f;

    [Header("조향")]
    [SerializeField] private float steerSpeed = 80f;

    [Header("주행 감각")]
    [Tooltip("측면 미끄러짐을 얼마나 빨리 잡을지")]
    [SerializeField] private float lateralGrip = 8f;
    [Tooltip("악셀을 떼었을 때 자연스럽게 줄어드는 속도")]
    [SerializeField] private float coastDeceleration = 0.6f;
    [Tooltip("이 속도 이하에서는 조향하지 않음")]
    [SerializeField] private float minimumSteerSpeed = 0.5f;


    private Rigidbody rb;

    private float moveInput;
    private float steerInput;

    private bool isDriving;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        isDriving = false;
    }


    private void Update()
    {
        if (!isDriving)
        {
            moveInput = 0f;
            steerInput = 0f;
            return;
        }

        moveInput = Input.GetAxisRaw("Vertical");

        steerInput = Input.GetAxisRaw("Horizontal");
    }


    private void FixedUpdate()
    {
        if (!isDriving) return;

        HandleMovement();
        HandleSteering();
        HandleLateralGrip();
    }


    public void SetDriving(bool driving)
    {
        isDriving = driving;

        if (!driving)
        {
            moveInput = 0f;
            steerInput = 0f;
        }
    }


    private void HandleMovement()
    {
        float forwardSpeed = Vector3.Dot(rb.velocity, transform.forward);

        if (moveInput > 0f)
        {
            if (forwardSpeed < maxForwardSpeed)
            {
                rb.AddForce(transform.forward * moveInput * acceleration, ForceMode.Acceleration);
            }
        }
        else if (moveInput < 0f)
        {
            if (forwardSpeed > -maxReverseSpeed)
            {
                rb.AddForce(transform.forward * moveInput * acceleration, ForceMode.Acceleration);
            }
        }
        else
        {
            ApplyCoasting(forwardSpeed);
        }
    }

    private void HandleSteering()
    {
        if (Mathf.Abs(steerInput) < 0.01f) return;

        float forwardSpeed = Vector3.Dot(rb.velocity, transform.forward);

        if (Mathf.Abs(forwardSpeed) < minimumSteerSpeed) return;

        float drivingDirection = Mathf.Sign(forwardSpeed);

        Quaternion turnRotation = Quaternion.Euler(0f, steerInput * steerSpeed * drivingDirection * Time.fixedDeltaTime, 0f);

        rb.MoveRotation(rb.rotation * turnRotation);
    }

    private void HandleLateralGrip()
    {
        Vector3 velocity = rb.velocity;

        float verticalSpeed = velocity.y;

        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

        float horizontalSpeed = horizontalVelocity.magnitude;

        if (horizontalSpeed < 0.01f) return;

        float directionSign = Vector3.Dot(horizontalVelocity, transform.forward) >= 0f ? 1f : -1f;

        Vector3 targetDirection = transform.forward * directionSign;

        float gripAmount = Mathf.Clamp01(lateralGrip * Time.fixedDeltaTime);

        Vector3 correctedDirection = Vector3.Slerp(horizontalVelocity.normalized, targetDirection, gripAmount).normalized;

        Vector3 correctedHorizontalVelocity = correctedDirection * horizontalSpeed;

        rb.velocity = correctedHorizontalVelocity + Vector3.up * verticalSpeed;
    }

    private void ApplyCoasting(float forwardSpeed)
    {
        if (Mathf.Abs(forwardSpeed) < 0.01f) return;

        float direction = Mathf.Sign(forwardSpeed);

        float maxDecelerationThisStep = Mathf.Abs(forwardSpeed) / Time.fixedDeltaTime;

        float deceleration = Mathf.Min(coastDeceleration, maxDecelerationThisStep);

        rb.AddForce(-transform.forward * direction * deceleration, ForceMode.Acceleration);
    }
}