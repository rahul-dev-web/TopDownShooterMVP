/// <summary>
/// EnemySpawner - Spawns enemies at intervals
/// 
/// Wave-based या continuous spawning
/// Difficulty scaling
/// </summary>

using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    // ============== SERIALIZED FIELDS ==============

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float spawnRadius = 15f;

    // ============== PRIVATE FIELDS ==============

    private float _lastSpawnTime;
    private int _currentEnemyCount = 0;
    private List<GameObject> _activeEnemies = new List<GameObject>();

    private void Awake()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] Enemy prefab not assigned!");
            enabled = false;
            return;
        }

        _lastSpawnTime = Time.time;
        Debug.Log("[EnemySpawner] Initialized");
    }

    private void Update()
    {
        // Check if we can spawn
        if (_currentEnemyCount < maxEnemies && 
            Time.time - _lastSpawnTime > spawnInterval)
        {
            SpawnEnemy();
            _lastSpawnTime = Time.time;
        }

        // Clean up dead enemies
        _activeEnemies.RemoveAll(e => e == null);
        _currentEnemyCount = _activeEnemies.Count;
    }

    private void SpawnEnemy()
    {
        // Get player position
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null)
{
    Debug.LogWarning("[EnemySpawner] Player not found, cannot spawn");
    return;
}

        Vector3 playerPos = player.GetWorldPosition();

        // Calculate spawn position (random around player)
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = playerPos + (Vector3)randomDir * spawnRadius;

        // Spawn enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        _activeEnemies.Add(enemy);

        Debug.Log($"[EnemySpawner] Spawned enemy at {spawnPos}. Count: {_currentEnemyCount}/{maxEnemies}");
    }

    public int GetEnemyCount() => _currentEnemyCount;
    public void SetMaxEnemies(int newMax) => maxEnemies = newMax;
    public void SetSpawnInterval(float newInterval) => spawnInterval = newInterval;
}