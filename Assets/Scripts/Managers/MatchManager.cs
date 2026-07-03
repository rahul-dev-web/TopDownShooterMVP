/// <summary>
/// MatchManager - Match/Game Logic
/// Match को manage करता है (timer, score, etc)
/// </summary>
using UnityEngine;
 
public class MatchManager : MonoBehaviour
{
    public enum MatchState
    {
        Waiting,
        Countdown,
        Playing,
        Ending,
        Ended
    }
 
    private MatchState _currentMatchState = MatchState.Waiting;
    private float _matchTimer;
    private float _matchDuration = 600f; // 10 minutes
    private int _targetScore = 50;
 
    private void OnEnable()
    {
        Initialize();
    }
 
    private void Initialize()
    {
        Debug.Log("[MatchManager] Initializing...");
        _matchTimer = _matchDuration;
        Debug.Log("[MatchManager] ✓ Initialized");
    }
 
    private void Update()
    {
        if (_currentMatchState == MatchState.Playing)
        {
            _matchTimer -= Time.deltaTime;
 
            if (_matchTimer <= 0)
            {
                EndMatch();
            }
        }
    }
 
    public void StartMatch()
    {
        _currentMatchState = MatchState.Countdown;
        Debug.Log("[MatchManager] Match starting...");
        Invoke(nameof(ActuallyStartMatch), 3f);
    }
 
    private void ActuallyStartMatch()
    {
        _currentMatchState = MatchState.Playing;
        _matchTimer = _matchDuration;
        Debug.Log("[MatchManager] Match started!");
    }
 
    public void EndMatch()
    {
        _currentMatchState = MatchState.Ended;
        Debug.Log("[MatchManager] Match ended!");
        GameManager.Instance.SetGameState(GameManager.GameState.GameOver);
    }
 
    public float GetRemainingTime() => _matchTimer;
    public MatchState GetMatchState() => _currentMatchState;
    public float GetMatchDuration() => _matchDuration;
    public int GetTargetScore() => _targetScore;
 
    public void SetMatchDuration(float duration)
    {
        _matchDuration = duration;
        _matchTimer = duration;
    }
 
    public void SetTargetScore(int score)
    {
        _targetScore = score;
    }
}