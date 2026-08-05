using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_CharacterGoods : MonoBehaviour
{
    [SerializeField] private TMP_Text apText;

    private float refreshTimer;

    private void Awake()
    {
        if (apText == null) apText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (Account_Manager.Instance == null)
        {
            return;
        }

        Account_Manager.Instance.OnAPChanged += UpdateAPText;

        UpdateAPText(Account_Manager.Instance.currentAP, Account_Manager.Instance.maxAP);
    }

    private void OnDisable()
    {
        if (Account_Manager.Instance != null)
        {
            Account_Manager.Instance.OnAPChanged -= UpdateAPText;
        }
    }

    private void UpdateAPText(int currentAP, int maxAP)
    {
        if (apText == null) return;

        apText.text = $"{currentAP} / {maxAP}";
    }
}
