using System;
using UnityEngine;

/// <summary>
/// Projectile that produces structured combat damage.
/// Pool-safe and validated through centralized DamageRules.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float speed = 20f;
    [SerializeField, Min(0f)] private float damage = 10f;
    [SerializeField, Min(0.01f)] private float lifetime = 5f;
    [SerializeField, Min(0.01f)] private float bulletSize = 0.2f;
    [SerializeField] private bool penetrating;
    [SerializeField] private int maxPenetrations = 1;
    [SerializeField] private Color bulletColor = Color.yellow;
    [SerializeField, Min(0f)] private float knockbackForce;

    private Rigidbody2D _rb;
    private CircleCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _direction = Vector2.right;
    private float _spawnTime;
    private int _hitCount;
    private bool _despawning;
    private GameObject _owner;

    public static event Action<Vector3, float, Bullet> OnBulletHit;
    public static event Action<DamageInfo, DamageResult, Bullet> OnDamageResolved;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null) _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        SetupVisuals();
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnEnable()
    {
        _spawnTime = Time.time;
        _hitCount = 0;
        _despawning = false;
        if (_collider != null) _collider.enabled = true;
        if (_spriteRenderer != null) _spriteRenderer.enabled = true;
        ApplyVelocity();
    }

    private void OnDisable()
    {
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        if (_collider != null) _collider.enabled = false;
        if (_spriteRenderer != null) _spriteRenderer.enabled = false;
        _owner = null;
    }

    private void Update()
    {
        if (Time.time - _spawnTime >= lifetime)
        {
            Despawn();
            return;
        }

        if (_rb != null && _rb.linearVelocity.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(_rb.linearVelocity.y, _rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_despawning || other == null || other.CompareTag("Bullet"))
            return;

        // Friendly/self damage is ignored without consuming the projectile.
        if (!DamageRules.CanDamage(_owner, other))
            return;

        Vector2 hitPoint = other.ClosestPoint(transform.position);
        Vector2 hitDirection = _direction.sqrMagnitude > 0f ? _direction.normalized : Vector2.zero;
        DamageInfo info = new DamageInfo(damage, DamageType.Bullet, _owner, hitPoint, hitDirection, knockbackForce);

        bool resolvedDamage = DamageUtility.TryApplyDamage(other, info, out IDamageable damageable);
        DamageResult result = default;
        if (resolvedDamage && damageable is Health health)
            result = new DamageResult(true, !health.IsAlive, info.Amount, health.CurrentHealth);

        OnBulletHit?.Invoke(hitPoint, damage, this);
        if (resolvedDamage) OnDamageResolved?.Invoke(info, result, this);

        // World geometry should still stop a non-penetrating projectile.
        _hitCount++;
        if (!penetrating || _hitCount >= Mathf.Max(1, maxPenetrations))
            Despawn();
    }

    private void SetupVisuals()
    {
        _spriteRenderer.color = bulletColor;
        _spriteRenderer.sortingOrder = 50;
        transform.localScale = Vector3.one * bulletSize;
        if (_collider != null)
        {
            _collider.radius = 0.1f;
            _collider.isTrigger = true;
        }
    }

    private void ApplyVelocity()
    {
        if (_rb != null) _rb.linearVelocity = _direction.normalized * speed;
    }

    private void Despawn()
    {
        if (_despawning) return;
        _despawning = true;
        PoolManager poolManager = GameManager.Instance?.GetPoolManager();
        if (poolManager != null && poolManager.PoolExists("bullet"))
        {
            poolManager.ReturnPooledObject("bullet", gameObject);
            return;
        }
        Destroy(gameObject);
    }

    public void Initialize(Vector2 bulletDirection, float bulletSpeed, float bulletDamage,
        float bulletLifetime, bool isPenetrating, Color color, float size)
    {
        Initialize(bulletDirection, bulletSpeed, bulletDamage, bulletLifetime, isPenetrating, color, size, null);
    }

    public void Initialize(Vector2 bulletDirection, float bulletSpeed, float bulletDamage,
        float bulletLifetime, bool isPenetrating, Color color, float size, GameObject owner)
    {
        _direction = bulletDirection.sqrMagnitude > 0f ? bulletDirection.normalized : Vector2.right;
        speed = Mathf.Max(0.1f, bulletSpeed);
        damage = Mathf.Max(0f, bulletDamage);
        lifetime = Mathf.Max(0.01f, bulletLifetime);
        penetrating = isPenetrating;
        bulletColor = color;
        bulletSize = Mathf.Max(0.01f, size);
        _owner = owner;
        SetupVisuals();
        ApplyVelocity();
    }

    public float GetDamage() => damage;
    public Vector2 GetDirection() => _direction;
    public Vector3 GetPosition() => transform.position;
    public bool HasHit() => _hitCount > 0;
    public float GetLifetimeRemaining() => Mathf.Max(0f, lifetime - (Time.time - _spawnTime));
    public GameObject GetOwner() => _owner;
}
