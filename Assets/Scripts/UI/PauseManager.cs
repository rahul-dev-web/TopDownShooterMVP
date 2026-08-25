/// <summary>
/// PauseManager - Handles game pausing
/// 
/// Pause/Resume functionality
/// Time scale management
/// </summary>

using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private bool _isPaused = false;

    private void Update()
    {
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Playing)
            return;

        // Press ESC to pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (_isPaused)
            Resume();
        else
            Pause();
    }

    private void Pause()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        GameManager.Instance.SetGameState(GameManager.GameState.Paused);
        
        ScreenManager screenManager = FindAnyObjectByType<ScreenManager>();
        if (screenManager != null)
        {
            screenManager.ShowScreen(ScreenManager.ScreenType.Pause);
        }

        Debug.Log("[PauseManager] Game paused");
    }

    private void Resume()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        GameManager.Instance.SetGameState(GameManager.GameState.Playing);

        ScreenManager screenManager = FindAnyObjectByType<ScreenManager>();
        if (screenManager != null)
        {
            screenManager.ShowScreen(ScreenManager.ScreenType.Gameplay);
        }

        Debug.Log("[PauseManager] Game resumed");
    }

    public bool IsPaused() => _isPaused;
}