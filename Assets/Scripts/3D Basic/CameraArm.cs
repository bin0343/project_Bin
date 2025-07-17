using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraArm : MonoBehaviour
{
    [SerializeField]
    private Transform Cameraarm; // 메인 캐릭터의 카메라
    [SerializeField]
    private float MouseSensitivity = 2.0f;
    

    //private float CameraPitch = 0f;  //카메라 상하 회전 값 저장

    void Start()
    {
        
    }

    void Update()
    {
        LookAround();
    }

    private void LookAround()
    {
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector3 CameraAngle = Cameraarm.rotation.eulerAngles;
        float x = CameraAngle.x - mouseInput.y;
        if (x < 180f) x = Mathf.Clamp(x, -1f, 70f);
        else x = Mathf.Clamp(x, 335f, 361f);

        Cameraarm.rotation = Quaternion.Euler(x, CameraAngle.y + mouseInput.x, CameraAngle.z);
    }
}
