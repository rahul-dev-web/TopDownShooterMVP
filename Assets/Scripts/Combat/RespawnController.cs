using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Local gameplay respawn lifecycle. Keeps death handling separate from Health and
/// can later be driven by MatchManager/server authority.
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
    private bool _respawning;

    public static event Action<RespawnController, float> OnRespawnStarted;
    public static event Action<RespawnController> OnRespawnCompleted;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _player = GetComponent<PlayerController>();
        _colliders = GetComponentsInChildren<Collider2D>(true);
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _health.Died += HandleDeath;
    }

    private void OnDisable()
    {
        _health.Died -= HandleDeath;
    }

    private void HandleDeath(Health health)
    {
        if (_respawning) return;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        _respawning = true;
        SetGameplayEnabled(false);
        OnRespawnStarted?.Invoke(this, respawnDelay);
        yield return new WaitForSeconds(respawnDelay);

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
        if (_player != null) _player.SetAlive(true);
        SetGameplayEnabled(true);
        _respawning = false;
        OnRespawnCompleted?.Invoke(this);
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
