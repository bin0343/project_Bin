using UnityEngine;

public class TeleportPoint3D : Interactable
{
    [Header("텔레포트 정보 설정")]
    public string teleportName = "초보자 마을 공중전화";
    [TextArea] public string teleportDesc = "마을로 빠르게 이동할 수 있는 공중전화입니다.";

    [Header("현재 상태")]
    public bool isActivated = false; // 시작할 때는 비활성화(false) 상태

    protected override void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (isActivated)
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.RestorePartyAtTeleport();
            }

            return;
        }

        base.OnTriggerEnter(other);

        if (interactionText != null)
        {
            interactionText.text = $"{interactionKey} : 활성화";
        }
    }

    protected override void OpenMenu()
    {
        if (!isActivated)
        {
            isActivated = true;
            Debug.Log($"{teleportName} 워프 포인트가 활성화되었습니다!");

            if (LocalMapTeleportManager.instance != null)
                LocalMapTeleportManager.instance.RefreshTeleportMarkers();

            if (MiniMapTeleportManager.instance != null)
                MiniMapTeleportManager.instance.RefreshMiniMapTeleports();

            isPlayerInRange = false;

            if (interactionPromptUI != null) interactionPromptUI.SetActive(false);
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.RestorePartyAtTeleport();
        }
    }
}