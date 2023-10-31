using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeagueSystem : MonoBehaviour
{
    public static LeagueSystem Instance;

    [SerializeField] List<Team> _teams;
    [SerializeField] List<Match> _seasonMatches;

    [SerializeField] int _nextMatchIndex;

    public static event Action<List<Team>, SeasonStage> OnRegularSeasonFinished;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(this); }

        ReadTeamsFromFile();

        GameManager.OnAdvance += SimulateGameweek;
        //GameManager.OnAdvance += GetNextGame;
        GameManager.OnGameStarted += GetNextGame;
    }

    private void ReadTeamsFromFile()
    {
        _teams = new List<Team>();

        TextAsset teamFile = ConfigManager.Instance.GetCurrentConfig().LeagueFile;
        SquadCreator squadCreator = new SquadCreator();

        string[] rows = teamFile.text.Split('\n');

        int id = 0;
        foreach (string row in rows)
        {
            string[] teamData = row.Split(',');
            string teamName = teamData[0];
            int.TryParse(teamData[1], out int rating);

            List<Player> players = squadCreator.CreateSquad(id, rating);
            Team newTeam = new Team(id, teamName, rating, players);
            _teams.Add(newTeam);

            id++;
        }

        DistributeDraftPicks();
        _seasonMatches = ConfigManager.Instance.GetCurrentConfig().ScheduleGenerator.GenerateSchedule(_teams);
        _seasonMatches = _seasonMatches.OrderBy(x => x.GetWeek()).ToList();
    }

    public List<Match> GetMatchesForTeam(Team team)
    {
        List<Match> matchesForTeam = new List<Match>();
        int teamID = team.GetTeamID();

        foreach (Match match in _seasonMatches)
        {
            if (match.GetHomeTeamID() == teamID)
            {
                matchesForTeam.Add(match);
            } else if (match.GetAwayTeamID() == teamID)
            {
                matchesForTeam.Add(match);
            }
        }

        return matchesForTeam;
    }

    public List<Match> GetMatchesFromWeek(int week)
    {
        List<Match> matches = new List<Match>();

        foreach (var match in _seasonMatches)
        {
            if (match.GetWeek() > week) return matches;

            if (match.GetWeek() == week)
            {
                matches.Add(match);
            }
        }

        return matches;
    }

    public Team GetTeam(int id)
    {
        return _teams.Where(x => x.GetTeamID() == id).ToList()[0];
    }

    public List<Team> GetTeamsSortedByID()
    {
        _teams = _teams.OrderBy(x => x.GetTeamID()).ToList();
        return _teams;
    }

    public List<Team> GetTeams()
    {
        _teams = _teams.OrderByDescending(x => x.GetCurrentSeasonStats().GetWinPercentage()).ToList();
        return _teams;
    }

    public List<Team> GetTeamsWithoutTeam(int teamID)
    {
        return GetTeamsSortedByID().Where(x => x.GetTeamID() != teamID).ToList();
    }

    public List<Player> GetAllPlayers()
    {
        List<Player> players = new();

        foreach (Team team in _teams)
        {
            players.AddRange(team.GetPlayersFromTeam());
        }

        return players;
    }

    public List<Player> GetTopListOfStat(string stat, int amount)
    {
        List<Player> players = GetAllPlayers();
        players = players.OrderByDescending(x => x.GetLatestSeason().GetAverageOfStat(stat)).Take(amount).ToList();
        return players;
    }
    

    public List<Player> GetTopListOfStat(List<string> stats, int amount)
    {
        List<Player> players = GetAllPlayers();
        players = players.OrderByDescending(x => x.GetLatestSeason().GetAverageOfStat(stats)).Take(amount).ToList();
        return players;
    }

    private void DistributeDraftPicks()
    {
        // TODO: Base off season standings and use weighted lottery if enabled

        // Temporary solution
        int index = 1;
        for (int round = 1; round < ConfigManager.Instance.GetCurrentConfig().DraftRounds + 1; round++)
        {
            foreach (Team team in _teams)
            {
                team.AddDraftPick(new DraftPick(round, index));

                // Don't add to index when it's the last pick of the round
                if ((index == 30 && round % 2 != 0) || (index == 1 && round % 2 == 0)) break;

                // Add to index
                if (round % 2 == 0) index--;
                else index++;
            }
        }
    }

    public void SimulateSeason()
    {
        StartCoroutine(SimulateMatchesWithProgress(_seasonMatches));

        //ConfigManager.Instance.GetCurrentConfig().MatchSimulator.SimulateMatch(_seasonMatches[0]);
    }

    IEnumerator SimulateMatchesWithProgress(List<Match> matchesToSim)
    {
        int matches = matchesToSim.Count;
        Debug.Log($"Simming {matches} matches");
        MatchSimulator matchSimulator = ConfigManager.Instance.GetCurrentConfig().MatchSimulator;
        for (int i = 0; i < matches; i++)
        {
            Match match = matchesToSim[i];
            matchSimulator.SimulateMatch(match);

            float progress = (float)(i + 1) / matches;
            Debug.Log((int)(100 * progress) + "%");

            //Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, GetTeam(GameManager.Instance.GetTeamID()));

            yield return null;
        }

        SortStandings();
        Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, GetTeam(GameManager.Instance.GetTeamID()));
        //Navigation.Instance.GoToScreen(true, CanvasKey.Standings, GetTeams());
        //OnRegularSeasonFinished?.Invoke(_teams);
    }

    private void SortStandings()
    {
        _teams = _teams.OrderByDescending(x => x.GetCurrentSeasonStats().GetWinPercentage()).ToList();
    }

    private void SimulateGameweek(SeasonStage seasonStage, int week)
    {
        switch (seasonStage)
        {
            case SeasonStage.RegularSeason:
                List<Match> matches = GetMatchesFromWeek(week);
                if (matches.Count > 0)
                {
                    StartCoroutine(TransitionAnimation.Instance.StartTransitionWithWaitForCompletion(() => { GetNextGame(seasonStage, week); }, SimulateMatchesWithProgress(matches)));
                } else
                {
                    OnRegularSeasonFinished?.Invoke(_teams, SeasonStage.Playoffs);
                }
                break;
            case SeasonStage.Playoffs:
                StartCoroutine(TransitionAnimation.Instance.StartTransitionWithWaitForCompletion(() => { }, PlayoffSystem.Instance.SimulateGameweekInPlayoffRound()));
                break;
            case SeasonStage.OffSeason:
                Debug.Log("We are now in the offseason");
                break;
            default:
                break;
        }
    }

    public bool HasNextGame()
    {
        int myTeamID = GameManager.Instance.GetTeamID();
        return _seasonMatches.Where(x => x.GetWeek() == GameManager.Instance.GetCurrentWeek() + 1 && (x.GetHomeTeamID() == myTeamID || x.GetAwayTeamID() == myTeamID)).ToList().Count > 0;
    }

    private void GetNextGame(SeasonStage seasonStage, int week)
    {
        if (seasonStage != SeasonStage.RegularSeason) return;
        if (!HasNextGame()) return;

        if (week < _seasonMatches.Last().GetWeek() - 1)
        {
            int myTeamID = GameManager.Instance.GetTeamID();
            Match match = _seasonMatches.Where(x => x.GetWeek() == week + 1 && (x.GetHomeTeamID() == myTeamID || x.GetAwayTeamID() == myTeamID)).ToList()[0];
            _nextMatchIndex = _seasonMatches.IndexOf(match);
        }
    }

    public Match GetNextMatchData()
    {
        return _seasonMatches[_nextMatchIndex];
    }
}
