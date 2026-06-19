using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompanionHpBar : MonoBehaviour
{
    public Transform target;  // 머리 위치 (예: 몬스터 머리본 Transform)
    public Vector3 offset = new Vector3(0, 2f, 0); // 머리 위로 띄우는 오프셋
    private Camera cam;
    public Image hpBarFront; // 체력바 fill 이미지 (UI에서 할당)
    public Text damageText;
    private Character_Stat stat; // 연동할 몬스터 스탯

    private Coroutine hpChangeCoroutine;

    void Start()
    {
        stat = GetComponentInParent<Character_Stat>();
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            if (cam == null || !cam.gameObject.activeInHierarchy)
            {
                cam = Camera.main;
            }

            if (cam != null)
            {
                // [옵션 1] 롤/로아 스타일 (가장 추천)
                // 카메라가 쿼터뷰로 위에서 내려다봐도, HP바는 찌그러지지 않고 화면에 평면으로 보이게 함
                transform.rotation = cam.transform.rotation;

                // [옵션 2] 수직으로 세우기 (원하신다면 주석 풀고 사용)
                // 카메라를 바라보긴 하는데, 뒤로 눕지 않고 땅에 수직으로 서있는 표지판처럼 만듦
                // transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, Vector3.up);
            }
        }
    }

    public void Setup(Character_Stat newStat)
    {
        stat = newStat;
        if (stat != null && hpBarFront != null)
        {
            hpBarFront.fillAmount = (float)stat.currentHP / stat.maxHP;
        }
    }


    public void UpdateHpBar()
    {
        if (stat != null && hpBarFront != null)
        {
            if (hpChangeCoroutine != null)
                StopCoroutine(hpChangeCoroutine);

            float targetFillAmount = (float)stat.currentHP / stat.maxHP;
            hpChangeCoroutine = StartCoroutine(AnimateImageFill(targetFillAmount));
        }
    }

    private IEnumerator AnimateImageFill(float targetValue)
    {
        float startValue = hpBarFront.fillAmount;
        float elapsed = 0f;
        float duration = 0.3f; // 애니메이션 시간 (0.3초)

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            hpBarFront.fillAmount = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            yield return null; // 다음 프레임까지 대기
        }

        hpBarFront.fillAmount = targetValue;
    }
}
