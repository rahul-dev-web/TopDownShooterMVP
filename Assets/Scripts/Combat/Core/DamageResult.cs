/// <summary>
/// Result returned after a damage transaction.
/// Keeps callers from inspecting concrete Health implementations.
/// </summary>
public readonly struct DamageResult
{
    public readonly bool Applied;
    public readonly bool Killed;
    public readonly float DamageApplied;
    public readonly float RemainingHealth;

    public DamageResult(bool applied, bool killed, float damageApplied, float remainingHealth)
    {
        Applied = applied;
        Killed = killed;
        DamageApplied = damageApplied;
        RemainingHealth = remainingHealth;
    }
}
