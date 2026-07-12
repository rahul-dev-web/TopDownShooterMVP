/// <summary>
/// Health - Individual health component
/// 
/// किसी भी object के लिए health manage करता है
/// Player, Enemies, etc. पर लगा सकते हो
/// 
/// Usage:
/// Attach to any GameObject
/// </summary>

using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    // ============== SERIALIZED FIELDS ==============

    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private string objectName = "Object";

    // ============== PRIVATE FIELDS ==============

    private bool _isAlive = true;
    private float _lastDamageTime;
    private float _invulnerabilityDuration = 0.2f;  // Damage flash duration

    // Events
    public static event Action<float, float> OnHealthChanged;      // (current, max)
    public static event Action<Vector3, float> OnDamageTaken;     // (position, damage)
    public static event Action OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
        _isAlive = true;
        Debug.Log($"[Health] {objectName} health initialized: {currentHealth}/{maxHealth}");
    }

    // ============== DAMAGE SYSTEM ==============

    public void TakeDamage(float damageAmount, Vector3 damageSource)
    {
        if (!_isAlive)
            return;

        // Invulnerability check
        if (Time.time - _lastDamageTime < _invulnerabilityDuration)
            return;

        currentHealth -= damageAmount;
        _lastDamageTime = Time.time;

        // Clamp health
        if (currentHealth < 0)
            currentHealth = 0;

            GameManager.Instance?.GetAudioManager()?.PlaySFX("hit");

        // Send events
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke(damageSource, damageAmount);

        Debug.Log($"[Health] {objectName} took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");

        // Check death
        if (currentHealth <= 0)
        {
            Die();
        }

        // Damage flash effect
        StartCoroutine(DamageFlashCoroutine());
    }

    private System.Collections.IEnumerator DamageFlashCoroutine()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sprite.color = originalColor;
        }
        else
        {
            yield return null;
        }
    }

    public void Die()
    {
        if (!_isAlive)
            return;

        _isAlive = false;

        // Play Death Sound
        GameManager.Instance?.GetAudioManager()?.PlaySFX("death");
        Debug.Log($"[Health] {objectName} has died!");

        // Send death event
        OnDeath?.Invoke();

        // Disable gameplay
        if (isPlayer)
        {
            PlayerController player = GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetAlive(false);
            }
        }

        // TODO: Play death animation
        // TODO: Play death sound
    }

    public void Heal(float healAmount)
    {
        if (!_isAlive)
            return;

        currentHealth += healAmount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"[Health] {objectName} healed {healAmount}. Health: {currentHealth}/{maxHealth}");
    }

    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ============== GETTERS ==============

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercent() => currentHealth / maxHealth;
    public bool IsAlive() => _isAlive;
    public bool IsDead() => !_isAlive;

    public void PrintDebugInfo()
    {
        Debug.Log($"=== HEALTH DEBUG ({objectName}) ===");
        Debug.Log($"Current: {currentHealth}/{maxHealth}");
        Debug.Log($"Percent: {GetHealthPercent() * 100}%");
        Debug.Log($"Is Alive: {_isAlive}");
    }
    [ContextMenu("Test Damage 25")]
private void TestDamage()
{
    TakeDamage(25f, transform.position);
}
}