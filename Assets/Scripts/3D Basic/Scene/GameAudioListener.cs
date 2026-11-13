using UnityEngine;

public class GameAudioListener : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null) return;
        }

        if (BattleManager.instance == null) return;

        GameObject activeCharacter = BattleManager.instance.GetActiveCharacter();

        if (activeCharacter == null) return;

        // 거리 기준 = 플레이어
        transform.position = activeCharacter.transform.position;

        // 방향 기준 = 카메라
        transform.rotation = mainCamera.transform.rotation;
    }
}