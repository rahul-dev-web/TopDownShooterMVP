/// <summary>
/// MatchManager - Owns local Team Deathmatch lifecycle for the MVP.
/// Score authority is isolated here so networking can later replace local events
/// without forcing UI consumers to understand transport details.
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
    private int _redScore;
    private int _blueScore;
    private bool _ending;
    private TeamId _winner = TeamId.None;

    public static event Action<MatchState> OnMatchStateChanged;
    public static event Action<float> OnTimerChanged;
    public static event Action<int, int> OnScoreChanged;
    public static event Action<int, int, int> OnTeamScoreChanged;
    public static event Action<KillEvent> OnKillRegistered;
    public static event Action<TeamId> OnWinnerDeclared;
    public static event Action OnMatchEnded;

    private void OnEnable()
    {
        KillAttribution.OnKillConfirmed += HandleKillConfirmed;
        ResetMatch();
    }

    private void OnDisable()
    {
        KillAttribution.OnKillConfirmed -= HandleKillConfirmed;
    }

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
        _redScore = 0;
        _blueScore = 0;
        _winner = TeamId.None;
        _ending = false;
        SetState(MatchState.Playing);
        OnTimerChanged?.Invoke(_matchTimer);
        PublishScore();
        Debug.Log("[MatchManager] Team Deathmatch started.");
    }

    private void HandleKillConfirmed(KillEvent killEvent)
    {
        if (_currentMatchState != MatchState.Playing || !killEvent.HasKiller || killEvent.Killer == null)
            return;

        TeamMember killerTeamMember = killEvent.Killer.GetComponentInParent<TeamMember>();
        if (killerTeamMember == null || killerTeamMember.Team == TeamId.None)
            return;

        AddTeamScore(killerTeamMember.Team, 1);
        OnKillRegistered?.Invoke(killEvent);
    }

    public void AddTeamScore(TeamId team, int amount = 1)
    {
        if (_currentMatchState != MatchState.Playing || amount <= 0 || team == TeamId.None)
            return;

        switch (team)
        {
            case TeamId.Red:
                _redScore += amount;
                break;
            case TeamId.Blue:
                _blueScore += amount;
                break;
            default:
                return;
        }

        PublishScore();

        if (_redScore >= targetScore)
        {
            _winner = TeamId.Red;
            EndMatch();
        }
        else if (_blueScore >= targetScore)
        {
            _winner = TeamId.Blue;
            EndMatch();
        }
    }

    public void EndMatch()
    {
        if (_ending || _currentMatchState == MatchState.Ended)
            return;

        _ending = true;
        CancelInvoke(nameof(ActuallyStartMatch));

        if (_winner == TeamId.None)
        {
            if (_redScore > _blueScore) _winner = TeamId.Red;
            else if (_blueScore > _redScore) _winner = TeamId.Blue;
        }

        SetState(MatchState.Ending);
        OnWinnerDeclared?.Invoke(_winner);
        SetState(MatchState.Ended);
        OnMatchEnded?.Invoke();

        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameManager.GameState.GameOver);

        Debug.Log($"[MatchManager] Match ended. Red: {_redScore}, Blue: {_blueScore}, Winner: {_winner}");
    }

    public void ResetMatch()
    {
        CancelInvoke(nameof(ActuallyStartMatch));
        _matchTimer = matchDuration;
        _redScore = 0;
        _blueScore = 0;
        _winner = TeamId.None;
        _ending = false;
        SetState(MatchState.Waiting);
        OnTimerChanged?.Invoke(_matchTimer);
        PublishScore();
    }

    private void PublishScore()
    {
        OnScoreChanged?.Invoke(Mathf.Max(_redScore, _blueScore), targetScore);
        OnTeamScoreChanged?.Invoke(_redScore, _blueScore, targetScore);
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
    public int GetCurrentScore() => Mathf.Max(_redScore, _blueScore);
    public int GetTeamScore(TeamId team) => team == TeamId.Red ? _redScore : team == TeamId.Blue ? _blueScore : 0;
    public TeamId GetWinner() => _winner;

    public void SetMatchDuration(float duration)
    {
        matchDuration = Mathf.Max(1f, duration);
        if (_currentMatchState != MatchState.Playing)
            _matchTimer = matchDuration;
    }

    public void SetTargetScore(int score) => targetScore = Mathf.Max(1, score);
}
