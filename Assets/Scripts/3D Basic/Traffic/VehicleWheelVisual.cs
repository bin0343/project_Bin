using UnityEngine;

public class VehicleWheelVisual : MonoBehaviour
{
    [Header("차량")]
    [SerializeField] private PlayerVehicleController controller;

    [Header("앞바퀴 조향 Pivot")]
    [SerializeField] private Transform frontLeftSteer;
    [SerializeField] private Transform frontRightSteer;

    [Header("바퀴 회전 Pivot")]
    [SerializeField] private Transform frontLeftSpin;
    [SerializeField] private Transform frontRightSpin;
    [SerializeField] private Transform rearLeftSpin;
    [SerializeField] private Transform rearRightSpin;

    [Header("바퀴 설정")]
    [SerializeField] private float wheelRadius = 0.35f;
    [SerializeField] private float maxVisualSteerAngle = 30f;
    private float wheelSpinAngle;


    private void Update()
    {
        if (controller == null) return;

        UpdateWheelSpin();
        UpdateSteering();
    }


    private void UpdateWheelSpin()
    {
        if (wheelRadius <= 0f) return;

        float speed = controller.ForwardSpeed;

        float angleDelta = (speed / wheelRadius) * Mathf.Rad2Deg * Time.deltaTime;

        wheelSpinAngle += angleDelta;

        ApplySpin(frontLeftSpin);
        ApplySpin(frontRightSpin);
        ApplySpin(rearLeftSpin);
        ApplySpin(rearRightSpin);
    }


    private void ApplySpin(Transform wheel)
    {
        if (wheel == null) return;

        wheel.localRotation = Quaternion.Euler(wheelSpinAngle, 0f, 0f);
    }


    private void UpdateSteering()
    {
        float steerAngle = controller.CurrentSteerInput * maxVisualSteerAngle;

        if (frontLeftSteer != null)
        {
            frontLeftSteer.localRotation = Quaternion.Euler(0f, steerAngle, 0f);
        }

        if (frontRightSteer != null)
        {
            frontRightSteer.localRotation = Quaternion.Euler(0f, steerAngle, 0f);
        }
    }
}