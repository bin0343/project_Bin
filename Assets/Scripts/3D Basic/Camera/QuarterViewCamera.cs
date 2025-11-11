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
    }

    void LateUpdate()
    {
        if (target == null) return;

        //카메라의 위치 = 타겟위치 + 오프셋
        transform.position = target.position + offset;
    }
}
