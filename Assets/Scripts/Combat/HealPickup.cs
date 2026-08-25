using UnityEngine;

/// <summary>
/// Simple health pickup for the local MVP combat loop.
/// Applies healing through Health.Heal so all existing health UI events remain authoritative.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HealPickup : MonoBehaviour
{
    [Header("Healing")]
    [SerializeField, Min(1f)] private float healAmount = 25f;
    [SerializeField] private bool playersOnly = true;
    [SerializeField] private bool destroyAfterUse = true;

    [Header("Respawn")]
    [SerializeField] private bool respawnAfterUse;
    [SerializeField, Min(0.1f)] private float respawnDelay = 15f;

    private Collider2D _pickupCollider;
    private SpriteRenderer _spriteRenderer;
    private bool _consumed;

    private void Awake()
    {
        _pickupCollider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _pickupCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_consumed)
            return;

        if (playersOnly && other.GetComponent<PlayerController>() == null)
            return;

        Health health = other.GetComponent<Health>();
        if (health == null || !health.IsAlive)
            return;

        float missingHealth = health.MaxHealth - health.CurrentHealth;
        if (missingHealth <= 0.01f)
            return;

        health.Heal(Mathf.Min(healAmount, missingHealth));
        Consume();
    }

    private void Consume()
    {
        _consumed = true;

        if (destroyAfterUse && !respawnAfterUse)
        {
            Destroy(gameObject);
            return;
        }

        if (_pickupCollider != null)
            _pickupCollider.enabled = false;
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;

        if (respawnAfterUse)
            Invoke(nameof(Respawn), respawnDelay);
    }

    private void Respawn()
    {
        _consumed = false;
        if (_pickupCollider != null)
            _pickupCollider.enabled = true;
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = true;
    }

    [ContextMenu("Test Consume")]
    private void TestConsume()
    {
        Consume();
    }
}
