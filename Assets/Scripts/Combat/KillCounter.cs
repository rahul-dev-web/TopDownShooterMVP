/// <summary>
/// KillCounter - Tracks player kills और stats
/// 
/// Kills tracked करता है
/// Deaths tracked करता है
/// Statistics maintain करता है
/// </summary>

using UnityEngine;
using System;

public class KillCounter : MonoBehaviour
{
    // ============== PRIVATE FIELDS ==============

    private int _totalKills = 0;
    private int _totalDeaths = 0;
    private int _currentKillStreak = 0;
    private int _maxKillStreak = 0;

    // Events
    public static event Action<int> OnKillScored;      // (kill count)
    public static event Action<int> OnDeathOccurred;   // (death count)
    public static event Action<int> OnKillStreakChanged; // (streak count)

    private void OnEnable()
    {
        EnemyHealth.OnEnemyKilled += HandleEnemyKilled;
        Health.OnDeath += HandlePlayerDeath;

        Debug.Log("[KillCounter] Subscribed to events");
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyKilled -= HandleEnemyKilled;
        Health.OnDeath -= HandlePlayerDeath;
    }

    private void HandleEnemyKilled(int reward)
    {
        _totalKills++;
        _currentKillStreak++;

        if (_currentKillStreak > _maxKillStreak)
            _maxKillStreak = _currentKillStreak;

        OnKillScored?.Invoke(_totalKills);
        OnKillStreakChanged?.Invoke(_currentKillStreak);

        Debug.Log($"[KillCounter] Kill #{_totalKills}! Streak: {_currentKillStreak}");
    }

    private void HandlePlayerDeath()
    {
        _totalDeaths++;
        _currentKillStreak = 0;

        OnDeathOccurred?.Invoke(_totalDeaths);
        OnKillStreakChanged?.Invoke(_currentKillStreak);

        Debug.Log($"[KillCounter] Death #{_totalDeaths}!");
    }

    // ============== GETTERS ==============

    public int GetTotalKills() => _totalKills;
    public int GetTotalDeaths() => _totalDeaths;
    public int GetKillStreak() => _currentKillStreak;
    public int GetMaxKillStreak() => _maxKillStreak;

    public float GetKDRatio()
    {
        if (_totalDeaths == 0)
            return _totalKills > 0 ? _totalKills : 0;
        return (float)_totalKills / _totalDeaths;
    }

    public void PrintStats()
    {
        Debug.Log("=== KILL COUNTER STATS ===");
        Debug.Log($"Total Kills: {_totalKills}");
        Debug.Log($"Total Deaths: {_totalDeaths}");
        Debug.Log($"Current Streak: {_currentKillStreak}");
        Debug.Log($"Max Streak: {_maxKillStreak}");
        Debug.Log($"K/D Ratio: {GetKDRatio():F2}");
    }
}