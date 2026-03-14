using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_MiniPortrait : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image miniHpBar;

    private float currentTargetHp = -1;

    public void UpdatePortrait(Sprite icon, float currentHp, float maxHp)
    {
        if (portraitImage.sprite != icon)
        {
            portraitImage.sprite = icon;
        }

        if (currentTargetHp != currentHp)
        {
            currentTargetHp = currentHp;

            float hpRatio = maxHp > 0 ? currentHp / maxHp : 0;

            miniHpBar.DOKill();
            miniHpBar.DOFillAmount(hpRatio, 0.3f);
        }
    }
}
