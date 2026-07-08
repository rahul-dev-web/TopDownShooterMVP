/// <summary>
/// Bullet - Individual bullet object
/// 
/// Spawn होता है जब gun fire करे
/// Physics से move करता है
/// Collision पर damage deal करता है
/// Pool से reuse होता है
/// 
/// Usage:
/// Automatically spawned by Gun
/// </summary>

using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Bullet : MonoBehaviour
{
    // ============== SERIALIZED FIELDS ==============

    [SerializeField] private float speed = 20f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float bulletSize = 0.2f;
    [SerializeField] private bool penetrating = false;
    [SerializeField] private Color bulletColor = Color.yellow;

    // ============== PRIVATE FIELDS ==============

    private Rigidbody2D _rb;
    private CircleCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _direction = Vector2.right;
    private float _spawnTime;
    private bool _hasHit = false;

    // Events
    public static event Action<Vector3, float, Bullet> OnBulletHit;

    private void Awake()
    {
        // Components को cache करो
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // Create SpriteRenderer if doesn't exist
        if (_spriteRenderer == null)
        {
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Setup default circle sprite
        SetupBulletVisuals();

        Debug.Log("[Bullet] Awake complete");
    }

    private void SetupBulletVisuals()
    {
        // Default white circle sprite
        _spriteRenderer.color = bulletColor;
        _spriteRenderer.sortingOrder = 50;

        // Scale for size
        transform.localScale = Vector3.one * bulletSize;

        // Collider setup
        if (_collider != null)
        {
            _collider.radius = 0.1f;
            _collider.isTrigger = true;
        }
    }

    private void OnEnable()
    {
        _spawnTime = Time.time;
        _hasHit = false;

        // Enable components
        if (_rb != null) _rb.isKinematic = false;
        if (_collider != null) _collider.enabled = true;
        if (_spriteRenderer != null) _spriteRenderer.enabled = true;

        Debug.Log("[Bullet] Enabled");
    }

    private void OnDisable()
    {
        // Disable physics
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        if (_collider != null) _collider.enabled = false;
        if (_spriteRenderer != null) _spriteRenderer.enabled = false;
    }

    private void Start()
    {
        // Set initial velocity
        if (_rb != null)
        {
            _rb.linearVelocity = _direction.normalized * speed;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Update()
    {   Debug.Log($"Bullet Pos = {transform.position}");
        // Check lifetime and destroy if exceeded
        if (Time.time - _spawnTime > lifetime)
        {
            DestroyBullet();
            return;
        }

        // Rotate bullet towards movement direction
        if (_rb != null && _rb.linearVelocity.magnitude > 0)
        {
            float angle = Mathf.Atan2(_rb.linearVelocity.y, _rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Already hit और penetrating नहीं है
        if (_hasHit && !penetrating)
            return;

        // Player को hit मत करो (खुद को नहीं)
        if (collision.CompareTag("Player"))
            return;

        // Bullet से ignore करो
        if (collision.CompareTag("Bullet"))
            return;

        // Hit event भेजो
        OnBulletHit?.Invoke(transform.position, damage, this);
        
        Debug.Log($"[Bullet] Hit '{collision.gameObject.name}' at {transform.position}");

        // अगर penetrating नहीं है तो destroy करो
        if (!penetrating)
        {
            _hasHit = true;
            DestroyBullet();
        }
    }

    private void DestroyBullet()
    {
        // Pool manager से return करो
        if (GameManager.Instance != null)
        {
            PoolManager poolManager = GameManager.Instance.GetPoolManager();
            if (poolManager != null && poolManager.PoolExists("bullet"))
            {
                poolManager.ReturnPooledObject("bullet", gameObject);
                return;
            }
        }

        // अगर pool नहीं है तो directly destroy करो
        Destroy(gameObject);
    }

    // ============== PUBLIC INITIALIZATION ==============

    public void Initialize(Vector2 bulletDirection, float bulletSpeed, float bulletDamage, 
                          float bulletLifetime, bool isPenetrating, Color color, float size)
    {
        _direction = bulletDirection;
        speed = bulletSpeed;
        damage = bulletDamage;
        lifetime = bulletLifetime;
        penetrating = isPenetrating;
        bulletColor = color;
        bulletSize = size;

        // Apply visuals
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = bulletColor;
        }

        transform.localScale = Vector3.one * bulletSize;
        if (_rb != null)
{
    _rb.linearVelocity = _direction.normalized * speed;
    Debug.Log($"Velocity = {_rb.linearVelocity}");
}
        Debug.Log($"Spawn Position = {transform.position}");
        Debug.Log($"[Bullet] Initialized - Speed: {speed}, Damage: {damage}");
    }

    // ============== PUBLIC GETTERS ==============

    public float GetDamage() => damage;
    public Vector2 GetDirection() => _direction;
    public Vector3 GetPosition() => transform.position;
    public bool HasHit() => _hasHit;
    public float GetLifetimeRemaining() => lifetime - (Time.time - _spawnTime);

    // ============== DEBUG ==============

    public void PrintDebugInfo()
    {
        Debug.Log("=== BULLET DEBUG ===");
        Debug.Log($"Position: {transform.position}");
        Debug.Log($"Direction: {_direction}");
        Debug.Log($"Velocity: {_rb.linearVelocity}");
        Debug.Log($"Damage: {damage}");
        Debug.Log($"Lifetime Remaining: {GetLifetimeRemaining()}");
        Debug.Log($"Has Hit: {_hasHit}");
    }
}
