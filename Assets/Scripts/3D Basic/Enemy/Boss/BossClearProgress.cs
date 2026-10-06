using UnityEngine;

public static class BossClearProgress
{
    public static bool IsCleared(string bossID)
    {
        if (string.IsNullOrWhiteSpace(bossID)) return false;

        if (GameDataManager.Instance == null) return false;

        return GameDataManager.Instance.IsBossCleared(bossID);
    }

    public static void MarkCleared(string bossID)
    {
        if (string.IsNullOrWhiteSpace(bossID))
        {
            Debug.LogError("[BossClearProgress] " + "Boss ID가 비어 있습니다.");

            return;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("[BossClearProgress] " + "GameDataManager가 없습니다.");

            return;
        }

        GameDataManager.Instance.MarkBossCleared(bossID);
    }

    public static void ResetClear(string bossID)
    {
        if (string.IsNullOrWhiteSpace(bossID)) return;

        GameDataManager.Instance?.ResetBossClear(bossID);
    }
}