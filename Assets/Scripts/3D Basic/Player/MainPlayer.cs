using UnityEngine;

public class MainPlayer : MonoBehaviour
{
    private Animator Animator;
    private Rigidbody Rigidbody;
    //private Vector2 MouseInput;
    //private float CameraPitch = 0f;

    [SerializeField]
    private float CharacterSpeed = 5.0f; // 캐릭터 속도
    [SerializeField]
    private Transform CharacterBody; // 메인 캐릭터
    [SerializeField]
    private Transform CameraArm; // 메인 캐릭터의 카메라
    [SerializeField]
    //private float MouseSensitivity = 2.0f;


    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Animator = CharacterBody.GetComponentInChildren<Animator>();
    }

    void Update()
    {
        LookAround();
    }

    private void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool Ismove = moveInput.magnitude != 0;
        //Animator.SetBool("Iswalk", Iswalk);

        if (Ismove)
        {
            Vector3 lookForward = new Vector3(CameraArm.forward.x, 0f, CameraArm.forward.z).normalized;
            Vector3 lookRight = new Vector3(CameraArm.right.x, 0f, CameraArm.right.z).normalized;
            Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;

            CharacterBody.forward = lookForward; // 이동할 때 이동방향 바라보게 세팅
            transform.position += moveDir * Time.deltaTime * CharacterSpeed; // 이동
        }
    }

    private void LookAround()
    {
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector3 CameraAngle = CameraArm.rotation.eulerAngles;
        float x = CameraAngle.x - mouseInput.y;
        if (x < 180f) x = Mathf.Clamp(x, -1f, 70f);
        else x = Mathf.Clamp(x, 335f, 361f);

        CameraArm.rotation = Quaternion.Euler(x, CameraAngle.y + mouseInput.x, CameraAngle.z);
    }
}
