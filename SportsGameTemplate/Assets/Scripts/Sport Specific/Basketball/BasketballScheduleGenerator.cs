using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BasketballScheduleGenerator : ScheduleGenerator
{
    public List<Match> GenerateSchedule(List<Team> teams)
    {
        List<Match> matches = new List<Match>();
        Debug.Log($"Teams in league: {teams.Count}");

        List<Team> teamsToPlay = teams;
        teamsToPlay = teamsToPlay.Shuffle();

        foreach (Team team in teams)
        {
            int limit = team.GetMatchdays().Count;
            teamsToPlay = UpdatePossibleOpponents(teamsToPlay);

            for (int i = 0; i < ConfigManager.Instance.GetCurrentConfig().GamesPerTeamInRegularSeason - limit; i++)
            {
                int opponentID = GetRandomOpponent(team.GetTeamID(), teamsToPlay);
                Team opponent = LeagueSystem.Instance.GetTeam(opponentID);

                int week = FindFreeMatchday(team, opponent);
                team.AddMatchdayAsTaken(week);
                opponent.AddMatchdayAsTaken(week);

                if (GetHomeOrAway())
                {
                    matches.Add(new Match(0, week, team.GetTeamID(), opponentID));
                } else
                {
                    matches.Add(new Match(0, week, opponentID, team.GetTeamID()));
                }
            }
        }

        return matches;
    }

    private List<Team> UpdatePossibleOpponents(List<Team> teamsToPlay)
    {
        List<Team> possible = new List<Team>();

        foreach (Team team in teamsToPlay)
        {
            if (team.GetMatchdays().Count < ConfigManager.Instance.GetCurrentConfig().GamesPerTeamInRegularSeason)
            {
                possible.Add(team);
            }
        }

        return possible;
    }

    private int FindFreeMatchday(Team team, Team opponent)
    {
        List<int> availableTeam = team.GetAvailableMatchdays();
        List<int> availableOpponent = opponent.GetAvailableMatchdays();

        return availableTeam.Intersect(availableOpponent).ToList()[0];
    }

    private int GetRandomOpponent(int teamID, List<Team> possibleOpponents)
    {
        int opponentID = -1;

        while (opponentID < 0 || teamID == opponentID)
        {
            int random = UnityEngine.Random.Range(0, possibleOpponents.Count);
            
            if (possibleOpponents[random].GetMatchdays().Count < ConfigManager.Instance.GetCurrentConfig().GamesPerTeamInRegularSeason)
            {
                opponentID = possibleOpponents[random].GetTeamID();
            }
        }

        return opponentID;
    }

    private bool GetHomeOrAway()
    {
        if (UnityEngine.Random.Range(0f, 1f) > .5f)
        {
            return true;
        } else
        {
            return false;
        }
    }
}
