using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginSceneController : MonoBehaviour
{
    [Header("인증")]
    [SerializeField] private UgsAuthenticationManager authenticationManager;

    [Header("로그인 패널")]
    [SerializeField] private GameObject loginChoicePanel;
    [SerializeField] private Button googleLoginButton;
    [SerializeField] private Button guestLoginButton;
    [SerializeField] private TMP_Text loginStatusText;

    [Header("게임 시작 패널")]
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private Button startClickButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private TMP_Text accountInfoText;
    [SerializeField] private CanvasGroup pressToStartCanvasGroup;

    [Header("로딩 패널")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;

    [Header("시작 텍스트 점멸")]
    [SerializeField, Min(0.1f)]
    private float blinkSpeed = 1f;

    [Header("씬 이동")]
    [SerializeField] private string citySceneName = "City";

    private Coroutine blinkCoroutine;
    private bool isLoadingScene;

    private void Awake()
    {
        guestLoginButton?.onClick.AddListener(OnClickGuestLogin);
        startClickButton?.onClick.AddListener(OnClickStartGame);
        logoutButton?.onClick.AddListener(OnClickLogout);

        // Google 로그인은 다음 단계에서 구현
        if (googleLoginButton != null)
        {
            googleLoginButton.interactable = false;
        }
    }

    private async void Start()
    {
        ShowLoading("서버에 연결 중...");

        bool initialized = await authenticationManager.InitializeAsync();

        if (!initialized)
        {
            ShowLoginChoice(authenticationManager.LastErrorMessage);

            return;
        }

        ShowLoading("로그인 정보를 확인 중...");

        bool restored = await authenticationManager.TryRestoreSessionAsync();

        if (restored)
        {
            ShowReadyPanel();
        }
        else
        {
            ShowLoginChoice();
        }
    }

    #region 버튼 입력

    private async void OnClickGuestLogin()
    {
        SetLoginButtonsInteractable(false);
        ShowLoading("게스트 로그인 중...");

        bool success = await authenticationManager.SignInAsGuestAsync();

        if (success)
        {
            ShowReadyPanel();
        }
        else
        {
            ShowLoginChoice(authenticationManager.LastErrorMessage);
        }
    }

    private void OnClickStartGame()
    {
        if (isLoadingScene)
        {
            return;
        }

        if (!authenticationManager.IsSignedIn)
        {
            ShowLoginChoice("로그인 정보가 없습니다.");
            return;
        }

        StartCoroutine(LoadCitySceneRoutine());
    }

    private void OnClickLogout()
    {
        StopBlink();

        authenticationManager.SignOutAndClearSession();

        ShowLoginChoice("로그아웃되었습니다.");
    }

    #endregion

    #region 패널 전환
    private void ShowLoading(string message)
    {
        loginChoicePanel.SetActive(false);
        readyPanel.SetActive(false);
        loadingPanel.SetActive(true);

        if (loadingText != null)
        {
            loadingText.text = message;
        }
    }

    private void ShowLoginChoice(string message = "")
    {
        StopBlink();

        loadingPanel.SetActive(false);
        readyPanel.SetActive(false);
        loginChoicePanel.SetActive(true);

        SetLoginButtonsInteractable(true);

        if (loginStatusText != null)
        {
            loginStatusText.text = message;
        }
    }

    private void ShowReadyPanel()
    {
        loginChoicePanel.SetActive(false);
        loadingPanel.SetActive(false);
        readyPanel.SetActive(true);

        if (accountInfoText != null)
        {
            accountInfoText.text = $"로그인 완료\n" + $"Player ID: {authenticationManager.PlayerId}";
        }

        StartBlink();
    }

    private void SetLoginButtonsInteractable(bool interactable)
    {
        if (guestLoginButton != null)
        {
            guestLoginButton.interactable = interactable;
        }

        // Google 버튼은 아직 구현 전이므로 계속 비활성화
        if (googleLoginButton != null)
        {
            googleLoginButton.interactable = false;
        }
    }

    #endregion

    #region 시작 문구 점멸
    private void StartBlink()
    {
        StopBlink();

        if (pressToStartCanvasGroup == null) return;

        blinkCoroutine = StartCoroutine(BlinkTextRoutine());
    }

    private void StopBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (pressToStartCanvasGroup != null)
        {
            pressToStartCanvasGroup.alpha = 1f;
        }
    }

    private IEnumerator BlinkTextRoutine()
    {
        while (true)
        {
            float alpha = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f);

            pressToStartCanvasGroup.alpha = alpha;

            yield return null;
        }
    }

    #endregion

    #region 씬 이동
    private IEnumerator LoadCitySceneRoutine()
    {
        isLoadingScene = true;

        StopBlink();
        ShowLoading("게임을 불러오는 중...");

        AsyncOperation operation = SceneManager.LoadSceneAsync(citySceneName);

        if (operation == null)
        {
            isLoadingScene = false;
            ShowReadyPanel();
            yield break;
        }

        while (!operation.isDone)
        {
            yield return null;
        }
    }

    #endregion

    private void OnDestroy()
    {
        guestLoginButton?.onClick.RemoveListener(OnClickGuestLogin);
        startClickButton?.onClick.RemoveListener(OnClickStartGame);
        logoutButton?.onClick.RemoveListener(OnClickLogout);
    }
}