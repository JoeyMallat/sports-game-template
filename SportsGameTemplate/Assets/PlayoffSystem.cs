using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayoffSystem : MonoBehaviour
{
    [SerializeField] List<Team> _playoffTeams;
    [SerializeField] List<PlayoffRound> _playoffRounds;

    private void Awake()
    {
        LeagueSystem.OnRegularSeasonFinished += GetPlayoffTeams;
    }

    private void GetPlayoffTeams(List<Team> standings)
    {
        _playoffTeams = new List<Team>();

        _playoffTeams.AddRange(standings.GetRange(0, 16));

        int lastIndex = _playoffTeams.Count;
        for (int i = 0; i < _playoffTeams.Count / 2; i++)
        {
            int matchupIndex = i % 2 == 0 ? i : lastIndex / 2 - i;
            _playoffRounds[0].AddMatchup(matchupIndex, _playoffTeams[i].GetTeamID(), i + 1, _playoffTeams[lastIndex - i - 1].GetTeamID(), lastIndex - i);
        }
    }
}
