using UnityEngine;
using UnityEngine.Audio;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    public int ResolutionWidth { get; private set; }
    public int ResolutionHeight { get; private set; }
    public FullScreenMode ScreenMode { get; private set; }
    public int QualityLevel { get; private set; }
    public bool VSyncEnabled { get; private set; }

    private const string RESOLUTION_WIDTH_KEY = "Setting_ResolutionWidth";
    private const string RESOLUTION_HEIGHT_KEY = "Setting_ResolutionHeight";
    private const string SCREEN_MODE_KEY = "Setting_ScreenMode";
    private const string QUALITY_KEY = "Setting_Quality";
    private const string VSYNC_KEY = "Setting_VSync";

    public float MasterVolume { get; private set; } = 1f;
    public float BGMVolume { get; private set; } = 1f;
    public float SFXVolume { get; private set; } = 1f;
    public float UIVolume { get; private set; } = 1f;

    private const string MASTER_KEY = "Setting_MasterVolume";
    private const string BGM_KEY = "Setting_BGMVolume";
    private const string SFX_KEY = "Setting_SFXVolume";
    private const string UI_KEY = "Setting_UIVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAudioSettings();
        LoadDisplaySettings();
    }

    private void Start()
    {
        ApplyAudioSettings();
        ApplyDisplaySettings();
    }

    private void LoadAudioSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        BGMVolume = PlayerPrefs.GetFloat(BGM_KEY, 1f);
        SFXVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);
        UIVolume = PlayerPrefs.GetFloat(UI_KEY, 1f);
    }

    private void ApplyAudioSettings()
    {
        SetMixerVolume("MasterVolume", MasterVolume);
        SetMixerVolume("BGMVolume", BGMVolume);
        SetMixerVolume("SFXVolume", SFXVolume);
        SetMixerVolume("UIVolume", UIVolume);
    }

    private void LoadDisplaySettings()
    {
        ResolutionWidth = PlayerPrefs.GetInt(RESOLUTION_WIDTH_KEY, Screen.width);
        ResolutionHeight = PlayerPrefs.GetInt(RESOLUTION_HEIGHT_KEY, Screen.height);

        ScreenMode = (FullScreenMode)PlayerPrefs.GetInt(SCREEN_MODE_KEY, (int)Screen.fullScreenMode);

        QualityLevel = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());

        VSyncEnabled = PlayerPrefs.GetInt(VSYNC_KEY, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
    }

    private void ApplyDisplaySettings()
    {
        int maxQualityIndex = QualitySettings.names.Length - 1;

        QualityLevel = Mathf.Clamp(QualityLevel, 0, maxQualityIndex);

        QualitySettings.SetQualityLevel(QualityLevel, true);

        // 2. VSync
        QualitySettings.vSyncCount = VSyncEnabled ? 1 : 0;

        // 3. 화면 모드 + 해상도
        Screen.SetResolution(ResolutionWidth, ResolutionHeight, ScreenMode);
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = value;
        SetMixerVolume("MasterVolume", value);

        PlayerPrefs.SetFloat(MASTER_KEY, value);
    }

    public void SetBGMVolume(float value)
    {
        BGMVolume = value;
        SetMixerVolume("BGMVolume", value);

        PlayerPrefs.SetFloat(BGM_KEY, value);
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = value;
        SetMixerVolume("SFXVolume", value);

        PlayerPrefs.SetFloat(SFX_KEY, value);
    }

    public void SetUIVolume(float value)
    {
        UIVolume = value;
        SetMixerVolume("UIVolume", value);

        PlayerPrefs.SetFloat(UI_KEY, value);
    }

    private void SetMixerVolume(string parameterName, float value)
    {
        float decibel;

        if (value <= 0.0001f)
        {
            decibel = -80f;
        }
        else
        {
            decibel = Mathf.Log10(value) * 20f;
        }

        bool success = audioMixer.SetFloat(parameterName, decibel);

        if (!success)
        {
            Debug.LogWarning($"[Audio Settings] AudioMixer Parameter를 찾지 못했습니다: {parameterName}");
        }
    }

    public void SetScreenMode(FullScreenMode mode)
    {
        ScreenMode = mode;

        Screen.SetResolution(ResolutionWidth, ResolutionHeight, ScreenMode);

        PlayerPrefs.SetInt(SCREEN_MODE_KEY, (int)ScreenMode);
    }

    public void SetResolution(int width, int height)
    {
        ResolutionWidth = width;
        ResolutionHeight = height;

        Screen.SetResolution(ResolutionWidth, ResolutionHeight, ScreenMode);

        PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, ResolutionWidth);
        PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, ResolutionHeight);
    }

    public void SetQuality(int qualityIndex)
    {
        QualityLevel = qualityIndex;

        QualitySettings.SetQualityLevel(QualityLevel, true);

        PlayerPrefs.SetInt(QUALITY_KEY, QualityLevel);

        QualitySettings.vSyncCount = VSyncEnabled ? 1 : 0;
    }

    public void SetVSync(bool enabled)
    {
        VSyncEnabled = enabled;

        QualitySettings.vSyncCount = enabled ? 1 : 0;

        PlayerPrefs.SetInt(VSYNC_KEY, enabled ? 1 : 0);
    }

    public void SaveSettings()
    {
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        SaveSettings();
    }
}