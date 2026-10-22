using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class CityLoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text percentText;

    [Header("진행률 연출")]
    [SerializeField, Min(1f)]
    private float progressSpeed = 45f;

    [SerializeField, Min(0f)]
    private float completeHoldSeconds = 0.7f;

    private float displayedProgress;
    private float targetProgress;

    private static bool showOnNextCityLoad;

    private bool activeForThisEntry;

    public static void RequestShowOnNextCityLoad()
    {
        showOnNextCityLoad = true;
    }

    private void Awake()
    {
        activeForThisEntry = showOnNextCityLoad;

        showOnNextCityLoad = false;

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(activeForThisEntry);
        }

        if (activeForThisEntry)
        {
            displayedProgress = 0f;
            targetProgress = 0f;
            UpdatePercentText();
        }
    }

    private void Update()
    {
        if (loadingPanel == null || !loadingPanel.activeSelf) return;

        displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, progressSpeed * Time.unscaledDeltaTime);

        UpdatePercentText();
    }

    public void Show()
    {
        if (!activeForThisEntry) return;

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        displayedProgress = 0f;
        targetProgress = 0f;

        UpdatePercentText();
    }

    public void SetProgress(int percent)
    {
        if (!activeForThisEntry) return;

        targetProgress = Mathf.Clamp(percent, 0, 100);
    }

    public void SetMessage(string message)
    {
        if (!activeForThisEntry) return;

        if (loadingText != null)
        {
            loadingText.text = message;
        }
    }

    private void UpdatePercentText()
    {
        if (percentText != null)
        {
            percentText.text = $"{Mathf.RoundToInt(displayedProgress)}%";
        }
    }

    public async Task CompleteAsync()
    {
        if (!activeForThisEntry) return;


        targetProgress = 100f;

        // 화면에 보이는 숫자가 실제로 100%가 될 때까지 기다림
        while (displayedProgress < 99.9f)
        {
            await Task.Yield();
        }

        displayedProgress = 100f;
        UpdatePercentText();

        SetMessage("완료");

        // 100% 상태를 잠시 보여줌
        float elapsed = 0f;

        while (elapsed < completeHoldSeconds)
        {
            elapsed += Time.unscaledDeltaTime;

            await Task.Yield();
        }

        Hide();
    }

    public void Hide()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    public void ShowError(string message)
    {
        if (loadingText != null)
        {
            loadingText.text = message;
        }
    }
}