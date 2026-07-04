/// <summary>
/// SpawnManager - Player और Enemy Spawning
/// Spawn points को manage करता है
/// </summary>
using UnityEngine;
 
public class SpawnManager : MonoBehaviour
{
    private Transform[] _spawnPoints;
    private int _nextSpawnIndex = 0;
 
    private void OnEnable()
    {
        Initialize();
    }
 
    private void Initialize()
    {
        Debug.Log("[SpawnManager] Initializing...");
 
        // Scene में सभी spawn points find करो
        // Tag "SpawnPoint" वाली सभी objects को find करो
        _spawnPoints = FindObjectsOfType<Transform>();
 
        Debug.Log($"[SpawnManager] ✓ Found {_spawnPoints.Length} spawn points");
    }
 
    public Vector3 GetSpawnPosition()
    {
        if (_spawnPoints.Length == 0)
        {
            Debug.LogError("[SpawnManager] No spawn points found!");
            return Vector3.zero;
        }
 
        Vector3 spawnPos = _spawnPoints[_nextSpawnIndex].position;
        _nextSpawnIndex = (_nextSpawnIndex + 1) % _spawnPoints.Length;
 
        return spawnPos;
    }
 
    public Transform GetSpawnPoint()
    {
        if (_spawnPoints.Length == 0)
        {
            Debug.LogError("[SpawnManager] No spawn points found!");
            return null;
        }
 
        Transform spawnPoint = _spawnPoints[_nextSpawnIndex];
        _nextSpawnIndex = (_nextSpawnIndex + 1) % _spawnPoints.Length;
 
        return spawnPoint;
    }
 
    public void RegisterSpawnPoint(Transform spawnPoint)
    {
        // Dynamic spawn point register करो
        Debug.Log($"[SpawnManager] Registered spawn point: {spawnPoint.name}");
    }
 
    public int GetSpawnPointCount() => _spawnPoints.Length;

    // Assets/Scripts/Managers/SpawnManager.cs में यह method add करो

public GameObject SpawnPlayer()
{
    GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Player/Player");
    if (playerPrefab == null)
    {
        Debug.LogError("[SpawnManager] Player prefab not found!");
        return null;
    }

    Vector3 spawnPos = GetSpawnPosition();
    GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    player.name = "Player";

    Debug.Log($"[SpawnManager] Player spawned at {spawnPos}");
    return player;
}
}