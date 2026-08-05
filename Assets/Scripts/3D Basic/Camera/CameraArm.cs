using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraArm : MonoBehaviour
{
    [SerializeField] private Transform cameraArm;        // 인스펙터: CameraArm 자기 자신 연결
    [SerializeField] public Camera ThirdPersonCamera;
    [SerializeField] private float zoomSpeed = 2.0f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 5.0f;
    [SerializeField] private Transform playerBody;       // 인스펙터: Player 모델 오브젝트 연결

    [Header("태그 시스템용 타겟")]
    [SerializeField] private Transform currentTarget;

    private float CurrentZoom = -3.0f;

    void Start()
    {
        ThirdPersonCamera.enabled = true;
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
        
        Vector3 cameraAngle = cameraArm.rotation.eulerAngles;
        float x = cameraAngle.x - mouseY;

        if (x < 180f) x = Mathf.Clamp(x, -1f, 70f);
        else x = Mathf.Clamp(x, 335f, 361f);

        cameraArm.rotation = Quaternion.Euler(x, cameraAngle.y + mouseX, cameraAngle.z);
    }

    public void SetTarget(Transform newCharacter)
    {
        currentTarget = newCharacter;
    }

    // 업데이트가 끝난 후 카메라의 '위치'를 타겟에게 맞춤
    private void LateUpdate()
    {
        if (currentTarget != null)
        {
            // 캐릭터 발바닥 기준 1.5f 위(명치쯤)를 따라다니게 설정
            transform.position = currentTarget.position + Vector3.up * 1.5f;
        }
    }

    private void ZoomCamera()
    {
        CurrentZoom += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        CurrentZoom = Mathf.Clamp(CurrentZoom, -maxZoom, -minZoom);
        ThirdPersonCamera.transform.localPosition = new Vector3(0, 0, CurrentZoom);
    }
}
