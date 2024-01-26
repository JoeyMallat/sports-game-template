using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayoffSystem : MonoBehaviour
{
    public static PlayoffSystem Instance;

    [SerializeField] int _currentRound;
    [SerializeField] List<PlayoffRound> _playoffRounds;

    List<Team> _playoffTeams;
    PlayoffMatchup _nextMatchup;

    public static event Action<List<Team>, SeasonStage> OnPlayoffsFinished;

    private void Awake()
    {
        LeagueSystem.OnRegularSeasonFinished += GetPlayoffTeams;
        PlayoffRound.OnPlayoffRoundFinished += InitializeNextRound;

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void GetPlayoffTeams(List<Team> standings, SeasonStage seasonStage)
    {
        _playoffTeams = new List<Team>();

        _playoffTeams.AddRange(standings.GetRange(0, 16));

        _currentRound = 0;
        SetTeamsInRound(_currentRound, _playoffTeams);
    }

    public bool IsTeamInPlayoffs()
    {
        List<PlayoffMatchup> rounds = _playoffRounds[_currentRound].GetMatchups().Where(x => x.GetHomeTeamID() == GameManager.Instance.GetTeamID() || x.GetAwayTeamID() == GameManager.Instance.GetTeamID()).ToList();

        if (rounds.Count == 0) { return false; }

        PlayoffMatchup myMatchup = _playoffRounds[_currentRound].GetMatchups().Where(x => x.GetHomeTeamID() == GameManager.Instance.GetTeamID() || x.GetAwayTeamID() == GameManager.Instance.GetTeamID()).ToList()[0];

        if (myMatchup.GetSeriesWinner() != -1)
        {
            return myMatchup.GetSeriesWinner() == GameManager.Instance.GetTeamID();
        }
        else
        {
            return true;
        }
    }

    private void InitializeNextRound(List<Team> advancingTeams)
    {
        if (_currentRound < 3)
        {
            _currentRound++;
            SetTeamsInRound(_currentRound, advancingTeams);
        }
        else
        {
            OnPlayoffsFinished?.Invoke(new List<Team>(), SeasonStage.OffSeason);
        }
    }

    public int GetPlayoffRound()
    {
        return _currentRound;
    }

    private void SetTeamsInRound(int round, List<Team> teamsInRound)
    {
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
            for (int i = 0; i < lastIndex; i += 2)
            {
                _playoffRounds[round].AddMatchup(matchupIndex, teamsInRound[i].GetTeamID(), 0, teamsInRound[i + 1].GetTeamID(), 0);
                matchupIndex++;
            }
        }
        else
        {
            _playoffRounds[round].AddMatchup(0, teamsInRound[0].GetTeamID(), 0, teamsInRound[1].GetTeamID(), 0);
        }

        SetNextMatch();
        //_playoffRounds[round].SimMatches();
    }

    public void SetLoadData(int playoffRound, List<PlayoffMatchup> playoffMatchups)
    {
        _currentRound = playoffRound;

        int startIndex = 0;

        foreach (var round in _playoffRounds)
        {
            int matchupIndex = 0;
            for (int i = startIndex; i < round.GetMatchups().Count + startIndex; i++)
            {
                round.SetMatchup(matchupIndex, playoffMatchups[i]);
                matchupIndex++;
            }
            startIndex += round.GetMatchups().Count;
        }
    }

    public IEnumerator SimulateGameweekInPlayoffRound()
    {
        _playoffRounds[_currentRound].SimMatches();
        SetNextMatch();
        Navigation.Instance.GoToScreen(false, CanvasKey.MatchResult, GetNextMatchData().GetLastPlayedMatch());
        yield return null;
    }

    public IEnumerator SimulateRestOfPlayoffRound()
    {
        _playoffRounds[_currentRound].SimSeries();
        SetNextMatch();
        Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()));
        yield return null;
    }

    public IEnumerator SimulateEntirePlayoffs()
    {
        for (int i = _currentRound; i < 4; i++)
        {
            _playoffRounds[i].SimSeries();
            yield return null;
        }

        Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()));
    }

    private void SetNextMatch()
    {
        int teamID = GameManager.Instance.GetTeamID();

        if (!IsTeamInPlayoffs()) return;

        _nextMatchup = _playoffRounds[_currentRound].GetMatchups().Where(x => (x.GetHomeTeamID() == teamID || x.GetAwayTeamID() == teamID)).ToList()[0];
    }

    public PlayoffMatchup GetNextMatchData()
    {
        SetNextMatch();
        return _nextMatchup;
    }

    public List<PlayoffMatchup> GetAllPlayoffsMatchups()
    {
        List<PlayoffMatchup> matchups = new List<PlayoffMatchup>();
        foreach (var round in _playoffRounds)
        {
            matchups.AddRange(round.GetMatchups());
        }

        return matchups;
    }

    public bool MyTeamSeriesOver()
    {
        List<PlayoffMatchup> rounds = _playoffRounds[_currentRound].GetMatchups().Where(x => x.GetHomeTeamID() == GameManager.Instance.GetTeamID() || x.GetAwayTeamID() == GameManager.Instance.GetTeamID()).ToList();

        if (rounds.Count == 0) { return true; }

        PlayoffMatchup myMatchup = _playoffRounds[_currentRound].GetMatchups().Where(x => x.GetHomeTeamID() == GameManager.Instance.GetTeamID() || x.GetAwayTeamID() == GameManager.Instance.GetTeamID()).ToList()[0];

        if (myMatchup.GetSeriesWinner() != -1)
        {
            return true;
        }

        return false;
    }
}
