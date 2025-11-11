using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraArm : MonoBehaviour
{
    [SerializeField] private Transform cameraArm;        // 인스펙터: CameraArm 자기 자신 연결
    //[SerializeField] private Transform headBone;         // 인스펙터: Player 모델의 머리 뼈 연결
    [SerializeField] public Camera ThirdPersonCamera;
    //[SerializeField] public Camera FirstPersonCamera;
    [SerializeField] private float zoomSpeed = 2.0f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 5.0f;
    //[SerializeField] private Vector3 headOffset = new Vector3(0, 0.1f, 0);
    [SerializeField] private Transform playerBody;       // 인스펙터: Player 모델 오브젝트 연결

    //private float xRotation = 0f;
    private float CurrentZoom = -3.0f;

    void Start()
    {
        ThirdPersonCamera.enabled = true;
        //FirstPersonCamera.enabled = false;
        // 마우스 커서 고정 및 숨기기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (UI_Manager.Instance != null && UI_Manager.Instance.IsUIOpen)
            return;
        LookAround();
        ZoomCamera();
    }

    private void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        /*if (FirstPersonCamera.enabled)
        {
            // 1인칭: Y축(상하) 회전값만 계산해서 카메라의 Local Rotation에 적용
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -70f, 80f);
            FirstPersonCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }*/
        // 3인칭: 카메라 피봇(CameraArm)을 마우스 움직임에 따라 회전
        Vector3 cameraAngle = cameraArm.rotation.eulerAngles;
        float x = cameraAngle.x - mouseY;

        if (x < 180f) x = Mathf.Clamp(x, -1f, 70f);
        else x = Mathf.Clamp(x, 335f, 361f);

        cameraArm.rotation = Quaternion.Euler(x, cameraAngle.y + mouseX, cameraAngle.z);
    }

    private void LateUpdate()
    {
        /*if (FirstPersonCamera.enabled)
        {
            // 1인칭일 때, 카메라의 월드 위치를 머리 뼈 위치로 고정
            FirstPersonCamera.transform.position = headBone.position + headBone.TransformDirection(headOffset);
        }*/
    }

    private void ZoomCamera()
    {
        CurrentZoom += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        CurrentZoom = Mathf.Clamp(CurrentZoom, -maxZoom, -minZoom);
        ThirdPersonCamera.transform.localPosition = new Vector3(0, 0, CurrentZoom);

        // 줌 거리에 따라 1인칭/3인칭 카메라 전환
        /*if (CurrentZoom >= -minZoom - 0.1f)
        {
            if (!FirstPersonCamera.enabled)
            {
                ThirdPersonCamera.enabled = false;
                FirstPersonCamera.enabled = true;
            }
        }
        else
        {
            if (!ThirdPersonCamera.enabled)
            {
                ThirdPersonCamera.enabled = true;
                FirstPersonCamera.enabled = false;
            }
        }*/
    }
}
