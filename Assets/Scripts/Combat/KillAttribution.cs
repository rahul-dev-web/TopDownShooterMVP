using System;
using UnityEngine;

/// <summary>
/// Records the last valid damaging source and converts a death into a structured KillEvent.
/// Attribution is time-limited so stale damage does not receive credit.
/// </summary>
[RequireComponent(typeof(Health))]
public class KillAttribution : MonoBehaviour
{
    [SerializeField, Min(0f)] private float attributionWindow = 10f;

    private Health _health;
    private GameObject _lastAttacker;
    private DamageInfo _lastDamageInfo;
    private float _lastDamageTime = float.NegativeInfinity;

    public static event Action<KillEvent> OnKillConfirmed;

    private void Awake() => _health = GetComponent<Health>();

    private void OnEnable()
    {
        if (_health == null) return;
        _health.DamageApplied += HandleDamageApplied;
        _health.Died += HandleDied;
    }

    private void OnDisable()
    {
        if (_health == null) return;
        _health.DamageApplied -= HandleDamageApplied;
        _health.Died -= HandleDied;
    }

    private void HandleDamageApplied(DamageInfo info, DamageResult result)
    {
        if (!result.Applied || info.Source == null)
            return;

        _lastAttacker = info.Source;
        _lastDamageInfo = info;
        _lastDamageTime = Time.time;
    }

    private void HandleDied(Health victimHealth)
    {
        bool validAttribution = _lastAttacker != null &&
                                Time.time - _lastDamageTime <= attributionWindow;

        OnKillConfirmed?.Invoke(new KillEvent(
            validAttribution ? _lastAttacker : null,
            gameObject,
            _lastDamageInfo,
            validAttribution));

        ClearAttribution();
    }

    public void ClearAttribution()
    {
        _lastAttacker = null;
        _lastDamageInfo = default;
        _lastDamageTime = float.NegativeInfinity;
    }
}
