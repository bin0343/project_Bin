using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUIController : MonoBehaviour
{
    public Image skillIcon;
    public Image FillMask;
    public TextMeshProUGUI keyText;
    public TextMeshProUGUI cooldownText;

    private float cooldown = 0f;
    private float cooldownTimer = 0f;

    public void SetCooldown(float cd)
    {
        cooldown = cd;
        cooldownTimer = cd;
        //skillIcon.color = new Color(1f, 1f, 1f, 0.4f); // 반투명 처리
        cooldownText.gameObject.SetActive(true);
        FillMask.fillAmount = 1f;
        FillMask.gameObject.SetActive(true);
    }

    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            float percent = Mathf.Clamp01(cooldownTimer / cooldown);
            FillMask.fillAmount = percent;
            cooldownText.text = Mathf.Ceil(cooldownTimer).ToString("0");

            if (cooldownTimer <= 0f)
            {
                FillMask.gameObject.SetActive(false);
                cooldownText.gameObject.SetActive(false);
                //skillIcon.color = new Color(1f, 1f, 1f, 1f); // 원래대로
            }
        }
    }
}