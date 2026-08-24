/// <summary>
/// KillCounter - Tracks local MVP kills, deaths and streaks.
/// Match score integration is isolated here so the combat event source can later be replaced by network-authoritative events.
/// </summary>
using System;
using UnityEngine;

public class KillCounter : MonoBehaviour
{
    private int _totalKills;
    private int _totalDeaths;
    private int _currentKillStreak;
    private int _maxKillStreak;
    private MatchManager _matchManager;

    public static event Action<int> OnKillScored;
    public static event Action<int> OnDeathOccurred;
    public static event Action<int> OnKillStreakChanged;

    private void Awake()
    {
        _matchManager = FindFirstObjectByType<MatchManager>();
    }

    private void OnEnable()
    {
        EnemyHealth.OnEnemyKilled += HandleEnemyKilled;
        Health.OnDeath += HandlePlayerDeath;
        MatchManager.OnMatchStateChanged += HandleMatchStateChanged;
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyKilled -= HandleEnemyKilled;
        Health.OnDeath -= HandlePlayerDeath;
        MatchManager.OnMatchStateChanged -= HandleMatchStateChanged;
    }

    private void HandleEnemyKilled(int reward)
    {
        _totalKills++;
        _currentKillStreak++;
        _maxKillStreak = Mathf.Max(_maxKillStreak, _currentKillStreak);

        if (_matchManager != null)
            _matchManager.AddScore(1);

        OnKillScored?.Invoke(_totalKills);
        OnKillStreakChanged?.Invoke(_currentKillStreak);
    }

    private void HandlePlayerDeath()
    {
        _totalDeaths++;
        _currentKillStreak = 0;
        OnDeathOccurred?.Invoke(_totalDeaths);
        OnKillStreakChanged?.Invoke(_currentKillStreak);
    }

    private void HandleMatchStateChanged(MatchManager.MatchState state)
    {
        if (state != MatchManager.MatchState.Waiting)
            return;

        _totalKills = 0;
        _totalDeaths = 0;
        _currentKillStreak = 0;
        _maxKillStreak = 0;

        OnKillScored?.Invoke(_totalKills);
        OnDeathOccurred?.Invoke(_totalDeaths);
        OnKillStreakChanged?.Invoke(_currentKillStreak);
    }

    public int GetTotalKills() => _totalKills;
    public int GetTotalDeaths() => _totalDeaths;
    public int GetKillStreak() => _currentKillStreak;
    public int GetMaxKillStreak() => _maxKillStreak;

    public float GetKDRatio()
    {
        if (_totalDeaths == 0)
            return _totalKills > 0 ? _totalKills : 0f;
        return (float)_totalKills / _totalDeaths;
    }
}