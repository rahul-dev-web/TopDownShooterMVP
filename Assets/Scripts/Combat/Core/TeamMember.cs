using System;
using UnityEngine;

/// <summary>
/// Attaches team identity to a combat entity. Team is intentionally independent
/// from networking so the same component works in local play and online sessions.
/// </summary>
public class TeamMember : MonoBehaviour
{
    [SerializeField] private TeamId team = TeamId.None;

    public TeamId Team => team;
    public static event Action<TeamMember, TeamId, TeamId> OnTeamChanged;

    public void SetTeam(TeamId newTeam)
    {
        if (team == newTeam)
            return;

        TeamId previous = team;
        team = newTeam;
        OnTeamChanged?.Invoke(this, previous, team);
    }

    public bool IsSameTeam(TeamMember other)
    {
        return other != null && team != TeamId.None && team == other.team;
    }
}
