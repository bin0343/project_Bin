using UnityEngine;

[DefaultExecutionOrder(-100)]
public class CameraFollowTarget : MonoBehaviour
{
    public Transform playerRoot;

    [Header("카메라 부드러움 세팅")]
    [Tooltip("값이 작을수록 칼같이 따라가고, 크면 부드럽습니다. 계단 떨림 흡수용으로 0.03 ~ 0.05를 추천합니다.")]
    public float smoothTime = 0.04f;

    [Header("카메라 높이 보정")]
    [Tooltip("카메라가 바라볼 높이 (발바닥 기준, 1.2 ~ 1.5 사이로 조절하면 원래 높이가 됩니다)")]
    public float heightOffset = 1.3f;

    private Vector3 currentVelocity;

    void Update()
    {
        if (playerRoot == null) return;

        Vector3 targetPos = playerRoot.position + Vector3.up * heightOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref currentVelocity,
            smoothTime
        );
    }
}