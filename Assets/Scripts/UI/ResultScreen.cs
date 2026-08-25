using TMPro;
using UnityEngine;

/// <summary>
/// Displays the local MVP match result.
/// Reads from KillCounter so combat stats have a single source of truth.
/// The same UI can later consume authoritative server results.
/// </summary>
public class ResultScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text deathsText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private TMP_Text scoreText;

    private KillCounter _killCounter;
    private MatchManager _matchManager;

    private void Awake()
    {
        _killCounter = FindFirstObjectByType<KillCounter>();
        _matchManager = FindFirstObjectByType<MatchManager>();
    }

    private void OnEnable() => Refresh();

    public void Refresh()
    {
        int kills = _killCounter != null ? _killCounter.GetTotalKills() : 0;
        int deaths = _killCounter != null ? _killCounter.GetTotalDeaths() : 0;
        int score = _matchManager != null ? _matchManager.GetCurrentScore() : kills;
        int target = _matchManager != null ? _matchManager.GetTargetScore() : 0;

        if (titleText != null)
            titleText.text = target > 0 && score >= target ? "VICTORY" : "MATCH RESULT";

        if (killsText != null)
            killsText.text = $"Kills: {kills}";

        if (deathsText != null)
            deathsText.text = $"Deaths: {deaths}";

        if (scoreText != null)
            scoreText.text = target > 0 ? $"Score: {score} / {target}" : $"Score: {score}";

        if (summaryText != null)
        {
            float kd = deaths == 0 ? kills : (float)kills / deaths;
            summaryText.text = kd >= 1f ? "Good match!" : "Keep improving!";
        }
    }

    public void RestartMatch()
    {
        Time.timeScale = 1f;
        GameManager.Instance.RestartGame();
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ResetStats();
        GameManager.Instance.SetGameState(GameManager.GameState.Menu);
    }
}
