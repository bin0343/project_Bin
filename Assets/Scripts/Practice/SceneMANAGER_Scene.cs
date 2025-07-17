using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class SceneMANAGER : MonoBehaviour
{
    public SCENE CurrentScene;
    public SCENE Scene;
    public SCENE PreviousScene;

    public void ChangeScene(SCENE _e)       
    {
        if (Scene == _e) return;

        PreviousScene = CurrentScene;
        CurrentScene = _e;
        Scene = _e;

        /*SceneManager.LoadScene((int)Scene);

        return;*/ //여기까지 해도 로딩하는데 문제 없음.

        /*public void ChangeScene(SCENE _e, bool _Loading = false)
        {
            if (Scene == _e) return;

            Scene = _e;

            if (_Loading)
            {
                SceneManager.LoadScene((int)SCENE.LOADING);
                return;
            }
        }*/


        switch (_e)         //서버 데이터 때문에(리셋이 필요할 때 사용)
        {
            case SCENE.TITLE:
                string Name = Shared.SceneMANAGER.GetPlayerPrefsStringKey("PlayerName");
                if (!string.IsNullOrEmpty(Name))
                    StartCoroutine(AutoLoading(SCENE.LOGIN));
                break;
            case SCENE.LOGIN:
                string username = Shared.SceneMANAGER.GetPlayerPrefsStringKey("PlayerName");
                if (!string.IsNullOrEmpty(username) && username.Length > 2)
                {
                    StartCoroutine(AutoLoading(SCENE.LOADING));
                }
                break;
            case SCENE.LOADING:
                if (Shared.SceneMANAGER.PreviousScene == SCENE.LOGIN || Shared.SceneMANAGER.PreviousScene == SCENE.BATTLE)
                {
                    StartCoroutine(Loading(SCENE.LOBBY));
                }
                if (Shared.SceneMANAGER.PreviousScene == SCENE.LOBBY)
                {
                    StartCoroutine(Loading(SCENE.BATTLE));
                }
                break;
            case SCENE.LOBBY:
                break;
            case SCENE.BATTLE:
                break;
            case SCENE.END:
                break;
        }

        SceneManager.LoadScene((int)Scene);
    }

    IEnumerator Loading(SCENE NextScene)
    {
        yield return new WaitForSeconds(10f);
        Shared.SceneMANAGER.ChangeScene(NextScene);
    }

    IEnumerator AutoLoading(SCENE NextScene)
    {
        yield return new WaitForSeconds(2f);
        Shared.SceneMANAGER.ChangeScene(NextScene);
    }
}
