using TMPro;
using UnityEngine;

/// <summary>
/// Displays the current local MVP score and target score.
/// Later this can consume authoritative network score events without changing the UI prefab.
/// </summary>
public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private string format = "Score: {0} / {1}";

    private MatchManager _matchManager;

    private void Awake()
    {
        _matchManager = FindFirstObjectByType<MatchManager>();
    }

    private void OnEnable()
    {
        MatchManager.OnScoreChanged += HandleScoreChanged;
        Refresh();
    }

    private void OnDisable()
    {
        MatchManager.OnScoreChanged -= HandleScoreChanged;
    }

    private void Refresh()
    {
        if (_matchManager != null)
            HandleScoreChanged(_matchManager.GetCurrentScore(), _matchManager.GetTargetScore());
    }

    private void HandleScoreChanged(int score, int target)
    {
        if (scoreText != null)
            scoreText.text = string.Format(format, score, target);
    }
}