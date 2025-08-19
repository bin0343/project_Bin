using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraArm : MonoBehaviour
{
    [SerializeField] private Transform cameraArm;    // 카메라 회전축 (pivot)
    [SerializeField] private Transform mainCamera;   // 실제 카메라
    [SerializeField] private float zoomSpeed = 2.0f; // 줌 속도
    [SerializeField] private float minZoom = 0.0f;   // 1인칭 시점
    [SerializeField] private float maxZoom = 5.0f;   // 최대 줌아웃 거리

    private void Update()
    {
        LookAround();
        ZoomCamera();
    }

    private void LookAround()
    {
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector3 cameraAngle = cameraArm.rotation.eulerAngles;
        float x = cameraAngle.x - mouseInput.y;

        if (x < 180f) x = Mathf.Clamp(x, -1f, 70f);
        else x = Mathf.Clamp(x, 335f, 361f);

        cameraArm.rotation = Quaternion.Euler(x, cameraAngle.y + mouseInput.x, cameraAngle.z);
    }

    private void ZoomCamera()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 localPos = mainCamera.localPosition;
            localPos.z += scroll * zoomSpeed;
            localPos.z = Mathf.Clamp(localPos.z, -maxZoom, -minZoom);
            mainCamera.localPosition = localPos;
        }
    }
}
