using UnityEngine;

/// <summary>
/// Centralized combat validation. Keeps friendly-fire policy out of individual
/// bullets/grenades and can later be reused by server-authoritative validation.
/// </summary>
public static class DamageRules
{
    /// <summary>
    /// Default local MVP policy. Future match rules can toggle this through a MatchRules asset.
    /// </summary>
    public static bool FriendlyFireEnabled { get; set; }

    public static bool CanDamage(GameObject source, Component target)
    {
        if (target == null)
            return false;

        if (source == null)
            return true;

        if (target.transform.IsChildOf(source.transform))
            return false;

        TeamMember sourceTeam = source.GetComponentInParent<TeamMember>();
        TeamMember targetTeam = target.GetComponentInParent<TeamMember>();

        if (sourceTeam == null || targetTeam == null)
            return true;

        if (sourceTeam.Team == TeamId.None || targetTeam.Team == TeamId.None)
            return true;

        if (sourceTeam.Team != targetTeam.Team)
            return true;

        return FriendlyFireEnabled;
    }

    public static bool CanDamage(GameObject source, Collider2D target)
    {
        return target != null && CanDamage(source, target.transform);
    }
}
