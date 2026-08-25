/// <summary>
/// SettingsScreen - Settings menu UI
/// 
/// Volume controls
/// Graphics settings
/// Input settings
/// Back button
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsScreen : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private Button backButton;

    private ScreenManager _screenManager;

    private void Start()
    {
        _screenManager = FindAnyObjectByType<ScreenManager>();

        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        backButton.onClick.AddListener(OnBackClicked);

        // Initialize with current values
        LoadSettings();

        Debug.Log("[SettingsScreen] Initialized");
    }

    private void LoadSettings()
    {
        AudioManager audioManager = GameManager.Instance.GetAudioManager();
        
        masterVolumeSlider.value = AudioListener.volume;
        sfxVolumeSlider.value = 0.8f;  // Default
        musicVolumeSlider.value = 0.6f;  // Default

        UpdateVolumeTexts();
    }

    private void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        masterVolumeText.text = $"{value * 100:F0}%";
    }

    private void OnSFXVolumeChanged(float value)
    {
        sfxVolumeText.text = $"{value * 100:F0}%";
    }

    private void OnMusicVolumeChanged(float value)
    {
        musicVolumeText.text = $"{value * 100:F0}%";
    }

    private void UpdateVolumeTexts()
    {
        masterVolumeText.text = $"{masterVolumeSlider.value * 100:F0}%";
        sfxVolumeText.text = $"{sfxVolumeSlider.value * 100:F0}%";
        musicVolumeText.text = $"{musicVolumeSlider.value * 100:F0}%";
    }

    private void OnBackClicked()
    {
        Debug.Log("[SettingsScreen] Back button clicked");
        
        // Determine which screen to go back to
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
        {
            _screenManager.ShowScreen(ScreenManager.ScreenType.Pause);
        }
        else
        {
            _screenManager.ShowScreen(ScreenManager.ScreenType.MainMenu);
        }
    }
}