using TMPro;
using UnityEngine;

/// <summary>
/// Displays Team Deathmatch scores from MatchManager events.
/// UI remains read-only; MatchManager stays the score authority.
/// </summary>
public class TeamScoreDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text redScoreText;
    [SerializeField] private TMP_Text blueScoreText;
    [SerializeField] private TMP_Text targetScoreText;

    [Header("Formatting")]
    [SerializeField] private string redPrefix = "RED ";
    [SerializeField] private string bluePrefix = "BLUE ";
    [SerializeField] private string targetPrefix = "FIRST TO ";

    private void OnEnable()
    {
        MatchManager.OnTeamScoreChanged += HandleTeamScoreChanged;
        RefreshFromManager();
    }

    private void OnDisable()
    {
        MatchManager.OnTeamScoreChanged -= HandleTeamScoreChanged;
    }

    private void RefreshFromManager()
    {
        MatchManager manager = GameManager.Instance != null
            ? GameManager.Instance.GetMatchManager()
            : FindFirstObjectByType<MatchManager>();

        if (manager == null)
            return;

        HandleTeamScoreChanged(
            manager.GetTeamScore(TeamId.Red),
            manager.GetTeamScore(TeamId.Blue),
            manager.GetTargetScore());
    }

    private void HandleTeamScoreChanged(int redScore, int blueScore, int targetScore)
    {
        if (redScoreText != null)
            redScoreText.text = $"{redPrefix}{redScore}";

        if (blueScoreText != null)
            blueScoreText.text = $"{bluePrefix}{blueScore}";

        if (targetScoreText != null)
            targetScoreText.text = $"{targetPrefix}{targetScore}";
    }
}
