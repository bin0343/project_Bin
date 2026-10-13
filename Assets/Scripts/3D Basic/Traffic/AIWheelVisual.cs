using UnityEngine;

public class AIWheelVisual : MonoBehaviour
{
    [Header("Â÷·®")]
    [SerializeField] private CarPathFollower pathFollower;

    [Header("¾Õ¹ÙÄû Á¶Çâ Pivot")]
    [SerializeField] private Transform frontLeftSteer;
    [SerializeField] private Transform frontRightSteer;

    [Header("4¹ÙÄû È¸Àü Pivot")]
    [SerializeField] private Transform frontLeftSpin;
    [SerializeField] private Transform frontRightSpin;
    [SerializeField] private Transform backLeftSpin;
    [SerializeField] private Transform backRightSpin;

    [Header("¹ÙÄû ¼³Á¤")]
    [Min(0.01f)]
    [SerializeField] private float wheelRadius = 0.32f;

    private float spinAngle;

    private Quaternion frontLeftSteerBase;
    private Quaternion frontRightSteerBase;
    private Quaternion frontLeftSpinBase;
    private Quaternion frontRightSpinBase;
    private Quaternion backLeftSpinBase;
    private Quaternion backRightSpinBase;


    private void Awake()
    {
        if (pathFollower == null)
        {
            pathFollower = GetComponent<CarPathFollower>();
        }
        if (frontLeftSteer != null)
        {
            frontLeftSteerBase = frontLeftSteer.localRotation;
        }
        if (frontRightSteer != null)
        {
            frontRightSteerBase = frontRightSteer.localRotation;
        }
        if (frontLeftSpin != null)
        {
            frontLeftSpinBase = frontLeftSpin.localRotation;
        }
        if (frontRightSpin != null)
        {
            frontRightSpinBase = frontRightSpin.localRotation;
        }
        if (backLeftSpin != null)
        {
            backLeftSpinBase = backLeftSpin.localRotation;
        }
        if (backRightSpin != null)
        {
            backRightSpinBase = backRightSpin.localRotation;
        }
    }


    private void Update()
    {
        if (pathFollower == null) return;

        UpdateSteering();

        UpdateWheelSpin();
    }

    private void UpdateSteering()
    {
        float steerAngle = pathFollower.VisualSteerAngle;

        if (frontLeftSteer != null)
        {
            frontLeftSteer.localRotation = frontLeftSteerBase * Quaternion.AngleAxis(steerAngle, Vector3.up);
        }
        if (frontRightSteer != null)
        {
            frontRightSteer.localRotation = frontRightSteerBase * Quaternion.AngleAxis(steerAngle, Vector3.up);
        }
    }

    private void UpdateWheelSpin()
    {
        if (wheelRadius <= 0f) return;

        float speed = pathFollower.CurrentSpeed;
        float angleDelta = (speed / wheelRadius) * Mathf.Rad2Deg * Time.deltaTime;

        spinAngle += angleDelta;

        ApplySpin(frontLeftSpin, frontLeftSpinBase);
        ApplySpin(frontRightSpin, frontRightSpinBase);
        ApplySpin(backLeftSpin, backLeftSpinBase);
        ApplySpin(backRightSpin, backRightSpinBase);
    }

    private void ApplySpin(Transform wheel, Quaternion baseRotation)
    {
        if (wheel == null) return;

        wheel.localRotation = baseRotation * Quaternion.AngleAxis(spinAngle, Vector3.right);
    }
}