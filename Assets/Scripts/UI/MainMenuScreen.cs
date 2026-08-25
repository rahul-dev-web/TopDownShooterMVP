/// <summary>
/// MainMenuScreen - Main menu UI logic
/// 
/// Play button
/// Settings button
/// Quit button
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuScreen : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI titleText;

    private ScreenManager _screenManager;

    private void Start()
    {
        _screenManager = FindAnyObjectByType<ScreenManager>();

        playButton.onClick.AddListener(OnPlayClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        Debug.Log("[MainMenuScreen] Initialized");
    }

    private void OnPlayClicked()
    {
        Debug.Log("[MainMenuScreen] Play button clicked");
        _screenManager.ShowScreen(ScreenManager.ScreenType.Gameplay);
        GameManager.Instance.SetGameState(GameManager.GameState.Playing);
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[MainMenuScreen] Settings button clicked");
        _screenManager.ShowScreen(ScreenManager.ScreenType.Settings);
    }

    private void OnQuitClicked()
    {
        Debug.Log("[MainMenuScreen] Quit button clicked");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}