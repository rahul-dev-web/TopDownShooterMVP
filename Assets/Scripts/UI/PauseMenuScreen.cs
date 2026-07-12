/// <summary>
/// PauseMenuScreen - In-game pause menu
/// 
/// Resume button
/// Settings button
/// Quit button
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuScreen : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI pauseText;

    private ScreenManager _screenManager;

    private void Start()
    {
        _screenManager = FindAnyObjectByType<ScreenManager>();

        resumeButton.onClick.AddListener(OnResumeClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        Debug.Log("[PauseMenuScreen] Initialized");
    }

    private void OnResumeClicked()
    {
        Debug.Log("[PauseMenuScreen] Resume button clicked");
        _screenManager.ShowScreen(ScreenManager.ScreenType.Gameplay);
        GameManager.Instance.SetGameState(GameManager.GameState.Playing);
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[PauseMenuScreen] Settings button clicked");
        _screenManager.ShowScreen(ScreenManager.ScreenType.Settings);
    }

    private void OnQuitClicked()
    {
        Debug.Log("[PauseMenuScreen] Quit button clicked");
        GameManager.Instance.SetGameState(GameManager.GameState.Menu);
        _screenManager.ShowScreen(ScreenManager.ScreenType.MainMenu);
    }
}