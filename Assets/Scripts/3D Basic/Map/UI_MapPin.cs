using UnityEngine;
using UnityEngine.UI;

public class UI_MapPin : MonoBehaviour
{
    [Header("애니메이션 설정")]
    public float floatHeight = 15f; // 위아래로 움직이는 범위 (픽셀 단위)
    public float floatSpeed = 5f;   // 움직이는 속도

    private RectTransform rectTrans;
    private Vector3 originalPos;
    private bool isHovering = false;
    private float currentAngle = 0f; // 애니메이션 각도 (타이머 역할)

    void Awake()
    {
        rectTrans = GetComponent<RectTransform>();
    }

    // 핀이 처음 생성될 때 위치를 기억 (MapController가 호출)
    public void Setup()
    {
        originalPos = rectTrans.localPosition;
    }

    void Update()
    {
        // 마우스가 버튼 위에 있을 때: 계속 위아래로 둥둥
        if (isHovering)
        {
            // 시간을 멈춰도(TimeScale=0) 움직이도록 unscaledDeltaTime 사용
            currentAngle += Time.unscaledDeltaTime * floatSpeed;

            // Sin 그래프를 이용해 위아래 반복 계산
            float newY = originalPos.y + Mathf.Sin(currentAngle) * floatHeight;

            rectTrans.localPosition = new Vector3(originalPos.x, newY, originalPos.z);
        }
        else
        {
            // 마우스 떼면: 각도 초기화하고 원래 위치로 부드럽게 복귀
            currentAngle = 0f;
            rectTrans.localPosition = Vector3.Lerp(rectTrans.localPosition, originalPos, Time.unscaledDeltaTime * 10f);
        }
    }

    public void SetHover(bool state)
    {
        isHovering = state;
    }
}