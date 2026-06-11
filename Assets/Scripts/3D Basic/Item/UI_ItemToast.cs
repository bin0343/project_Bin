using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 라이브러리 사용

public class UI_ItemToast : MonoBehaviour
{
    public Image itemIcon;
    public Text itemNameText;
    public Text itemAmountText;
    public CanvasGroup canvasGroup; // 투명도 조절용

    public void Setup(Item_Base item, int amount)
    {
        itemIcon.sprite = item.itemIcon;
        itemNameText.text = item.itemName;
        itemAmountText.text = $"x{amount}"; // 끝쪽에 있는 갯수 텍스트에 숫자 반영

        canvasGroup.alpha = 0f;
        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1f, 0.3f)); // 0.3초 동안 페이드 인
        seq.AppendInterval(2.0f);                 // 2초 대기
        seq.Append(canvasGroup.DOFade(0f, 0.3f)); // 0.3초 동안 페이드 아웃
        seq.OnComplete(() => Destroy(gameObject)); // 완전히 안 보이면 오브젝트 파괴
    }
}