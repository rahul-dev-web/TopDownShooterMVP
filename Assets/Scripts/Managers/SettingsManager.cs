/// <summary>
/// SettingsManager - Game Settings Management
/// यह manager को load करो एक अलग file में
/// </summary>
 
using UnityEngine;
 
public class SettingsManager : MonoBehaviour
{
    private GameSettings _gameSettings;
 
    private void OnEnable()
    {
        Initialize();
    }
 
    private void Initialize()
    {
        Debug.Log("[SettingsManager] Initializing...");
 
        // SaveManager से settings load करो
        _gameSettings = GameManager.Instance.GetSaveManager().LoadSettings();
 
        // Settings को apply करो
        ApplySettings();
 
        Debug.Log("[SettingsManager] ✓ Initialized");
    }
 
    private void ApplySettings()
    {
        // Audio settings
        GameManager.Instance.GetAudioManager().SetMasterVolume(_gameSettings.masterVolume);
        GameManager.Instance.GetAudioManager().SetMusicVolume(_gameSettings.musicVolume);
        GameManager.Instance.GetAudioManager().SetSFXVolume(_gameSettings.sfxVolume);
 
        // Input settings
        GameManager.Instance.GetInputManager().SetMouseSensitivity(_gameSettings.mouseSensitivity);
        GameManager.Instance.GetInputManager().SetJoystickDeadzone(_gameSettings.joystickDeadzone);
 
        // Game settings
        Application.targetFrameRate = _gameSettings.targetFPS;
    }
 
    public void UpdateSetting(string settingName, object value)
    {
        // Dynamic setting update करो
        Debug.Log($"[SettingsManager] Updated {settingName} = {value}");
    }
 
    public GameSettings GetCurrentSettings() => _gameSettings;
 
    public void SaveSettingsToFile()
    {
        GameManager.Instance.GetSaveManager().SaveSettings(_gameSettings);
    }
 
    public void ResetSettingsToDefault()
    {
        _gameSettings = GameSettings.GetDefault();
        ApplySettings();
        SaveSettingsToFile();
    }
}