using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Enemy-specific death/reward behavior layered on top of the generic Health component.
/// Subscribes to the instance death event to avoid reacting to deaths from other enemies.
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int killReward = 100;
    [SerializeField, Min(0f)] private float destroyDelay = 0.5f;

    private Health _health;
    private bool _handledDeath;

    public static event Action<int> OnEnemyKilled;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (_health != null)
            _health.Died += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.Died -= HandleEnemyDeath;
    }

    private void HandleEnemyDeath(Health health)
    {
        if (_handledDeath)
            return;

        _handledDeath = true;
        OnEnemyKilled?.Invoke(killReward);
        Debug.Log($"[EnemyHealth] Enemy defeated. +{killReward} reward.");
        StartCoroutine(DeathCoroutine());
    }

    private IEnumerator DeathCoroutine()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null && destroyDelay > 0f)
        {
            Color original = sprite.color;
            for (float elapsed = 0f; elapsed < destroyDelay; elapsed += Time.deltaTime)
            {
                if (sprite == null)
                    yield break;

                float alpha = Mathf.Lerp(original.a, 0f, elapsed / destroyDelay);
                sprite.color = new Color(original.r, original.g, original.b, alpha);
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    public float GetHealth() => _health != null ? _health.CurrentHealth : 0f;
    public bool IsAlive() => _health != null && _health.IsAlive;
}
