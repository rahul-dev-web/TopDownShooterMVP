using UnityEngine;

/// <summary>
/// Immutable payload emitted when a combat entity dies.
/// Can later be replicated by the authoritative multiplayer layer.
/// </summary>
public readonly struct KillEvent
{
    public readonly GameObject Killer;
    public readonly GameObject Victim;
    public readonly DamageInfo DamageInfo;
    public readonly bool HasKiller;

    public KillEvent(GameObject killer, GameObject victim, DamageInfo damageInfo, bool hasKiller)
    {
        Killer = killer;
        Victim = victim;
        DamageInfo = damageInfo;
        HasKiller = hasKiller;
    }
}
