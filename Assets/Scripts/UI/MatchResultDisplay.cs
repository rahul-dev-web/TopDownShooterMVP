using TMPro;
using UnityEngine;

/// <summary>
/// Displays the final Team Deathmatch result using MatchManager events.
/// The MatchManager remains the source of truth; this component only renders state.
/// </summary>
public class MatchResultDisplay : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text redScoreText;
    [SerializeField] private TMP_Text blueScoreText;
    [SerializeField] private TMP_Text summaryText;

    private int _redScore;
    private int _blueScore;
    private TeamId _winner = TeamId.None;

    private void OnEnable()
    {
        MatchManager.OnTeamScoreChanged += HandleScoreChanged;
        MatchManager.OnWinnerDeclared += HandleWinnerDeclared;
        MatchManager.OnMatchEnded += HandleMatchEnded;
    }

    private void OnDisable()
    {
        MatchManager.OnTeamScoreChanged -= HandleScoreChanged;
        MatchManager.OnWinnerDeclared -= HandleWinnerDeclared;
        MatchManager.OnMatchEnded -= HandleMatchEnded;
    }

    private void HandleScoreChanged(int redScore, int blueScore, int targetScore)
    {
        _redScore = redScore;
        _blueScore = blueScore;
        RefreshScore();
    }

    private void HandleWinnerDeclared(TeamId winner)
    {
        _winner = winner;
        RefreshResult();
    }

    private void HandleMatchEnded()
    {
        RefreshScore();
        RefreshResult();
    }

    private void RefreshScore()
    {
        if (redScoreText != null)
            redScoreText.text = _redScore.ToString();

        if (blueScoreText != null)
            blueScoreText.text = _blueScore.ToString();
    }

    private void RefreshResult()
    {
        string headline;
        string summary;

        switch (_winner)
        {
            case TeamId.Red:
                headline = "RED TEAM WINS";
                summary = $"Red {_redScore}  -  {_blueScore} Blue";
                break;

            case TeamId.Blue:
                headline = "BLUE TEAM WINS";
                summary = $"Blue {_blueScore}  -  {_redScore} Red";
                break;

            default:
                headline = "DRAW";
                summary = $"Red {_redScore}  -  {_blueScore} Blue";
                break;
        }

        if (resultText != null)
            resultText.text = headline;

        if (summaryText != null)
            summaryText.text = summary;
    }
}
