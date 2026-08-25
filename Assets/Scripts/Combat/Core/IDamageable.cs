/// <summary>
/// Common contract for anything that can receive combat damage.
/// Designed to work for local enemies, players and future network entities.
/// </summary>
public interface IDamageable
{
    bool IsAlive { get; }
    float CurrentHealth { get; }
    float MaxHealth { get; }

    bool ApplyDamage(DamageInfo damageInfo);
}
