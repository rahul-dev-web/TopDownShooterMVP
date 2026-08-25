using UnityEngine;

/// <summary>
/// Handles actions exposed by the match result screen.
/// Keeps UI button callbacks separate from MatchManager match-state authority.
/// </summary>
public class MatchResultActions : MonoBehaviour
{
    [Header("Optional Scene Names")]
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Behaviour")]
    [SerializeField] private bool restartByReloadingScene = true;

    /// <summary>
    /// Starts a fresh local match. Scene reload is preferred because it resets
    /// scene-owned players, spawn points, UI and transient combat state together.
    /// </summary>
    public void PlayAgain()
    {
        Time.timeScale = 1f;

        if (restartByReloadingScene && !string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetGameState(GameManager.GameState.Loading);

            UnityEngine.SceneManagement.SceneManager.LoadScene(gameplaySceneName);
            return;
        }

        MatchManager matchManager = FindFirstObjectByType<MatchManager>();
        if (matchManager != null)
        {
            matchManager.ResetMatch();
            matchManager.StartMatch();
        }
    }

    /// <summary>
    /// Leaves the current match and returns to the main menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        MatchManager matchManager = FindFirstObjectByType<MatchManager>();
        if (matchManager != null)
            matchManager.ResetMatch();

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetGameState(GameManager.GameState.Menu);

            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("[MatchResultActions] Main menu scene name is empty.");
        }
    }
}
