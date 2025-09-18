using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_BasicSkillOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image skillImage;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private Text cooldownText;

    private void Awake()
    {
        // 컴포넌트가 없다면 직접 찾아오기
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        // 처음엔 무조건 숨김 처리
        Hide();
    }

    private void OnEnable()
    {
        // Player_Action 스크립트가 보내는 이벤트에 구독(연결)
        Player_Action.OnRunningAttackUsed += StartCooldownDisplay;
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 이벤트 구독 해제 (메모리 누수 방지)
        Player_Action.OnRunningAttackUsed -= StartCooldownDisplay;
    }

    private void StartCooldownDisplay(Sprite icon, float duration)
    {
        if (skillImage != null)
        {
            skillImage.sprite = icon;
        }
        // 이미 실행 중인 코루틴이 있다면 멈추고 새로 시작
        StopAllCoroutines();
        StartCoroutine(CooldownCoroutine(duration));
    }

    private IEnumerator CooldownCoroutine(float duration)
    {
        Show(); // UI 표시

        float timer = duration;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            // UI 업데이트
            cooldownOverlay.fillAmount = timer / duration;
            cooldownText.text = timer.ToString("F1"); // 소수점 한 자리까지 표시

            yield return null; // 다음 프레임까지 대기
        }

        Hide(); // 쿨타임 종료 후 UI 숨김
    }

    private void Show()
    {
        canvasGroup.alpha = 1f;
        cooldownText.gameObject.SetActive(true);
        cooldownOverlay.gameObject.SetActive(true);
        if (skillImage != null)
        {
            skillImage.gameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        canvasGroup.alpha = 0f;
        cooldownText.gameObject.SetActive(false);
        cooldownOverlay.gameObject.SetActive(false);
        if (skillImage != null)
        {
            skillImage.gameObject.SetActive(false);
        }
    }
}
