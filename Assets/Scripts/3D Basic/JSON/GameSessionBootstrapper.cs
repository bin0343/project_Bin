using UnityEngine;
using Unity.Services.Authentication;

[DefaultExecutionOrder(-1000)]
public class GameSessionBootstrapper : MonoBehaviour
{
    private void Start()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("[Bootstrap] 로그인 정보가 없습니다.");

            return;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("[Bootstrap] GameDataManager가 없습니다.");

            return;
        }

        string playerId = AuthenticationService.Instance.PlayerId;

        GameDataManager.Instance.InitializeForPlayer(playerId);

        bool loaded = GameDataManager.Instance.LoadGame();

        if (!loaded)
        {
            GameDataManager.Instance.InitializeNewGame();
        }

        if (loaded)
        {
            GameDataManager.Instance.RestoreTeleportStatesForCurrentScene();
        }

        if (BattleManager.instance != null)
        {
            BattleManager.instance.InitializeParty();

            if (loaded)
            {
                BattleManager.instance.RestoreActiveCharacterIndex(GameDataManager.Instance.SavedActiveCharacterIndex);

                GameDataManager.Instance.RestoreWorldStateForCurrentScene();
            }
        }

        GameDataManager.Instance.MarkSessionReady();

        if (loaded)
        {
            // AP 오프라인 회복 등
            // Load 과정에서 바뀐 데이터를 다시 저장
            GameDataManager.Instance.RequestAutoSave();
        }
        else
        {
            // 신규 유저의 최초 세이브 생성
            GameDataManager.Instance.ForceSave();
        }

        Debug.Log(loaded ? "[Bootstrap] 기존 세이브로 게임 시작": "[Bootstrap] 신규 계정으로 게임 시작");
    }
}