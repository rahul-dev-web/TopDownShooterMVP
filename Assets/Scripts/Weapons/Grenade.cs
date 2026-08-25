using System;
using UnityEngine;

/// <summary>
/// Timed throwable explosive. Uses ExplosionDamage for all combat resolution.
/// The projectile itself owns movement/lifetime; damage stays in the shared combat layer.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(ExplosionDamage))]
public class Grenade : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float fuseTime = 2.5f;
    [SerializeField, Min(0f)] private float throwForce = 10f;
    [SerializeField, Min(0f)] private float angularVelocity = 360f;
    [SerializeField] private bool explodeOnImpact;
    [SerializeField] private GameObject explosionEffectPrefab;

    private Rigidbody2D _rb;
    private ExplosionDamage _explosionDamage;
    private GameObject _owner;
    private float _spawnTime;
    private bool _exploded;

    public static event Action<Grenade, int> OnGrenadeExploded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _explosionDamage = GetComponent<ExplosionDamage>();
    }

    private void OnEnable()
    {
        _spawnTime = Time.time;
        _exploded = false;
    }

    private void OnDisable()
    {
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
        _owner = null;
    }

    private void Update()
    {
        if (!_exploded && Time.time - _spawnTime >= fuseTime)
            Explode();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_exploded || !explodeOnImpact || collision == null)
            return;

        if (_owner != null && collision.transform.IsChildOf(_owner.transform))
            return;

        Explode();
    }

    public void Throw(Vector2 direction, GameObject owner = null)
    {
        _owner = owner;
        _exploded = false;
        _spawnTime = Time.time;

        Vector2 normalizedDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        _rb.linearVelocity = normalizedDirection * throwForce;
        _rb.angularVelocity = angularVelocity;
    }

    public void Explode()
    {
        if (_exploded)
            return;

        _exploded = true;
        _rb.linearVelocity = Vector2.zero;
        int damagedTargets = _explosionDamage != null ? _explosionDamage.Explode(_owner) : 0;

        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        OnGrenadeExploded?.Invoke(this, damagedTargets);
        GameManager.Instance?.GetAudioManager()?.PlaySFX("explosion");

        Destroy(gameObject);
    }

    public GameObject GetOwner() => _owner;
}
