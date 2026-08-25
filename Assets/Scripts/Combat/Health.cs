using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Generic health component implementing the shared combat contract.
/// Keeps legacy damage events/methods while exposing the structured combat pipeline.
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isPlayer;
    [SerializeField] private string objectName = "Object";
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.2f;
    [SerializeField, Min(0.01f)] private float damageFlashDuration = 0.15f;

    private bool _isAlive = true;
    private float _lastDamageTime = float.NegativeInfinity;

    // Legacy events retained for existing UI and gameplay consumers.
    public static event Action<float, float> OnHealthChanged;
    public static event Action<Vector3, float> OnDamageTaken;
    public static event Action OnDeath;

    // Structured events for the refactored combat pipeline.
    public event Action<DamageInfo, DamageResult> DamageApplied;
    public event Action<Health> Died;

    public bool IsAlive => _isAlive;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;
        _isAlive = true;
        _lastDamageTime = float.NegativeInfinity;
        Debug.Log($"[Health] {objectName} initialized: {currentHealth}/{maxHealth}");
    }

    /// <summary>Structured damage entry point.</summary>
    public bool ApplyDamage(DamageInfo damageInfo)
    {
        return ApplyDamageWithResult(damageInfo).Applied;
    }

    public DamageResult ApplyDamageWithResult(DamageInfo damageInfo)
    {
        if (!_isAlive || !damageInfo.IsValid)
            return new DamageResult(false, false, 0f, currentHealth);

        if (Time.time - _lastDamageTime < invulnerabilityDuration)
            return new DamageResult(false, false, 0f, currentHealth);

        float before = currentHealth;
        currentHealth = Mathf.Max(0f, currentHealth - damageInfo.Amount);
        float applied = before - currentHealth;
        _lastDamageTime = Time.time;

        GameManager.Instance?.GetAudioManager()?.PlaySFX("hit");

        var result = new DamageResult(true, currentHealth <= 0f, applied, currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke(damageInfo.HitPoint, applied);
        DamageApplied?.Invoke(damageInfo, result);

        Debug.Log($"[Health] {objectName} took {applied} {damageInfo.Type} damage. {currentHealth}/{maxHealth}");

        if (result.Killed)
            Die();
        else
            StartCoroutine(DamageFlashCoroutine());

        return result;
    }

    // Backward-compatible adapters for existing callers/scenes.
    public void TakeDamage(float damageAmount, Vector3 damageSource)
    {
        ApplyDamage(new DamageInfo(damageAmount, DamageType.Bullet, null, damageSource));
    }

    public void TakeDamage(float damageAmount)
    {
        ApplyDamage(new DamageInfo(damageAmount));
    }

    private IEnumerator DamageFlashCoroutine()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite == null)
            yield break;

        Color originalColor = sprite.color;
        sprite.color = Color.red;
        yield return new WaitForSeconds(damageFlashDuration);

        if (sprite != null)
            sprite.color = originalColor;
    }

    public void Die()
    {
        if (!_isAlive)
            return;

        _isAlive = false;
        GameManager.Instance?.GetAudioManager()?.PlaySFX("death");

        OnDeath?.Invoke();
        Died?.Invoke(this);
        Debug.Log($"[Health] {objectName} has died.");

        if (isPlayer)
        {
            PlayerController player = GetComponent<PlayerController>();
            player?.SetAlive(false);
        }
    }

    public void Heal(float healAmount)
    {
        if (!_isAlive || healAmount <= 0f)
            return;

        float before = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        if (!Mathf.Approximately(before, currentHealth))
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        _isAlive = currentHealth > 0f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercent() => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
    public bool IsDead() => !_isAlive;

    public void PrintDebugInfo()
    {
        Debug.Log($"=== HEALTH DEBUG ({objectName}) ===\nCurrent: {currentHealth}/{maxHealth}\nAlive: {_isAlive}");
    }

    [ContextMenu("Test Damage 25")]
    private void TestDamage()
    {
        ApplyDamage(new DamageInfo(25f, DamageType.Bullet, null, transform.position));
    }
}