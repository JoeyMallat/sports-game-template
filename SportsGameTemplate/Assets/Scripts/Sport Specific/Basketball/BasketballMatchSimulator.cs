using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketballMatchSimulator : MatchSimulator
{
    [Header("Match settings")]
    [SerializeField] const int _numberOfPeriods = 4;
    [SerializeField] const int _minutesPerPeriod = 12;
    [SerializeField] const bool _overTimePossible = true;

    [Header("Match simulation settings")]
    [SerializeField] float _scoringRate;

    public void SimulateMatch(Match match)
    {
        Team homeTeam = LeagueSystem.Instance.GetTeam(match.GetHomeTeamID());
        Team awayTeam = LeagueSystem.Instance.GetTeam(match.GetAwayTeamID());

        int homeTeamModifier = homeTeam.GetAverageTeamRating() - awayTeam.GetAverageTeamRating();
        int awayTeamModifier = awayTeam.GetAverageTeamRating() - homeTeam.GetAverageTeamRating();

        int homeTeamGoals = UnityEngine.Random.Range(80 - Mathf.Abs(homeTeamModifier), 120 - Mathf.Abs(homeTeamModifier));
        int awayTeamGoals = UnityEngine.Random.Range(80 - Mathf.Abs(awayTeamModifier), 120 - Mathf.Abs(awayTeamModifier));

        match.SetResult(homeTeamGoals, awayTeamGoals);
    }
}
