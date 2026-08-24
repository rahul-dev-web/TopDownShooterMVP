using UnityEngine;
using TMPro;

/// <summary>
/// Phase 5 result screen controller.
/// Shows the local MVP match summary. Later the same API can receive server-authoritative results.
/// </summary>
public class ResultScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text deathsText;
    [SerializeField] private TMP_Text summaryText;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (GameManager.Instance == null)
            return;

        int kills = GameManager.Instance.GetPlayerKills();
        int deaths = GameManager.Instance.GetPlayerDeaths();

        if (titleText != null)
            titleText.text = "MATCH RESULT";

        if (killsText != null)
            killsText.text = $"Kills: {kills}";

        if (deathsText != null)
            deathsText.text = $"Deaths: {deaths}";

        if (summaryText != null)
            summaryText.text = kills >= deaths ? "Good match!" : "Keep improving!";
    }

    public void RestartMatch()
    {
        GameManager.Instance.ResetStats();
        GameManager.Instance.RestartGame();
    }

    public void ReturnToMenu()
    {
        GameManager.Instance.ResetStats();
        GameManager.Instance.SetGameState(GameManager.GameState.Menu);
    }
}
