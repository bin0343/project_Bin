using System;
using System.IO;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using System.Text;

public enum StartupSaveResult
{
    UseLocal,
    DownloadedCloud,
    NoSave,
    Failed
}

public class CloudSaveManager : MonoBehaviour
{
    private const string CloudFileName = "GameSave_v1.json";

    [Header("자동 클라우드 동기화")]
    [SerializeField, Min(1f)]
    private float cloudUploadDelay = 5f;

    private bool cloudUploadDirty;
    private bool isUploading;
    private float cloudUploadTimer;

    [Header("종료 저장")]
    [SerializeField, Min(1f)]
    private float quitCloudTimeout = 5f;

    private bool quitFlushStarted;
    private bool allowQuit;

    private string currentWriteLock;
    private bool hasCloudConflict;

    private void Update()
    {
        if (quitFlushStarted) return;

        if (!cloudUploadDirty || isUploading) return;

        cloudUploadTimer -= Time.unscaledDeltaTime;

        if (cloudUploadTimer <= 0f)
        {
            _ = UploadLatestLocalSaveAsync();
        }
    }

    private async Task<bool> UploadLatestLocalSaveAsync()
    {
        while (isUploading)
        {
            await Task.Delay(50);
        }

        if (GameDataManager.Instance == null) return false;

        string localPath = GameDataManager.Instance.SaveFilePath;

        if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath)) return false;

        isUploading = true;
        cloudUploadDirty = false;

        try
        {
            byte[] fileBytes = File.ReadAllBytes(localPath);

            if (string.IsNullOrEmpty(currentWriteLock))
            {
                // 클라우드에 파일이 아직 없는 최초 저장
                await CloudSaveService.Instance.Files.Player.SaveAsync(CloudFileName, fileBytes);
            }
            else
            {
                // 기존 파일 수정 - 내가 알고 있는 버전과 같은지 검사
                await CloudSaveService.Instance.Files.Player.SaveAsync(
                    CloudFileName,
                    fileBytes,
                    new SaveOptions
                    {
                        WriteLock = currentWriteLock
                    }
                );
            }

            await RefreshWriteLockAsync();

            Debug.Log($"[CloudSave] 자동 업로드 완료 / {fileBytes.Length} bytes");

            return true;
        }
        catch (CloudSaveException e) when (e.Reason == CloudSaveExceptionReason.Conflict)
        {
            // WriteLock 충돌은 자동 재시도하지 않는다.
            hasCloudConflict = true;
            cloudUploadDirty = false;

            Debug.LogError("[CloudSave] 저장 충돌 감지!\n" + "다른 기기 또는 다른 저장 요청에 의해 " + "클라우드 세이브가 변경되었습니다.\n" + "현재 로컬 데이터로 자동 덮어쓰지 않습니다.");

            return false;
        }
        catch (CloudSaveException e)
        {
            // 인터넷 오류, 서비스 오류 등은 나중에 재시도
            Debug.LogError($"[CloudSave] 자동 업로드 실패\n" + $"Reason: {e.Reason}\n" + $"{e}");

            cloudUploadDirty = true;
            cloudUploadTimer = cloudUploadDelay;

            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] 예기치 않은 업로드 오류\n{e}");

            cloudUploadDirty = true;
            cloudUploadTimer = cloudUploadDelay;

            return false;
        }
        finally
        {
            isUploading = false;
        }
    }

    private void RequestCloudUpload()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        if (hasCloudConflict) return;

        if (cloudUploadDirty) return;

        cloudUploadDirty = true;
        cloudUploadTimer = cloudUploadDelay;
    }

    public async Task<bool> InitializeWriteLockAsync()
    {
        try
        {
            var files = await CloudSaveService.Instance.Files.Player.ListAllAsync();

            bool cloudFileExists = false;

            foreach (var file in files)
            {
                if (file.Key == CloudFileName)
                {
                    cloudFileExists = true;
                    break;
                }
            }

            hasCloudConflict = false;

            if (!cloudFileExists)
            {
                currentWriteLock = null;

                Debug.Log("[CloudSave] 기존 클라우드 파일이 없습니다. " + "최초 업로드 상태입니다.");

                return true;
            }

            var metadata = await CloudSaveService.Instance.Files.Player.GetMetadataAsync(CloudFileName);

            currentWriteLock = metadata.WriteLock;

            Debug.Log($"[CloudSave] WriteLock 준비 완료: {currentWriteLock}");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] WriteLock 초기화 실패\n{e}");

            return false;
        }
    }

    private async Task RefreshWriteLockAsync()
    {
        var metadata =
            await CloudSaveService.Instance.Files.Player.GetMetadataAsync(
                CloudFileName
            );

        currentWriteLock = metadata.WriteLock;
    }

    public async Task<StartupSaveResult> ResolveStartupSaveAsync()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("[CloudSave] 로그인되어 있지 않습니다.");
            return StartupSaveResult.Failed;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("[CloudSave] GameDataManager가 없습니다.");
            return StartupSaveResult.Failed;
        }

        try
        {
            string localPath = GameDataManager.Instance.SaveFilePath;

            bool localExists = !string.IsNullOrEmpty(localPath) && File.Exists(localPath);

            var files = await CloudSaveService.Instance.Files.Player.ListAllAsync();

            bool cloudExists = false;

            foreach (var file in files)
            {
                if (file.Key == CloudFileName)
                {
                    cloudExists = true;
                    break;
                }
            }

            // 둘 다 없음 = 진짜 신규 유저
            if (!localExists && !cloudExists)
            {
                Debug.Log("[CloudSave] 로컬/클라우드 세이브가 모두 없습니다.");

                return StartupSaveResult.NoSave;
            }

            // Cloud는 없고 Local만 있음
            if (localExists && !cloudExists)
            {
                Debug.Log("[CloudSave] 로컬 세이브만 존재합니다.");

                return StartupSaveResult.UseLocal;
            }

            byte[] cloudBytes = await CloudSaveService.Instance.Files.Player.LoadBytesAsync(CloudFileName);

            if (cloudBytes == null || cloudBytes.Length == 0)
            {
                Debug.LogError("[CloudSave] 클라우드 세이브가 비어 있습니다.");

                return StartupSaveResult.Failed;
            }

            // Cloud만 있음
            if (!localExists)
            {
                File.WriteAllBytes(localPath, cloudBytes);

                await RefreshWriteLockAsync();

                Debug.Log("[CloudSave] 로컬 세이브가 없어 " + "클라우드 세이브를 사용합니다.");

                return StartupSaveResult.DownloadedCloud;
            }

            // 여기부터 둘 다 존재
            string localJson = File.ReadAllText(localPath);

            string cloudJson = Encoding.UTF8.GetString(cloudBytes);

            GameSaveData localData = JsonUtility.FromJson<GameSaveData>(localJson);

            GameSaveData cloudData = JsonUtility.FromJson<GameSaveData>(cloudJson);

            if (localData == null || cloudData == null)
            {
                Debug.LogError("[CloudSave] 저장 파일 비교 중 JSON 변환에 실패했습니다.");

                return StartupSaveResult.Failed;
            }

            long localTicks = localData.lastSavedUtcTicks;

            long cloudTicks = cloudData.lastSavedUtcTicks;

            Debug.Log($"[CloudSave] 저장 시각 비교\n" + $"Local: {localTicks}\n" + $"Cloud: {cloudTicks}");

            if (cloudTicks > localTicks)
            {
                File.WriteAllBytes(localPath, cloudBytes);

                await RefreshWriteLockAsync();

                Debug.Log("[CloudSave] 클라우드 세이브가 더 최신입니다.");

                return StartupSaveResult.DownloadedCloud;
            }

            Debug.Log("[CloudSave] 로컬 세이브가 같거나 더 최신입니다.");

            return StartupSaveResult.UseLocal;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] 시작 세이브 판정 실패\n{e}");

            return StartupSaveResult.Failed;
        }
    }

    private async Task FlushAndQuitAsync()
    {
        try
        {
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.ForceSave();

                GameDataManager.Instance.MarkQuitSaveHandled();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("[CloudSave] 로그인 상태가 아니므로 " + "로컬 저장 후 종료합니다.");

                return;
            }

            if (hasCloudConflict)
            {
                Debug.LogWarning("[CloudSave] 클라우드 충돌 상태입니다. " + "클라우드를 덮어쓰지 않고 로컬 저장만 보존합니다.");

                return;
            }

            cloudUploadDirty = true;
            cloudUploadTimer = 0f;

            Task<bool> uploadTask = UploadLatestLocalSaveAsync();

            Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(quitCloudTimeout));

            Task completedTask = await Task.WhenAny(uploadTask, timeoutTask);

            if (completedTask == uploadTask)
            {
                bool success = await uploadTask;

                if (success)
                {
                    Debug.Log("[CloudSave] 종료 전 클라우드 저장 완료");
                }
                else
                {
                    Debug.LogWarning("[CloudSave] 종료 전 클라우드 저장에 실패했습니다. " + "로컬 저장은 유지됩니다.");
                }
            }
            else
            {
                Debug.LogWarning($"[CloudSave] 종료 전 업로드가 " + $"{quitCloudTimeout}초 안에 끝나지 않았습니다. " + "로컬 저장을 유지하고 종료합니다.");
            }
        }
        finally
        {
            allowQuit = true;

            Application.Quit();
        }
    }

    private bool HandleWantsToQuit()
    {
#if UNITY_EDITOR
        return true;
#else
    if (allowQuit)
    {
        return true;
    }

    if (quitFlushStarted)
    {
        return false;
    }

    quitFlushStarted = true;

    _ = FlushAndQuitAsync();

    return false;
#endif
    }

    private void OnEnable()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnLocalSaveCompleted += RequestCloudUpload;
        }

        Application.wantsToQuit += HandleWantsToQuit;
    }

    private void OnDisable()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnLocalSaveCompleted -= RequestCloudUpload;
        }

        Application.wantsToQuit -= HandleWantsToQuit;
    }
}