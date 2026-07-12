/// <summary>
/// EnemyAI - Basic enemy behavior
/// 
/// Chase player
/// Attack when close
/// Simple pathfinding
/// </summary>

using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // ============== SERIALIZED FIELDS ==============

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 1f;

    // ============== PRIVATE FIELDS ==============

    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    private PlayerController _targetPlayer;
    private float _lastAttackTime = -999f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();

        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 0;
        }

        Debug.Log("[EnemyAI] Initialized");
    }

    private void Start()
    {
        // Find player
        _targetPlayer = FindAnyObjectByType<PlayerController>();


    // Null check
    if (_targetPlayer == null)
    {
        Debug.LogError("[EnemyAI] Player not found! Disabling AI");
        enabled = false;
        return;
    }
    }

    private void FixedUpdate()
    {
        if (_targetPlayer == null || !_targetPlayer.IsAlive())
            return;

        Vector3 dirToPlayer = (_targetPlayer.GetWorldPosition() - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, _targetPlayer.GetWorldPosition());

        // Chase if in detection range
        if (distToPlayer < detectionRange)
        {
            ChasePlayer(dirToPlayer);

            // Attack if close enough
            if (distToPlayer < attackRange)
            {
                AttackPlayer();
            }
        }
        else
        {
            // Stop moving
            _rb.linearVelocity = Vector2.zero;
        }
    }

    private void ChasePlayer(Vector3 direction)
    {
        _rb.linearVelocity = direction * moveSpeed;

        // Flip sprite based on direction
        if (direction.x < 0)
            _sprite.flipX = true;
        else if (direction.x > 0)
            _sprite.flipX = false;
    }

    private void AttackPlayer()
    {
        if (Time.time - _lastAttackTime < attackCooldown)
            return;

        // Deal damage to player
        Health playerHealth = _targetPlayer.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage, transform.position);
            GameManager.Instance?.GetAudioManager()?.PlaySFX("enemy_attack");
            _lastAttackTime = Time.time;

            Debug.Log($"[EnemyAI] Attacked player! -{attackDamage} HP");
        }
    }

    public void PrintDebugInfo()
    {
        Debug.Log("=== ENEMY AI DEBUG ===");
        if (_targetPlayer != null)
        {
            float dist = Vector3.Distance(transform.position, _targetPlayer.GetWorldPosition());
            Debug.Log($"Distance to player: {dist}");
            Debug.Log($"Detection range: {detectionRange}");
            Debug.Log($"Attack range: {attackRange}");
        }
    }
}