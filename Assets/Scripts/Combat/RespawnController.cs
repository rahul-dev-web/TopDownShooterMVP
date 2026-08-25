using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Local gameplay respawn lifecycle. Respawns are cancelled when the match is no
/// longer playable, keeping local behavior compatible with future server authority.
/// </summary>
[RequireComponent(typeof(Health))]
public class RespawnController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float respawnDelay = 3f;
    [SerializeField] private bool disableGameplayWhileDead = true;

    private Health _health;
    private PlayerController _player;
    private Collider2D[] _colliders;
    private Rigidbody2D _rb;
    private KillAttribution _killAttribution;
    private Coroutine _respawnRoutine;
    private bool _respawning;

    public static event Action<RespawnController, float> OnRespawnStarted;
    public static event Action<RespawnController> OnRespawnCompleted;
    public static event Action<RespawnController> OnRespawnCancelled;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _player = GetComponent<PlayerController>();
        _colliders = GetComponentsInChildren<Collider2D>(true);
        _rb = GetComponent<Rigidbody2D>();
        _killAttribution = GetComponent<KillAttribution>();
    }

    private void OnEnable()
    {
        _health.Died += HandleDeath;
        MatchManager.OnMatchStateChanged += HandleMatchStateChanged;
        MatchManager.OnMatchEnded += HandleMatchEnded;
    }

    private void OnDisable()
    {
        _health.Died -= HandleDeath;
        MatchManager.OnMatchStateChanged -= HandleMatchStateChanged;
        MatchManager.OnMatchEnded -= HandleMatchEnded;
        CancelRespawn();
    }

    private void HandleDeath(Health health)
    {
        if (_respawning || !CanRespawn()) return;
        _respawnRoutine = StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        _respawning = true;
        SetGameplayEnabled(false);
        OnRespawnStarted?.Invoke(this, respawnDelay);
        yield return new WaitForSeconds(respawnDelay);

        if (!CanRespawn())
        {
            CancelRespawn();
            yield break;
        }

        SpawnManager spawnManager = FindFirstObjectByType<SpawnManager>();
        if (spawnManager != null)
            transform.position = spawnManager.GetSpawnPosition();
        else
            Debug.LogWarning("[RespawnController] SpawnManager not found; respawning at current position.");

        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }

        _health.SetHealth(_health.MaxHealth);
        _killAttribution?.ClearAttribution();
        if (_player != null) _player.SetAlive(true);
        SetGameplayEnabled(true);
        _respawning = false;
        _respawnRoutine = null;
        OnRespawnCompleted?.Invoke(this);
    }

    private bool CanRespawn()
    {
        MatchManager matchManager = FindFirstObjectByType<MatchManager>();
        return matchManager == null || matchManager.GetMatchState() == MatchManager.MatchState.Playing;
    }

    private void HandleMatchStateChanged(MatchManager.MatchState state)
    {
        if (state == MatchManager.MatchState.Ending || state == MatchManager.MatchState.Ended)
            CancelRespawn();
    }

    private void HandleMatchEnded()
    {
        CancelRespawn();
    }

    private void CancelRespawn()
    {
        if (!_respawning) return;
        if (_respawnRoutine != null) StopCoroutine(_respawnRoutine);
        _respawnRoutine = null;
        _respawning = false;
        OnRespawnCancelled?.Invoke(this);
    }

    private void SetGameplayEnabled(bool enabled)
    {
        if (!disableGameplayWhileDead) return;
        foreach (Collider2D col in _colliders)
            if (col != null) col.enabled = enabled;

        if (_player != null) _player.enabled = enabled;
    }

    public bool IsRespawning => _respawning;
}
