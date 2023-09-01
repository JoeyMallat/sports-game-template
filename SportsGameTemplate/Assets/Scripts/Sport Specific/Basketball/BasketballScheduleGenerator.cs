using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketballScheduleGenerator : ScheduleGenerator
{
    public List<Match> GenerateSchedule(List<Team> teams)
    {
        List<Match> matches = new List<Match>();

        Debug.Log($"Teams in league: {teams.Count}");

        foreach (Team team in teams)
        {
            foreach (Team t in teams)
            {
                if (team.GetTeamID() != t.GetTeamID())
                {
                    matches.Add(new Match(0, 0, team.GetTeamID(), t.GetTeamID()));
                    Debug.Log($"Match: {team.GetTeamName()} - {t.GetTeamName()}");
                }
            }
        }

        return matches;
    }
}
