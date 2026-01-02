using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 마우스 감지용 필수
using System.Collections;

public class NPC_Hover_Effect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("연결할 UI")]
    public GameObject nameTagObj;     // 하단 이름표 오브젝트
    public GameObject bubbleObj;      // 말풍선 오브젝트 (이미지)
    public Text bubbleText;           // 말풍선 안의 텍스트 (점 찍히는 곳)

    [Header("설정")]
    public float dotSpeed = 0.5f;     // 점 바뀌는 속도 (초)

    private Coroutine animCoroutine;  // 애니메이션 제어용

    void Start()
    {
        // 시작할 때 다 숨기기
        if (nameTagObj != null) nameTagObj.SetActive(false);
        if (bubbleObj != null) bubbleObj.SetActive(false);
    }

    // 마우스가 들어왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 1. 이름표 켜기
        if (nameTagObj != null) nameTagObj.SetActive(true);

        // 2. 말풍선 켜고 애니메이션 시작
        if (bubbleObj != null)
        {
            bubbleObj.SetActive(true);

            // 혹시 돌아가던 게 있으면 끄고 새로 시작
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(DotAnimation());
        }
    }

    // 마우스가 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        // 1. 이름표 끄기
        if (nameTagObj != null) nameTagObj.SetActive(false);

        // 2. 말풍선 끄고 애니메이션 중단
        if (bubbleObj != null)
        {
            bubbleObj.SetActive(false);
            if (animCoroutine != null) StopCoroutine(animCoroutine);
        }
    }

    // 점 1개 -> 2개 -> 3개 무한 반복 코루틴
    IEnumerator DotAnimation()
    {
        while (true)
        {
            if (bubbleText != null) bubbleText.text = "·";
            yield return new WaitForSeconds(dotSpeed);

            if (bubbleText != null) bubbleText.text = "··";
            yield return new WaitForSeconds(dotSpeed);

            if (bubbleText != null) bubbleText.text = "···";
            yield return new WaitForSeconds(dotSpeed);
        }
    }

    // (선택사항) 버튼이 꺼질 때(다른 화면으로 갈 때) 강제로 초기화
    private void OnDisable()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        if (nameTagObj != null) nameTagObj.SetActive(false);
        if (bubbleObj != null) bubbleObj.SetActive(false);
    }
}