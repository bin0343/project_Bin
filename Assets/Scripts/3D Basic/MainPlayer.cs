using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MainPlayer : MonoBehaviour
{
    private Animator Animator;
    private Rigidbody Rigidbody;
    private Vector2 MouseInput;

    [SerializeField]
    private float CharacterSpeed = 5.0f; // 캐릭터 속도
    [SerializeField]
    private Transform CharacterBody; // 메인 캐릭터
    [SerializeField]
    private Transform CharacterCamera; // 메인 캐릭터의 카메라

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Animator = CharacterBody.GetComponentInChildren<Animator>();
    }

    void Update()
    {
        CameraMove();
    }

    private void FixedUpdate()
    {
        PlayerMove();
    }

    void PlayerMove()
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool Iswalk = moveInput.magnitude != 0;
        //Animator.SetBool("Iswalk", Iswalk);

        if (Iswalk)
        {
            Vector3 lookForward = new Vector3(CharacterCamera.forward.x, 0f, CharacterCamera.forward.z).normalized;
            Vector3 lookRight = new Vector3(CharacterCamera.right.x, 0f, CharacterCamera.right.z).normalized;
            Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;

            CharacterBody.forward = moveDir; // 이동할 때 이동방향 바라보게 세팅
            transform.position += moveDir * Time.deltaTime * CharacterSpeed; // 이동
        }
    }

    void CameraMove()
    {
        MouseInput.x = Input.GetAxis("Mouse X"); // 마우스 좌우 수치
        MouseInput.y = Input.GetAxis("Mouse Y"); // 마우스 위아래 수치
        Vector3 CameraAngle = CharacterCamera.rotation.eulerAngles;
        // 카메라의 원래 각도를 오일러 각으로 저장
        float x = CameraAngle.x - MouseInput.y;
        // 카메라의 피치 값 계산

        // 캐릭터 카메라 상하 움직임 제한 (위쪽으로 70도, 아래쪽으로 25도 이상 움직이지 못하게 제한)
        if (x < 180f) x = Mathf.Clamp(x, -1f, 70f);
        else x = Mathf.Clamp(x, 335f, 361f);

        CharacterCamera.rotation = Quaternion.Euler(x, CameraAngle.y + MouseInput.x, CameraAngle.z);
        // 카메라 암 회전
    }
}
