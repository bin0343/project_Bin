using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private static GameObject instance;

    public Transform target; // 따라갈 대상 (보통 플레이어)
    public Vector2 minPosition; // 맵 왼쪽 아래 경계
    public Vector2 maxPosition; // 맵 오른쪽 위 경계

    public float smoothing = 5f; // 부드럽게 따라가기 정도

    void LateUpdate()
    {
        if (target != null)
        {
            // 현재 카메라 위치
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

            // 경계 안에 위치하도록 제한
            targetPosition.x = Mathf.Clamp(targetPosition.x, minPosition.x, maxPosition.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minPosition.y, maxPosition.y);

            // 부드럽게 이동
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
