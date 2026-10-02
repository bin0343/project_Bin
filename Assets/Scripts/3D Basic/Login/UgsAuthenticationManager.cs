using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;

public class UgsAuthenticationManager : MonoBehaviour
{
    private enum PlayerAccountOperation
    {
        None,
        SignIn,
        Link
    }

    private PlayerAccountOperation currentPlayerAccountOperation = PlayerAccountOperation.None;

    private TaskCompletionSource<bool> playerAccountSignInTask;
    private bool playerAccountEventsRegistered;

    [Header("브라우저 로그인")]
    [SerializeField, Min(0.1f)]
    private float browserReturnGraceSeconds = 1f;

    private bool browserLostFocus;
    private Coroutine browserReturnCheckCoroutine;

    public string LastErrorMessage { get; private set; } = string.Empty;

    public bool IsInitialized
    {
        get
        {
            return UnityServices.State == ServicesInitializationState.Initialized;
        }
    }

    public bool IsSignedIn
    {
        get
        {
            return IsInitialized && AuthenticationService.Instance.IsSignedIn;
        }
    }

    public bool HasSavedSession
    {
        get
        {
            return IsInitialized && AuthenticationService.Instance.SessionTokenExists;
        }
    }

    public string PlayerId
    {
        get
        {
            if (!IsSignedIn) return string.Empty;

            return AuthenticationService.Instance.PlayerId;
        }
    }

    #region UGS 로그인
    private async Task<bool> SignInWithUnityAuthenticationAsync()
    {
        try
        {
            string accessToken = PlayerAccountService.Instance.AccessToken;

            await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);

            Debug.Log($"[UGS] Player Accounts 로그인 성공 / " + $"Player ID: {AuthenticationService.Instance.PlayerId}");

            return true;
        }
        catch (AuthenticationException exception)
        {
            LastErrorMessage = "계정 인증에 실패했습니다.";

            Debug.LogException(exception);
            return false;
        }
        catch (RequestFailedException exception)
        {
            LastErrorMessage = "서버 요청에 실패했습니다.\n네트워크 상태를 확인해 주세요.";

            Debug.LogException(exception);
            return false;
        }
        catch (Exception exception)
        {
            LastErrorMessage = "계정 로그인 중 오류가 발생했습니다.";

            Debug.LogException(exception);
            return false;
        }
    }
    #endregion

    #region UGS 초기화
    public async Task<bool> InitializeAsync()
    {
        LastErrorMessage = string.Empty;

        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            bool success = UnityServices.State == ServicesInitializationState.Initialized;

            if (success)
            {
                RegisterPlayerAccountEvents();

                Debug.Log("[UGS] Unity Services 초기화 성공");
            }

            return success;
        }
        catch (Exception exception)
        {
            LastErrorMessage = "서버 초기화에 실패했습니다.\n네트워크 상태를 확인해 주세요.";

            Debug.LogException(exception);
            return false;
        }
    }

    #endregion

    #region 자동 로그인
    public async Task<bool> TryRestoreSessionAsync()
    {
        LastErrorMessage = string.Empty;

        if (!IsInitialized)
        {
            LastErrorMessage = "UGS가 아직 초기화되지 않았습니다.";
            return false;
        }

        if (!AuthenticationService.Instance.SessionTokenExists)
        {
            Debug.Log("[UGS] 저장된 로그인 세션 없음");
            return false;
        }

        if (AuthenticationService.Instance.IsSignedIn)
        {
            return true;
        }

        Debug.Log("[UGS] 저장된 세션으로 자동 로그인 시도");

        return await SignInAnonymouslyInternalAsync("자동 로그인");
    }

    #endregion

    #region 게스트 로그인
    public async Task<bool> SignInAsGuestAsync()
    {
        LastErrorMessage = string.Empty;

        if (!IsInitialized)
        {
            LastErrorMessage = "UGS가 아직 초기화되지 않았습니다.";
            return false;
        }

        if (AuthenticationService.Instance.IsSignedIn)
        {
            return true;
        }

        return await SignInAnonymouslyInternalAsync("게스트 로그인");
    }

    private async Task<bool> SignInAnonymouslyInternalAsync(string loginType)
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log($"[UGS] {loginType} 성공 / " + $"Player ID: {AuthenticationService.Instance.PlayerId}");

            return true;
        }
        catch (AuthenticationException exception)
        {
            LastErrorMessage = "로그인 인증에 실패했습니다.";

            Debug.LogException(exception);
            return false;
        }
        catch (RequestFailedException exception)
        {
            LastErrorMessage = "서버 요청에 실패했습니다.\n네트워크 상태를 확인해 주세요.";

            Debug.LogException(exception);
            return false;
        }
        catch (Exception exception)
        {
            LastErrorMessage = "로그인 중 알 수 없는 오류가 발생했습니다.";

            Debug.LogException(exception);
            return false;
        }
    }

    #endregion

    #region 구글 로그인
    private void RegisterPlayerAccountEvents()
    {
        if (playerAccountEventsRegistered)
        {
            return;
        }

        PlayerAccountService.Instance.SignedIn += OnPlayerAccountSignedIn;

        PlayerAccountService.Instance.SignInFailed += OnPlayerAccountSignInFailed;

        playerAccountEventsRegistered = true;
    }

    //브라우저 로그인 시작
    private async Task StartPlayerAccountBrowserFlowAsync()
    {
        try
        {
            await PlayerAccountService.Instance.StartSignInAsync();
        }
        catch (PlayerAccountsException exception)
        {
            Debug.LogException(exception);

            if (currentPlayerAccountOperation != PlayerAccountOperation.None)
            {
                CompletePlayerAccountOperation(false, "로그인에 실패했습니다.");
            }
        }
        catch (RequestFailedException exception)
        {
            Debug.LogException(exception);

            if (currentPlayerAccountOperation != PlayerAccountOperation.None)
            {
                CompletePlayerAccountOperation(false, "로그인 서버에 연결하지 못했습니다.");
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            if (currentPlayerAccountOperation != PlayerAccountOperation.None)
            {
                CompletePlayerAccountOperation(false, "로그인에 실패했습니다.");
            }
        }
    }

    private async void OnPlayerAccountSignedIn()
    {
        Debug.Log("[Player Accounts] 브라우저 인증 완료");

        bool success = false;

        switch (currentPlayerAccountOperation)
        {
            case PlayerAccountOperation.SignIn:

                success = await SignInWithUnityAuthenticationAsync();

                break;


            case PlayerAccountOperation.Link:

                success = await LinkCurrentPlayerWithUnityAsync();

                break;
        }

        CompletePlayerAccountOperation(success);
    }

    private void OnPlayerAccountSignInFailed(RequestFailedException exception)
    {
        if (currentPlayerAccountOperation == PlayerAccountOperation.None)
        {
            return;
        }

        Debug.LogWarning($"[Player Accounts] 로그인 실패: {exception.Message}");

        CompletePlayerAccountOperation(false, "로그인에 실패했습니다.");
    }

    public async Task<bool> SignInWithPlayerAccountAsync()
    {
        LastErrorMessage = string.Empty;

        if (!IsInitialized)
        {
            LastErrorMessage = "UGS가 아직 초기화되지 않았습니다.";

            return false;
        }

        if (AuthenticationService.Instance.IsSignedIn)
        {
            return true;
        }

        RegisterPlayerAccountEvents();

        // Player Accounts 쪽에는 이미 로그인돼 있는 경우
        if (PlayerAccountService.Instance.IsSignedIn)
        {
            return await SignInWithUnityAuthenticationAsync();
        }

        playerAccountSignInTask = new TaskCompletionSource<bool>();

        currentPlayerAccountOperation = PlayerAccountOperation.SignIn;

        Debug.Log("[Player Accounts] 브라우저 로그인 시작");

        _ = StartPlayerAccountBrowserFlowAsync();

        return await playerAccountSignInTask.Task;
    }

    //로그인 여부 확인
    private void CompletePlayerAccountOperation(bool success, string errorMessage = null)
    {
        if (!success && !string.IsNullOrEmpty(errorMessage))
        {
            LastErrorMessage = errorMessage;
        }

        currentPlayerAccountOperation = PlayerAccountOperation.None;

        browserLostFocus = false;

        if (browserReturnCheckCoroutine != null)
        {
            StopCoroutine(browserReturnCheckCoroutine);
            browserReturnCheckCoroutine = null;
        }

        TaskCompletionSource<bool> completionSource = playerAccountSignInTask;

        playerAccountSignInTask = null;

        completionSource?.TrySetResult(success);
    }

    //포커스 감지
    private void OnApplicationFocus(bool hasFocus)
    {
        // Player Accounts 작업 중이 아니면 무시
        if (currentPlayerAccountOperation == PlayerAccountOperation.None)
        {
            return;
        }

        if (playerAccountSignInTask == null)
        {
            return;
        }

        // 브라우저가 열리면서 게임이 포커스를 잃음
        if (!hasFocus)
        {
            browserLostFocus = true;
            return;
        }

        // 실제로 브라우저로 나갔다 온 경우만 검사
        if (!browserLostFocus)
        {
            return;
        }

        browserLostFocus = false;

        if (browserReturnCheckCoroutine != null)
        {
            StopCoroutine(browserReturnCheckCoroutine);
        }

        browserReturnCheckCoroutine = StartCoroutine(CheckBrowserLoginResultRoutine());
    }

    //브라우저 닫았는지 검사
    private IEnumerator CheckBrowserLoginResultRoutine()
    {
        yield return new WaitForSecondsRealtime(browserReturnGraceSeconds);

        browserReturnCheckCoroutine = null;

        // 기다리는 동안 정상 로그인 완료
        if (currentPlayerAccountOperation == PlayerAccountOperation.None)
        {
            yield break;
        }

        if (playerAccountSignInTask == null)
        {
            yield break;
        }

        // Player Accounts 로그인이 정상 완료됨
        if (PlayerAccountService.Instance.IsSignedIn)
        {
            yield break;
        }

        Debug.LogWarning("[Player Accounts] " + "브라우저가 닫혔지만 로그인이 완료되지 않았습니다.");

        CompletePlayerAccountOperation(false, "로그인에 실패했습니다.");
    }

    #endregion

    #region 로그아웃
    public void SignOutAndClearSession()
    {
        if (!IsInitialized)
        {
            return;
        }

        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut(true);
        }
        else if (AuthenticationService.Instance.SessionTokenExists)
        {
            AuthenticationService.Instance.ClearSessionToken();
        }

        if (PlayerAccountService.Instance.IsSignedIn)
        {
            PlayerAccountService.Instance.SignOut();
        }

        Debug.Log("[UGS] 로그아웃 및 저장된 세션 삭제");
    }

    #endregion

    #region 계정 연동
    //계정 정보 확인
    public async Task<bool> HasLinkedAccountAsync()
    {
        if (!IsSignedIn)
        {
            return false;
        }

        try
        {
            PlayerInfo playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();

            if (playerInfo == null || playerInfo.Identities == null)
            {
                return false;
            }

            return playerInfo.Identities.Count > 0;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            return false;
        }
    }

    private async Task<bool> LinkCurrentPlayerWithUnityAsync()
    {
        try
        {
            string accessToken = PlayerAccountService.Instance.AccessToken;

            await AuthenticationService.Instance.LinkWithUnityAsync(accessToken);

            Debug.Log($"[UGS] 계정 연동 성공 / " + $"Player ID: {AuthenticationService.Instance.PlayerId}");

            return true;
        }
        catch (AuthenticationException exception) when (exception.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            LastErrorMessage = "이 Google 계정은 이미 다른 게임 데이터와 연결되어 있습니다.";

            Debug.LogWarning(LastErrorMessage);
            return false;
        }
        catch (AuthenticationException exception)
        {
            LastErrorMessage = "계정 연동에 실패했습니다.";

            Debug.LogException(exception);
            return false;
        }
        catch (RequestFailedException exception)
        {
            LastErrorMessage = "서버 요청에 실패했습니다.";

            Debug.LogException(exception);
            return false;
        }
        catch (Exception exception)
        {
            LastErrorMessage = "계정 연동 중 오류가 발생했습니다.";

            Debug.LogException(exception);
            return false;
        }
    }

    public async Task<bool> LinkPlayerAccountAsync()
    {
        LastErrorMessage = string.Empty;

        if (!IsSignedIn)
        {
            LastErrorMessage = "로그인된 플레이어가 없습니다.";

            return false;
        }

        bool alreadyLinked = await HasLinkedAccountAsync();

        if (alreadyLinked)
        {
            LastErrorMessage = "이미 외부 계정과 연동되어 있습니다.";

            return false;
        }

        RegisterPlayerAccountEvents();

        // 브라우저 로그인 상태가 남아 있다면 새로 선택하게 함
        if (PlayerAccountService.Instance.IsSignedIn)
        {
            PlayerAccountService.Instance.SignOut();
        }

        playerAccountSignInTask = new TaskCompletionSource<bool>();

        currentPlayerAccountOperation = PlayerAccountOperation.Link;

        Debug.Log("[Player Accounts] 계정 연동용 로그인 시작");

        _ = StartPlayerAccountBrowserFlowAsync();

        return await playerAccountSignInTask.Task;
    }

    #endregion

    private void OnDestroy()
    {
        if (!playerAccountEventsRegistered)
        {
            return;
        }

        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            return;
        }

        PlayerAccountService.Instance.SignedIn -= OnPlayerAccountSignedIn;
        PlayerAccountService.Instance.SignInFailed -= OnPlayerAccountSignInFailed;

        playerAccountEventsRegistered = false;
    }
}
