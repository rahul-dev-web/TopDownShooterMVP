using UnityEngine;

/// <summary>
/// Shared helpers for combat producers. Keeps target lookup and damage construction
/// consistent across bullets, explosions and future abilities.
/// </summary>
public static class DamageUtility
{
    public static bool TryApplyDamage(
        Collider2D target,
        DamageInfo damageInfo,
        out IDamageable damageable)
    {
        damageable = null;
        if (target == null)
            return false;

        MonoBehaviour[] behaviours = target.GetComponentsInParent<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is not IDamageable candidate)
                continue;

            damageable = candidate;
            if (!candidate.IsAlive)
                return false;

            return candidate.ApplyDamage(damageInfo);
        }

        return false;
    }
}
