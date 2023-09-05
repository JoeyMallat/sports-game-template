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
            teamsToPlay = UpdatePossibleOpponents(teamsToPlay);
            int gamesToGenerate = ConfigManager.Instance.GetCurrentConfig().GamesPerTeamInRegularSeason;
            Debug.Log($"Games to generate: {gamesToGenerate}");
            int limit = team.GetMatchdayCount();
            Debug.Log($"Limit: {limit}");

            for (int i = 0; i < gamesToGenerate - limit; i++)
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

                teamsToPlay = UpdatePossibleOpponents(teamsToPlay, opponent);
            }
        }

        return matches;
    }

    private List<Team> UpdatePossibleOpponents(List<Team> teamsToPlay)
    {
        List<Team> possible = new List<Team>();

        foreach (Team team in teamsToPlay)
        {
            if (team.GetAvailableMatchdays().Count > 0)
            {
                possible.Add(team);
            }
        }

        return possible;
    }



    private List<Team> UpdatePossibleOpponents(List<Team> teamsToPlay, Team toCheck)
    {
        if (teamsToPlay.Contains(toCheck))
        {
            if (toCheck.GetMatchdayCount() < ConfigManager.Instance.GetCurrentConfig().GamesPerTeamInRegularSeason)
            {
                return teamsToPlay;
            } else
            {
                teamsToPlay.Remove(toCheck);
            }
        }

        return teamsToPlay;
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
        int tries = 0;

        while (opponentID < 0 && teamID != opponentID && tries < 30)
        {
            int random = UnityEngine.Random.Range(0, possibleOpponents.Count);
            opponentID = possibleOpponents[random].GetTeamID();
            tries++;
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
