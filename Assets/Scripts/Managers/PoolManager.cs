/// <summary>
/// PoolManager - Object Pooling System
/// Bullets, particles, enemies को reuse करता है (Instantiate/Destroy की जगह)
/// 
/// Usage:
/// GameManager.Instance.GetPoolManager().CreatePool("bullet", bulletPrefab, 50);
/// GameObject bullet = GameManager.Instance.GetPoolManager().GetPooledObject("bullet");
/// </summary>

using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    // Pool dictionary
    private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> _poolParents = new Dictionary<string, GameObject>();

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        Debug.Log("[PoolManager] Initializing...");
        Debug.Log("[PoolManager] ✓ Initialized");
    }

    // ============== POOL CREATION ==============

    /// <summary>
    /// एक नया object pool create करता है
    /// </summary>
    public void CreatePool(string poolName, GameObject prefab, int initialSize = 10)
    {
        // अगर pool पहले से exist करता है, तो return करो
        if (_pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"[PoolManager] Pool already exists: {poolName}");
            return;
        }

        Queue<GameObject> pool = new Queue<GameObject>();

        // Parent object बनाओ (hierarchy को clean रखने के लिए)
        GameObject poolParent = new GameObject($"Pool_{poolName}");
        poolParent.transform.SetParent(transform);
        _poolParents[poolName] = poolParent;

        // Initial objects create करो
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab, poolParent.transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        _pools[poolName] = pool;
        Debug.Log($"[PoolManager] Created pool '{poolName}' with {initialSize} objects");
    }

    // ============== POOL ACCESS ==============

    /// <summary>
    /// Pool से एक object निकालता है
    /// </summary>
    public GameObject GetPooledObject(string poolName)
    {
        if (!_pools.ContainsKey(poolName))
        {
            Debug.LogError($"[PoolManager] Pool not found: {poolName}");
            return null;
        }

        Queue<GameObject> pool = _pools[poolName];
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            // अगर pool खत्म हो गया है, तो नया object बनाओ
            Debug.LogWarning($"[PoolManager] Pool '{poolName}' exhausted, creating new object");
            obj = Instantiate(_poolParents[poolName].transform.GetChild(0).gameObject, 
                              _poolParents[poolName].transform);
        }

        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// Object को pool में वापस करता है
    /// </summary>
    public void ReturnPooledObject(string poolName, GameObject obj)
    {
        if (!_pools.ContainsKey(poolName))
        {
            Debug.LogError($"[PoolManager] Pool not found: {poolName}");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(_poolParents[poolName].transform);
        obj.transform.position = Vector3.zero;
        _pools[poolName].Enqueue(obj);
    }

    // ============== POOL MANAGEMENT ==============

    /// <summary>
    /// Pool का size बढ़ाता है
    /// </summary>
    public void ExpandPool(string poolName, int additionalSize)
    {
        if (!_pools.ContainsKey(poolName))
        {
            Debug.LogError($"[PoolManager] Pool not found: {poolName}");
            return;
        }

        Queue<GameObject> pool = _pools[poolName];
        GameObject prefab = _poolParents[poolName].transform.GetChild(0).gameObject;

        for (int i = 0; i < additionalSize; i++)
        {
            GameObject obj = Instantiate(prefab, _poolParents[poolName].transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        Debug.Log($"[PoolManager] Expanded pool '{poolName}' by {additionalSize} objects");
    }

    /// <summary>
    /// Pool को clear करता है
    /// </summary>
    public void ClearPool(string poolName)
    {
        if (!_pools.ContainsKey(poolName))
        {
            Debug.LogError($"[PoolManager] Pool not found: {poolName}");
            return;
        }

        Queue<GameObject> pool = _pools[poolName];
        while (pool.Count > 0)
        {
            Destroy(pool.Dequeue());
        }

        pool.Clear();
        Debug.Log($"[PoolManager] Cleared pool '{poolName}'");
    }

    /// <summary>
    /// सभी pools clear करता है
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var poolName in _pools.Keys)
        {
            ClearPool(poolName);
        }
        _pools.Clear();
        _poolParents.Clear();
        Debug.Log("[PoolManager] Cleared all pools");
    }

    // ============== GETTERS ==============

    public int GetPoolSize(string poolName)
    {
        if (!_pools.ContainsKey(poolName))
            return 0;

        return _pools[poolName].Count;
    }

    public bool PoolExists(string poolName) => _pools.ContainsKey(poolName);

    // ============== DEBUG ==============

    public void PrintPoolStats()
    {
        Debug.Log("=== POOL STATISTICS ===");
        int totalObjects = 0;

        foreach (var pool in _pools)
        {
            int count = pool.Value.Count;
            totalObjects += count;
            Debug.Log($"  {pool.Key}: {count} objects available");
        }

        Debug.Log($"Total pooled objects: {totalObjects}");
    }
}