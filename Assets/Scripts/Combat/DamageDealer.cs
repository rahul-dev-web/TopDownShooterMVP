/// <summary>
/// DamageDealer - Trigger-based damage system
/// 
/// जब कोई object से टकराए तो damage दे
/// Bullets use करेंगे
/// </summary>

using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    // ============== SERIALIZED FIELDS ==============

    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private bool onCollisionOnly = false;
    [SerializeField] private bool destroyAfterHit = true;

    // ============== PRIVATE FIELDS ==============

    private bool _hasDealtDamage = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("[DamageDealer] Trigger Enter : " + collision.gameObject.name);
        if (_hasDealtDamage && destroyAfterHit)
            return;

        // Player को damage मत दो
        if (collision.CompareTag("Player"))
            return;

        // Damage dealer को ignore करो
        if (collision.CompareTag("Bullet"))
            return;

        // Health component ढूंढो
        Health health = collision.GetComponent<Health>();
        if (health != null && health.IsAlive)
        {
            health.TakeDamage(damageAmount, transform.position);
            _hasDealtDamage = true;

            if (destroyAfterHit)
            {
                DestroyObject();
            }
        }
    }

    private void DestroyObject()
    {
        // Pool में return करो अगर available हो
        if (GameManager.Instance != null)
        {
            PoolManager poolManager = GameManager.Instance.GetPoolManager();
            if (poolManager.PoolExists("bullet"))
            {
                poolManager.ReturnPooledObject("bullet", gameObject);
                return;
            }
        }

        // Otherwise destroy
        Destroy(gameObject);
    }

    public void SetDamage(float newDamage)
    {
        damageAmount = newDamage;
    }
}
