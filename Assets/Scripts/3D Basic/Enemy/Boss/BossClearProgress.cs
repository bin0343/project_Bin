using UnityEngine;

public static class BossClearProgress
{
    private const string KeyPrefix = "BossCleared_";

    public static bool IsCleared(string bossID)
    {
        if (string.IsNullOrWhiteSpace(bossID)) return false;

        string saveKey = KeyPrefix + bossID;

        return PlayerPrefs.GetInt(saveKey, 0) == 1;
    }

    public static void MarkCleared(string bossID)
    {
        if (string.IsNullOrWhiteSpace(bossID))
        {
            Debug.LogError("[BossClearProgress] Boss ID가 비어 있어 " + "클리어 상태를 저장할 수 없습니다.");

            return;
        }

        string saveKey = KeyPrefix + bossID;

        PlayerPrefs.SetInt(saveKey, 1);
        PlayerPrefs.Save();

        Debug.Log($"[BossClearProgress] 최초 클리어 저장: {bossID}");
    }

    public static void ResetClear(string bossID)
    {
        if (string.IsNullOrWhiteSpace(bossID)) return;

        string saveKey = KeyPrefix + bossID;

        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();

        Debug.Log($"[BossClearProgress] 클리어 기록 초기화: {bossID}");
    }
}
