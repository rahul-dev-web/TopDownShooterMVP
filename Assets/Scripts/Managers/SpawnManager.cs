using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages explicitly registered scene spawn points.
/// Falls back to objects tagged "SpawnPoint" when no points are assigned in the Inspector.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    private readonly List<Transform> _registeredSpawnPoints = new List<Transform>();
    private int _nextSpawnIndex;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        _registeredSpawnPoints.Clear();

        // Preferred path: explicit Inspector references.
        if (spawnPoints != null)
        {
            foreach (Transform point in spawnPoints)
                RegisterSpawnPointInternal(point);
        }

        // Backward-compatible scene discovery when no explicit points are configured.
        if (_registeredSpawnPoints.Count == 0)
        {
            try
            {
                GameObject[] taggedPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
                foreach (GameObject point in taggedPoints)
                    RegisterSpawnPointInternal(point != null ? point.transform : null);
            }
            catch (UnityException)
            {
                Debug.LogWarning("[SpawnManager] Tag 'SpawnPoint' is not defined. Configure spawn points in the Inspector.");
            }
        }

        _nextSpawnIndex = 0;
        Debug.Log($"[SpawnManager] Ready with {_registeredSpawnPoints.Count} spawn points.");
    }

    public Vector3 GetSpawnPosition()
    {
        Transform point = GetSpawnPoint();
        return point != null ? point.position : transform.position;
    }

    public Transform GetSpawnPoint()
    {
        if (_registeredSpawnPoints.Count == 0)
        {
            Debug.LogError("[SpawnManager] No valid spawn points configured.");
            return null;
        }

        Transform point = _registeredSpawnPoints[_nextSpawnIndex];
        _nextSpawnIndex = (_nextSpawnIndex + 1) % _registeredSpawnPoints.Count;
        return point;
    }

    public void RegisterSpawnPoint(Transform spawnPoint)
    {
        RegisterSpawnPointInternal(spawnPoint);
    }

    private void RegisterSpawnPointInternal(Transform spawnPoint)
    {
        if (spawnPoint == null || _registeredSpawnPoints.Contains(spawnPoint))
            return;

        _registeredSpawnPoints.Add(spawnPoint);
    }

    public int GetSpawnPointCount() => _registeredSpawnPoints.Count;

    public GameObject SpawnPlayer()
    {
        GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Player/Player");
        if (playerPrefab == null)
        {
            Debug.LogError("[SpawnManager] Player prefab not found at Resources/Prefabs/Player/Player.");
            return null;
        }

        GameObject player = Instantiate(playerPrefab, GetSpawnPosition(), Quaternion.identity);
        player.name = "Player";
        return player;
    }
}