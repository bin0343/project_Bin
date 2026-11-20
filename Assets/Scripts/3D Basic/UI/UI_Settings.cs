using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class UI_Settings : MonoBehaviour
{
    [Header("화면 설정")]
    [SerializeField] private Dropdown screenModeDropdown;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Toggle vSyncToggle;

    [Header("오디오 설정")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider uiVolumeSlider;

    private List<Vector2Int> resolutionList = new List<Vector2Int>();

    private void Awake()
    {
        SetupScreenModeDropdown();
        SetupResolutionDropdown();
        SetupQualityDropdown();

        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        uiVolumeSlider.onValueChanged.AddListener(OnUIVolumeChanged);

        screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
    }

    private void OnEnable()
    {
        RefreshCurrentSettings();
        RefreshAudioSettings();
    }

    private void SetupScreenModeDropdown()
    {
        screenModeDropdown.ClearOptions();

        List<string> options = new List<string>()
        {
            "창 모드",
            "테두리 없는 전체화면",
            "전체화면"
        };

        screenModeDropdown.AddOptions(options);
    }

    private void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        resolutionList.Clear();

        Resolution[] resolutions = Screen.resolutions;

        foreach (Resolution resolution in resolutions)
        {
            Vector2Int size = new Vector2Int(resolution.width, resolution.height);

            if (!resolutionList.Contains(size))
            {
                resolutionList.Add(size);
            }
        }

        List<string> options = new List<string>();

        foreach (Vector2Int resolution in resolutionList)
        {
            options.Add($"{resolution.x} x {resolution.y}");
        }

        resolutionDropdown.AddOptions(options);
    }

    private void SetupQualityDropdown()
    {
        qualityDropdown.ClearOptions();

        List<string> options = new List<string>(QualitySettings.names);

        qualityDropdown.AddOptions(options);
    }

    private void RefreshCurrentSettings()
    {
        if (GameSettingsManager.Instance == null) return;

        GameSettingsManager settings =
            GameSettingsManager.Instance;

        screenModeDropdown.SetValueWithoutNotify(GetScreenModeIndex(settings.ScreenMode));

        for (int i = 0; i < resolutionList.Count; i++)
        {
            if (resolutionList[i].x == settings.ResolutionWidth && resolutionList[i].y == settings.ResolutionHeight)
            {
                resolutionDropdown.SetValueWithoutNotify(i);
                break;
            }
        }

        qualityDropdown.SetValueWithoutNotify(settings.QualityLevel);
        vSyncToggle.SetIsOnWithoutNotify(settings.VSyncEnabled);
    }

    private void RefreshAudioSettings()
    {
        if (GameSettingsManager.Instance == null) return;

        masterVolumeSlider.SetValueWithoutNotify(GameSettingsManager.Instance.MasterVolume);
        bgmVolumeSlider.SetValueWithoutNotify(GameSettingsManager.Instance.BGMVolume);
        sfxVolumeSlider.SetValueWithoutNotify(GameSettingsManager.Instance.SFXVolume);
        uiVolumeSlider.SetValueWithoutNotify(GameSettingsManager.Instance.UIVolume);
    }

    private void OnScreenModeChanged(int index)
    {
        GameSettingsManager.Instance?.SetScreenMode(GetFullScreenMode(index));
    }

    private void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= resolutionList.Count) return;

        Vector2Int resolution = resolutionList[index];

        GameSettingsManager.Instance?.SetResolution(resolution.x, resolution.y);
    }

    private void OnQualityChanged(int index)
    {
        GameSettingsManager.Instance?.SetQuality(index);
    }

    private void OnVSyncChanged(bool enabled)
    {
        GameSettingsManager.Instance?.SetVSync(enabled);
    }

    private FullScreenMode GetFullScreenMode(int index)
    {
        switch (index)
        {
            case 0:
                return FullScreenMode.Windowed;

            case 1:
                return FullScreenMode.FullScreenWindow;

            case 2:
                return FullScreenMode.ExclusiveFullScreen;
        }

        return FullScreenMode.FullScreenWindow;
    }

    private int GetScreenModeIndex(FullScreenMode mode)
    {
        switch (mode)
        {
            case FullScreenMode.Windowed:
                return 0;

            case FullScreenMode.FullScreenWindow:
                return 1;

            case FullScreenMode.ExclusiveFullScreen:
                return 2;

            default:
                return 1;
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        GameSettingsManager.Instance?.SetMasterVolume(value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        GameSettingsManager.Instance?.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        GameSettingsManager.Instance?.SetSFXVolume(value);
    }

    private void OnUIVolumeChanged(float value)
    {
        GameSettingsManager.Instance?.SetUIVolume(value);
    }
}