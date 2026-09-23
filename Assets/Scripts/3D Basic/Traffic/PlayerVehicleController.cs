using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerVehicleController : MonoBehaviour
{
    [Header("속도")]
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float maxForwardSpeed = 15f;
    [SerializeField] private float maxReverseSpeed = 6f;

    [Header("조향")]
    [Tooltip("저속에서의 최대 조향 속도")]
    [SerializeField] private float lowSpeedSteerSpeed = 80f;
    [Tooltip("최고속도에서의 조향 속도")]
    [SerializeField] private float highSpeedSteerSpeed = 30f;
    [Tooltip("이 속도 이하에서는 조향하지 않음")]
    [SerializeField] private float minimumSteerSpeed = 0.5f;

    [Header("주행 감각")]
    [Tooltip("측면 미끄러짐을 얼마나 빨리 잡을지")]
    [SerializeField] private float lateralGrip = 8f;
    [Tooltip("악셀을 떼었을 때 자연스럽게 줄어드는 속도")]
    [SerializeField] private float coastDeceleration = 0.6f;

    [Header("브레이크")]
    [SerializeField]
    private float brakeDeceleration = 12f;

    private bool brakeInput;

    private Rigidbody rb;

    private float moveInput;
    private float steerInput;

    private bool isDriving;

    public float CurrentSteerInput
    {
        get { return steerInput; }
    }

    public bool IsDriving
    {
        get { return isDriving; }
    }

    public float ForwardSpeed
    {
        get
        {
            if (rb == null) return 0f;

            return Vector3.Dot(rb.velocity, transform.forward);
        }
    }

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
            brakeInput = false;
            return;
        }

        moveInput = Input.GetAxisRaw("Vertical");
        steerInput = Input.GetAxisRaw("Horizontal");
        brakeInput = Input.GetKey(KeyCode.Space);
    }


    private void FixedUpdate()
    {
        if (!isDriving) return;

        HandleMovement();

        if (brakeInput)
        {
            HandleBrake();
        }

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
            if (!brakeInput)
            {
                ApplyCoasting(forwardSpeed);
            }
        }
    }

    private void HandleSteering()
    {
        if (Mathf.Abs(steerInput) < 0.01f) return;

        float forwardSpeed = Vector3.Dot(rb.velocity, transform.forward);
        float absoluteSpeed = Mathf.Abs(forwardSpeed);

        if (absoluteSpeed < minimumSteerSpeed) return;

        float referenceMaxSpeed = forwardSpeed >= 0f ? maxForwardSpeed : maxReverseSpeed;
        float speedFactor = Mathf.InverseLerp(minimumSteerSpeed, referenceMaxSpeed, absoluteSpeed);
        float currentSteerSpeed = Mathf.Lerp(lowSpeedSteerSpeed, highSpeedSteerSpeed, speedFactor);
        float drivingDirection = Mathf.Sign(forwardSpeed);

        Quaternion turnRotation = Quaternion.Euler(0f, steerInput * currentSteerSpeed * drivingDirection * Time.fixedDeltaTime, 0f);

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

    private void HandleBrake()
    {
        Vector3 velocity = rb.velocity;
        Vector3 verticalVelocity = Vector3.up * velocity.y;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

        float currentSpeed = horizontalVelocity.magnitude;

        if (currentSpeed < 0.01f) return;

        float nextSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeDeceleration * Time.fixedDeltaTime);

        Vector3 nextHorizontalVelocity = horizontalVelocity.normalized * nextSpeed;

        rb.velocity = nextHorizontalVelocity + verticalVelocity;
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