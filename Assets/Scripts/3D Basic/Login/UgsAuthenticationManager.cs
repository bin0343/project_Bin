using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class UgsAuthenticationManager : MonoBehaviour
{
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

        Debug.Log("[UGS] 로그아웃 및 저장된 세션 삭제");
    }

    #endregion
}
