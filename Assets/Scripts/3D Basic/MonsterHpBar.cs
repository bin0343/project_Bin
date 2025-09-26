using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterHpBar : MonoBehaviour
{
    public Transform target;  // 머리 위치 (예: 몬스터 머리본 Transform)
    public Vector3 offset = new Vector3(0, 2f, 0); // 머리 위로 띄우는 오프셋
    private Camera cam;
    public Image hpBarFront; // 체력바 fill 이미지 (UI에서 할당)
    public Text damageText;
    private Enemy_Stat stat; // 연동할 몬스터 스탯

    private Coroutine hpChangeCoroutine;

    void Start()
    {
        stat = GetComponentInParent<Enemy_Stat>();
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.rotation = cam.transform.rotation; // 카메라 완전 정면
        }
    }

    public void Setup(Enemy_Stat stat)
    {
        //stat = stat;
        hpBarFront.fillAmount = (float)stat.currentHP / stat.maxHP;
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
