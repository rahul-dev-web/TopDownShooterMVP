using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the remaining match time from MatchManager.
/// Safe for local MVP and ready to be fed by a network-authoritative match clock later.
/// </summary>
public class MatchTimerDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private string prefix = "";

    private void Awake()
    {
        if (timerText == null)
            timerText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        MatchManager matchManager = GameManager.Instance != null
            ? GameManager.Instance.GetMatchManager()
            : null;

        if (matchManager == null || timerText == null)
            return;

        float remaining = Mathf.Max(0f, matchManager.GetRemainingTime());
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timerText.text = $"{prefix}{minutes:00}:{seconds:00}";
    }
}
