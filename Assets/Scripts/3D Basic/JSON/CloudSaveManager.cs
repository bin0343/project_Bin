using System;
using System.IO;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;

public class CloudSaveManager : MonoBehaviour
{
    private const string CloudFileName = "GameSave_v1.json";

    [ContextMenu("Test Upload Save To Cloud")]
    public async void TestUploadSaveToCloud()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("[CloudSave] 로그인되어 있지 않습니다.");
            return;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("[CloudSave] GameDataManager가 없습니다.");
            return;
        }

        // 가장 최신 상태를 먼저 로컬 JSON으로 저장
        GameDataManager.Instance.ForceSave();

        string localPath = GameDataManager.Instance.SaveFilePath;

        if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
        {
            Debug.LogError($"[CloudSave] 로컬 세이브 파일을 찾을 수 없습니다.\n{localPath}");
            return;
        }

        try
        {
            byte[] fileBytes = File.ReadAllBytes(localPath);

            await CloudSaveService.Instance.Files.Player.SaveAsync(CloudFileName, fileBytes);

            Debug.Log($"[CloudSave] 업로드 성공!\n" + $"PlayerId: {AuthenticationService.Instance.PlayerId}\n" + $"File: {CloudFileName}\n" + $"Size: {fileBytes.Length} bytes");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] 업로드 실패\n{e}");
        }
    }
}