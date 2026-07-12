/// <summary>
/// GameOverScreen - Game over UI logic
/// 
/// Shows stats
/// Restart button
/// Main menu button
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    private ScreenManager _screenManager;

    private void Start()
    {
        _screenManager = FindObjectOfType<ScreenManager>();

        restartButton.onClick.AddListener(OnRestartClicked);
        menuButton.onClick.AddListener(OnMenuClicked);

        ScreenManager.OnScreenChanged += HandleScreenChanged;
    }

    private void HandleScreenChanged(ScreenManager.ScreenType screenType)
    {
        if (screenType == ScreenManager.ScreenType.GameOver)
        {
            UpdateStats();
        }
    }

    private void UpdateStats()
    {
        int kills = GameManager.Instance.GetPlayerKills();
        int deaths = GameManager.Instance.GetPlayerDeaths();
        float kdRatio = deaths > 0 ? (float)kills / deaths : kills;

        statsText.text = $"Kills: {kills}\nDeaths: {deaths}\nK/D Ratio: {kdRatio:F2}";
    }

    private void OnRestartClicked()
    {
        Debug.Log("[GameOverScreen] Restart button clicked");
        GameManager.Instance.ResetStats();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    private void OnMenuClicked()
    {
        Debug.Log("[GameOverScreen] Menu button clicked");
        GameManager.Instance.ResetStats();
        _screenManager.ShowScreen(ScreenManager.ScreenType.MainMenu);
    }

    private void OnDestroy()
    {
        ScreenManager.OnScreenChanged -= HandleScreenChanged;
    }
}