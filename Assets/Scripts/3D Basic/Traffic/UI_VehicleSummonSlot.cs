using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_VehicleSummonSlot : MonoBehaviour
{
    [SerializeField]
    private VehicleSummonManager summonManager;

    [Header("UI")]
    [SerializeField] private Image cooldownFill;

    [SerializeField] private TMP_Text cooldownText;


    private void Update()
    {
        if (summonManager == null) return;

        float remaining = summonManager.RemainingCooldown;
        float cooldown = summonManager.SummonCooldown;

        if (remaining > 0f)
        {
            float ratio = remaining / cooldown;

            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = ratio;
            }

            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(true);

                cooldownText.text = remaining.ToString("F1");
            }
        }
        else
        {
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = 0f;
            }

            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(false);
            }
        }
    }
}