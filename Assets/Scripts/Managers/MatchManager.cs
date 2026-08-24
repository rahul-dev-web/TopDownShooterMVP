/// <summary>
/// MatchManager - Owns local match lifecycle for the MVP.
/// Networking can later replace the timer/score authority without changing UI consumers.
/// </summary>
using System;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public enum MatchState { Waiting, Countdown, Playing, Ending, Ended }

    [SerializeField, Min(1f)] private float matchDuration = 600f;
    [SerializeField, Min(1)] private int targetScore = 50;
    [SerializeField, Min(0f)] private float countdownDuration = 3f;

    private MatchState _currentMatchState = MatchState.Waiting;
    private float _matchTimer;
    private int _currentScore;
    private bool _ending;

    public static event Action<MatchState> OnMatchStateChanged;
    public static event Action<float> OnTimerChanged;
    public static event Action<int, int> OnScoreChanged;
    public static event Action OnMatchEnded;

    private void OnEnable() => ResetMatch();

    private void Update()
    {
        if (_currentMatchState != MatchState.Playing)
            return;

        _matchTimer = Mathf.Max(0f, _matchTimer - Time.deltaTime);
        OnTimerChanged?.Invoke(_matchTimer);

        if (_matchTimer <= 0f)
            EndMatch();
    }

    public void StartMatch()
    {
        if (_currentMatchState == MatchState.Playing || _currentMatchState == MatchState.Countdown)
            return;

        CancelInvoke(nameof(ActuallyStartMatch));
        SetState(MatchState.Countdown);
        Invoke(nameof(ActuallyStartMatch), countdownDuration);
        Debug.Log("[MatchManager] Match countdown started.");
    }

    private void ActuallyStartMatch()
    {
        _matchTimer = matchDuration;
        _currentScore = 0;
        _ending = false;
        SetState(MatchState.Playing);
        OnTimerChanged?.Invoke(_matchTimer);
        OnScoreChanged?.Invoke(_currentScore, targetScore);
        Debug.Log("[MatchManager] Match started.");
    }

    public void AddScore(int amount = 1)
    {
        if (_currentMatchState != MatchState.Playing || amount <= 0)
            return;

        _currentScore += amount;
        OnScoreChanged?.Invoke(_currentScore, targetScore);

        if (_currentScore >= targetScore)
            EndMatch();
    }

    public void EndMatch()
    {
        if (_ending || _currentMatchState == MatchState.Ended)
            return;

        _ending = true;
        CancelInvoke(nameof(ActuallyStartMatch));
        SetState(MatchState.Ending);
        SetState(MatchState.Ended);
        OnMatchEnded?.Invoke();

        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameManager.GameState.GameOver);

        Debug.Log("[MatchManager] Match ended.");
    }

    public void ResetMatch()
    {
        CancelInvoke(nameof(ActuallyStartMatch));
        _matchTimer = matchDuration;
        _currentScore = 0;
        _ending = false;
        SetState(MatchState.Waiting);
        OnTimerChanged?.Invoke(_matchTimer);
        OnScoreChanged?.Invoke(_currentScore, targetScore);
    }

    private void SetState(MatchState state)
    {
        _currentMatchState = state;
        OnMatchStateChanged?.Invoke(state);
    }

    public float GetRemainingTime() => _matchTimer;
    public MatchState GetMatchState() => _currentMatchState;
    public float GetMatchDuration() => matchDuration;
    public int GetTargetScore() => targetScore;
    public int GetCurrentScore() => _currentScore;

    public void SetMatchDuration(float duration)
    {
        matchDuration = Mathf.Max(1f, duration);
        if (_currentMatchState != MatchState.Playing)
            _matchTimer = matchDuration;
    }

    public void SetTargetScore(int score) => targetScore = Mathf.Max(1, score);
}