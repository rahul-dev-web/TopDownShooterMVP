using UnityEngine;

/// <summary>
/// Immutable-style payload describing a single damage transaction.
/// Future networking can serialize/validate this data at the server boundary.
/// </summary>
public readonly struct DamageInfo
{
    public readonly float Amount;
    public readonly DamageType Type;
    public readonly GameObject Source;
    public readonly Vector2 HitPoint;
    public readonly Vector2 HitDirection;
    public readonly float KnockbackForce;

    public DamageInfo(
        float amount,
        DamageType type = DamageType.Bullet,
        GameObject source = null,
        Vector2 hitPoint = default,
        Vector2 hitDirection = default,
        float knockbackForce = 0f)
    {
        Amount = Mathf.Max(0f, amount);
        Type = type;
        Source = source;
        HitPoint = hitPoint;
        HitDirection = hitDirection;
        KnockbackForce = Mathf.Max(0f, knockbackForce);
    }

    public bool IsValid => Amount > 0f;
}
