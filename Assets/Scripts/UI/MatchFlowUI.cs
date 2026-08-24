using UnityEngine;

/// <summary>
/// Bridges MatchManager lifecycle events to UI screens.
/// Keeps gameplay systems independent from concrete UI objects.
/// </summary>
public class MatchFlowUI : MonoBehaviour
{
    [SerializeField] private ScreenManager screenManager;

    private void Awake()
    {
        if (screenManager == null)
            screenManager = FindFirstObjectByType<ScreenManager>();
    }

    private void OnEnable()
    {
        MatchManager.OnMatchStateChanged += HandleMatchStateChanged;
    }

    private void OnDisable()
    {
        MatchManager.OnMatchStateChanged -= HandleMatchStateChanged;
    }

    private void HandleMatchStateChanged(MatchManager.MatchState state)
    {
        if (screenManager == null)
            return;

        switch (state)
        {
            case MatchManager.MatchState.Waiting:
                if (screenManager.HasScreen(ScreenManager.ScreenType.MainMenu))
                    screenManager.ShowScreen(ScreenManager.ScreenType.MainMenu);
                break;

            case MatchManager.MatchState.Countdown:
                if (screenManager.HasScreen(ScreenManager.ScreenType.Loading))
                    screenManager.ShowScreen(ScreenManager.ScreenType.Loading);
                break;

            case MatchManager.MatchState.Playing:
                screenManager.ShowScreen(ScreenManager.ScreenType.Gameplay);
                break;

            case MatchManager.MatchState.Ended:
                if (screenManager.HasScreen(ScreenManager.ScreenType.Result))
                    screenManager.ShowScreen(ScreenManager.ScreenType.Result);
                else if (screenManager.HasScreen(ScreenManager.ScreenType.GameOver))
                    screenManager.ShowScreen(ScreenManager.ScreenType.GameOver);
                break;
        }
    }
}