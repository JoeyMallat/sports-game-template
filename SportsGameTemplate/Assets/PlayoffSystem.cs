using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayoffSystem : MonoBehaviour
{
    [SerializeField] int _currentRound;
    [SerializeField] List<PlayoffRound> _playoffRounds;

    private void Awake()
    {
        LeagueSystem.OnRegularSeasonFinished += GetPlayoffTeams;
        PlayoffRound.OnPlayoffRoundFinished += InitializeNextRound;
    }

    private void GetPlayoffTeams(List<Team> standings)
    {
        List<Team> _playoffTeams = new List<Team>();

        _playoffTeams.AddRange(standings.GetRange(0, 16));

        _currentRound = 0;
        SetTeamsInRound(_currentRound, _playoffTeams);
    }

    private void InitializeNextRound(List<Team> advancingTeams)
    {
        if (_currentRound < 4)
        {
            SetTeamsInRound(_currentRound, advancingTeams);
        }
    }

    private void SetTeamsInRound(int round, List<Team> teamsInRound)
    {
        _currentRound++;

        if (round == 0)
        {
            int lastIndex = teamsInRound.Count;
            for (int i = 0; i < teamsInRound.Count / 2; i++)
            {
                int matchupIndex = i % 2 == 0 ? i : lastIndex / 2 - i;
                _playoffRounds[round].AddMatchup(matchupIndex, teamsInRound[i].GetTeamID(), i + 1, teamsInRound[lastIndex - i - 1].GetTeamID(), lastIndex - i);
            }
        }
        else if (round < 3)
        {
            int lastIndex = teamsInRound.Count;
            int matchupIndex = 0;
            for (int i = 0; i < lastIndex; i+=2)
            {
                _playoffRounds[round].AddMatchup(matchupIndex, teamsInRound[i].GetTeamID(), 0, teamsInRound[i + 1].GetTeamID(), 0);
                matchupIndex++;
            }
        } else
        {
            _playoffRounds[round].AddMatchup(0, teamsInRound[0].GetTeamID(), 0, teamsInRound[1].GetTeamID(), 0);
        }

        _playoffRounds[round].SimMatches();
    }
}
