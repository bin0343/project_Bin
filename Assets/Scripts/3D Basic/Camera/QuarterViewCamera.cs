using UnityEngine;

public class QuarterViewCamera : MonoBehaviour
{
    [SerializeField] private Transform target;  //캐릭터
    [SerializeField] private Vector3 offset = new Vector3(0, 15f, -10f);    //쿼터뷰 고정 오프셋(높이 15, 거리 10)
    [SerializeField] private float fixedRotationX = 45f;    //x축 고정 각도 (아래 바라보는 각도)
    
    void Start()
    {
        //시작할 때 카메라의 회전 값을 쿼터뷰 각도로 고정
        transform.rotation = Quaternion.Euler(fixedRotationX, 0, 0);

        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                target = playerObj.transform;
                Player_Move playerMove = playerObj.GetComponent<Player_Move>();

                if (playerMove != null)
                {
                    // 1. 이동 기준을 이 쿼터뷰 카메라로 변경 (WASD 방향 맞춤)
                    playerMove.SetReferenceTransform(this.transform);

                    // 2. [중요] 플레이어에게 붙어있는 DailyCamera(백뷰)를 비활성화!
                    // 이렇게 하면 플레이어 쪽 AudioListener도 같이 꺼져서 중복 오류가 해결됨.
                    playerMove.SetDailyCameraActive(false);
                }

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        //카메라의 위치 = 타겟위치 + 오프셋
        transform.position = target.position + offset;
    }
}
