using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 꼭 필요한 네임스페이스

public class SceneLoader : MonoBehaviour
{
    public void LoadMiniGame()
    {
        SceneManager.LoadScene("MiniGame_Run");
    }
}