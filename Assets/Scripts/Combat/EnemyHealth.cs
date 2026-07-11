/// <summary>
/// EnemyHealth - Enemy-specific health management
/// 
/// Enemy के लिए special handling
/// Drop loot
/// Award points
/// </summary>

using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    // ============== SERIALIZED FIELDS ==============

    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private int killReward = 100;  // Points to award

    // ============== PRIVATE FIELDS ==============

    private Health _health;
    private float _currentHealth;
    private bool _isAlive = true;

    // Events
    public static event Action<int> OnEnemyKilled;  // (reward)

    private void Awake()
    {
        // Get or create Health component
        _health = GetComponent<Health>();
        if (_health == null)
        {
            _health = gameObject.AddComponent<Health>();
        }

        _currentHealth = maxHealth;
        Debug.Log("[EnemyHealth] Enemy health initialized");
    }

    private void OnEnable()
    {
        // Subscribe to health events
        Health.OnDeath += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        Health.OnDeath -= HandleEnemyDeath;
    }

    private void HandleEnemyDeath()
    {
        if (!_isAlive)
            return;

        _isAlive = false;

        // Award points
        OnEnemyKilled?.Invoke(killReward);

        Debug.Log($"[EnemyHealth] Enemy defeated! +{killReward} points");

        // Play death animation/effects
        StartCoroutine(DeathCoroutine());
    }

    private System.Collections.IEnumerator DeathCoroutine()
    {
        // Fade out
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            for (float t = 0; t < 0.5f; t += Time.deltaTime)
            {
                sprite.color = new Color(1, 1, 1, 1 - (t / 0.5f));
                yield return null;
            }
        }

        // Destroy
        Destroy(gameObject);
    }

    public float GetHealth() => _currentHealth;
    public bool IsAlive() => _isAlive;
}