using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reusable radial damage producer for grenades and explosive weapons.
/// Damage falls off linearly from the center to the configured radius.
/// </summary>
public class ExplosionDamage : MonoBehaviour
{
    [SerializeField, Min(0f)] private float maxDamage = 60f;
    [SerializeField, Min(0.1f)] private float radius = 3f;
    [SerializeField] private LayerMask damageableLayers = ~0;
    [SerializeField, Min(0f)] private float knockbackForce = 8f;
    [SerializeField] private bool damageFalloff = true;

    public static event Action<DamageInfo, IDamageable> OnExplosionDamageApplied;

    public int Explode(GameObject source = null)
    {
        Vector2 center = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, damageableLayers);
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
        int damagedCount = 0;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (source != null && hit.transform.IsChildOf(source.transform))
                continue;

            Vector2 closestPoint = hit.ClosestPoint(center);
            float distance = Vector2.Distance(center, closestPoint);
            float normalizedDistance = Mathf.Clamp01(distance / radius);
            float finalDamage = damageFalloff
                ? Mathf.Lerp(maxDamage, 0f, normalizedDistance)
                : maxDamage;

            if (finalDamage <= 0f)
                continue;

            Vector2 direction = (closestPoint - center).normalized;
            if (direction.sqrMagnitude <= 0.0001f)
                direction = UnityEngine.Random.insideUnitCircle.normalized;

            DamageInfo info = new DamageInfo(
                finalDamage,
                DamageType.Explosion,
                source,
                closestPoint,
                direction,
                knockbackForce * (damageFalloff ? 1f - normalizedDistance : 1f));

            MonoBehaviour[] behaviours = hit.GetComponentsInParent<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is not IDamageable damageable || damagedTargets.Contains(damageable))
                    continue;

                damagedTargets.Add(damageable);
                if (damageable.IsAlive && damageable.ApplyDamage(info))
                {
                    damagedCount++;
                    OnExplosionDamageApplied?.Invoke(info, damageable);
                }
                break;
            }
        }

        return damagedCount;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
