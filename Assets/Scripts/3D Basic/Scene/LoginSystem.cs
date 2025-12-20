using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoginSystem : MonoBehaviour
{
    [Header("--- 패널 (Popups) ---")]
    public GameObject loginPopup; 
    public GameObject createPopup; 

    [Header("--- 로그인 창 UI ---")]
    public TMP_InputField loginIdInput;
    public TMP_InputField loginPwInput;
    public Toggle autoLoginToggle; 
    public Text messageText;

    [Header("--- 계정 생성 창 UI ---")]
    public TMP_InputField createIdInput;
    public TMP_InputField createPwInput;

    [Header("--- 시작 화면 UI ---")]
    public CanvasGroup pressKeyTextGroup;
    public float blinkSpeed = 1.0f;

    private bool isLoginComplete = false;

    void Start()
    {
        if (messageText != null) messageText.text = "";

        if (PlayerPrefs.GetInt("AutoLogin", 0) == 1 && PlayerPrefs.HasKey("UserID"))
        {
            string savedId = PlayerPrefs.GetString("UserID");
            Debug.Log($"[자동 로그인] 환영합니다, {savedId}!");

            ShowTouchToStart();
        }
        else
        {
            ShowLoginPopup();
        }
    }

    void ShowLoginPopup()
    {
        loginPopup.SetActive(true);
        createPopup.SetActive(false);
        pressKeyTextGroup.gameObject.SetActive(false);

        loginIdInput.text = "";
        loginPwInput.text = "";
        if (messageText != null) messageText.text = "";
    }

    public void OnClickGoToCreate()
    {
        loginPopup.SetActive(false);
        createPopup.SetActive(true);

        createIdInput.text = "";
        createPwInput.text = "";
    }

    public void OnClickCancelCreate()
    {
        ShowLoginPopup();
    }

    public void OnClickLogin()
    {
        string inputId = loginIdInput.text;
        string inputPw = loginPwInput.text;

        if (string.IsNullOrEmpty(inputId) || string.IsNullOrEmpty(inputPw))
        {
            messageText.text = "아이디와 비밀번호를 입력해주세요.";
            return;
        }

        if (!PlayerPrefs.HasKey("UserID"))
        {
            messageText.text = "계정 정보가 없습니다.";
            return;
        }

        string savedId = PlayerPrefs.GetString("UserID");
        string savedPw = PlayerPrefs.GetString("UserPW");

        if (inputId == savedId && inputPw == savedPw)
        {
            Debug.Log("로그인 성공!");

            int autoLoginState = autoLoginToggle.isOn ? 1 : 0;
            PlayerPrefs.SetInt("AutoLogin", autoLoginState);
            PlayerPrefs.Save();

            ShowTouchToStart();
        }
        else
        {
            if (inputId != savedId)
                messageText.text = "계정 정보가 없습니다."; 
            else
                messageText.text = "비밀번호가 일치하지 않습니다."; 
        }
    }

    public void OnClickConfirmCreate()
    {
        string newId = createIdInput.text;
        string newPw = createPwInput.text;

        if (string.IsNullOrEmpty(newId) || string.IsNullOrEmpty(newPw))
        {
            Debug.Log("ID/PW를 입력해야 합니다.");
            return;
        }

        PlayerPrefs.SetString("UserID", newId);
        PlayerPrefs.SetString("UserPW", newPw);

        PlayerPrefs.SetInt("AutoLogin", 0);
        PlayerPrefs.Save();

        Debug.Log($"계정 생성 완료: {newId}");

        ShowLoginPopup();
        messageText.text = "계정이 생성되었습니다.\n로그인 해주세요."; // 안내 메시지
    }

    public void OnClickLogout()
    {
        Debug.Log("[TEST] 로그아웃 및 데이터 초기화");
        PlayerPrefs.SetInt("AutoLogin", 0);
        PlayerPrefs.Save();

        isLoginComplete = false;
        ShowLoginPopup();
        return;
    }

    void ShowTouchToStart()
    {
        loginPopup.SetActive(false);
        createPopup.SetActive(false);
        pressKeyTextGroup.gameObject.SetActive(true);
        pressKeyTextGroup.alpha = 0f;

        isLoginComplete = true;
        StartCoroutine(BlinkTextRoutine());
    }

    IEnumerator BlinkTextRoutine()
    {
        while (isLoginComplete)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1.0f);
            pressKeyTextGroup.alpha = alpha;
            yield return null;
        }
    }

    void Update()
    {
        if (isLoginComplete)
        {
            // [테스트용] 탭(Tab) 키 누르면 데이터 초기화 (로그아웃)
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Debug.Log("[TEST] 로그아웃 및 데이터 초기화");
                PlayerPrefs.SetInt("AutoLogin", 0);
                PlayerPrefs.Save();

                isLoginComplete = false;
                ShowLoginPopup();
                return;
            }

            if (Input.anyKeyDown)
            {
                Debug.Log("Village 씬으로 이동합니다.");
                SceneManager.LoadScene("Main Menu");
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (loginPopup.activeSelf)
            {
                if (loginIdInput.isFocused)
                {
                    loginPwInput.Select();
                }
                else if (loginPwInput.isFocused)
                {
                    loginIdInput.Select();
                }
            }
            else if (createPopup.activeSelf)
            {
                if (createIdInput.isFocused)
                {
                    createPwInput.Select();
                }
                else if (createPwInput.isFocused)
                {
                    createIdInput.Select();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (loginPopup.activeSelf) OnClickLogin();
            else if (createPopup.activeSelf) OnClickConfirmCreate();
        }
    }
}