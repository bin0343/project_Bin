using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraArm : MonoBehaviour
{
    [SerializeField] private Transform cameraArm;    // 카메라 회전축 (pivot)
    [SerializeField] private Transform headBone;
    [SerializeField] private Camera ThirdPersonCamera;   // 실제 카메라
    [SerializeField] public Camera FirstPersonCamera;   // 실제 카메라
    [SerializeField] private float zoomSpeed = 2.0f; // 줌 속도
    [SerializeField] private float minZoom = 0.0f;   // 1인칭 시점
    [SerializeField] private float maxZoom = 5.0f;   // 최대 줌아웃 거리
    [SerializeField] private Vector3 headOffset = new Vector3(0, 0.1f, 0); // 머리 기준 보정값
    //[SerializeField] private float followSpeed = 10f;
    [SerializeField] private Transform playerBody; // 캐릭터 루트(몸통)
    private float xRotation = 0f; // 카메라 Pitch 저장
    private float CurrentZoom = -3.0f;

    private void Start()
    {
        ThirdPersonCamera.enabled = true;
        FirstPersonCamera.enabled = false;
    }

    private void Update()
    {
        LookAround();
        ZoomCamera();
    }

    private void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (FirstPersonCamera.enabled)
        {
            // 마우스 입력
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -70f, 80f);

            // 좌우(Yaw)는 캐릭터 몸통이 회전
            playerBody.Rotate(Vector3.up * mouseX);

            // 카메라는 Pitch만 담당하지만,
            // 플레이어 몸통 회전(Yaw)을 기준으로 회전해야 함
            FirstPersonCamera.transform.rotation =
                Quaternion.Euler(xRotation, playerBody.rotation.eulerAngles.y, 0f);
        }
        else
        {
            // 3인칭 모드 기존 방식 유지
            Vector3 cameraAngle = cameraArm.rotation.eulerAngles;
            float x = cameraAngle.x - mouseY;

            if (x < 180f) x = Mathf.Clamp(x, -1f, 70f);
            else x = Mathf.Clamp(x, 335f, 361f);

            cameraArm.rotation = Quaternion.Euler(x, cameraAngle.y + mouseX, cameraAngle.z);
        }
    }

    private void LateUpdate()
    {
        if (FirstPersonCamera.enabled)
        {
            // 카메라 위치를 headBone에 고정 (로컬 위치 기준)
            FirstPersonCamera.transform.position = headBone.position + headOffset;

            // Pitch만 적용 (x축 회전)
            Vector3 euler = FirstPersonCamera.transform.localEulerAngles;
            euler.x = xRotation;
            FirstPersonCamera.transform.localEulerAngles = euler;
        }
    }

    private void ZoomCamera()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            CurrentZoom += scroll * zoomSpeed;
            CurrentZoom = Mathf.Clamp(CurrentZoom, -maxZoom, -minZoom);

            ThirdPersonCamera.transform.localPosition = new Vector3(0, 0, CurrentZoom);
        }

        // 전환 조건 수정
        if (CurrentZoom >= -minZoom - 0.1f)
        {
            ThirdPersonCamera.enabled = false;
            FirstPersonCamera.enabled = true;
        }
        else
        {
            ThirdPersonCamera.enabled = true;
            FirstPersonCamera.enabled = false;
        }
    }
}
