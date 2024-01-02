using Firebase.Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        GameManager.OnAdvance += SimulateGameweek;
        GameManager.OnNewSeasonStarted += StartNewSeason;
        GameManager.OnGameStarted += GetNextGame;
    }

    public List<Match> GetMatches()
    {
        return _seasonMatches;
    }

    public void ReadTeamsFromFile()
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

        // TODO: Only perform this when starting a new save!
        DistributeDraftPicks(_teams);
    }

    public void StartNewSeason()
    {
        _seasonMatches = ConfigManager.Instance.GetCurrentConfig().ScheduleGenerator.GenerateSchedule(_teams);
        _seasonMatches = _seasonMatches.OrderBy(x => x.GetWeek()).ToList();

        if (GameManager.Instance.GetCurrentSeason() != 0)
        {
            if (GameManager.Instance.GetPremiumStatus())
            {
                GameManager.Instance.AddToGems(50);
            }
            else
            {
                GameManager.Instance.AddToGems(10);
            }
        }

        if (_teams[0].GetDraftPicks().Count <= 0)
        {
            DistributeDraftPicks(_teams);
        }
    }

    public void SetTeams(List<Team> teams, int nextMatchIndex, List<Match> matches)
    {
        _teams = teams;
        _teams.ForEach(x => x.ResetEventsFromLoad());

        _nextMatchIndex = nextMatchIndex;
        _seasonMatches = matches;
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

    public List<Player> GetMVPList(int amount)
    {
        List<Player> players = GetAllPlayers();
        players = players.OrderByDescending(x => x.GetLatestSeason().GetMatchStats().Count * (x.GetLatestSeason().GetAveragePoints() * 0.7f + x.GetLatestSeason().GetAverageOfStat("assists") * 0.15f + x.GetLatestSeason().GetAverageOfStat("rebounds") * 0.05f)).Take(amount).ToList();
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

    private void DistributeDraftPicks(List<Team> teams)
    {
        int index = 1;
        for (int round = 1; round < ConfigManager.Instance.GetCurrentConfig().DraftRounds + 1; round++)
        {
            foreach (Team team in teams)
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

    IEnumerator SimulateToNextMatchWithProgress()
    {
        MatchSimulator matchSimulator = ConfigManager.Instance.GetCurrentConfig().MatchSimulator;
        List<Match> matchList = new List<Match>();
        int myTeamID = GameManager.Instance.GetTeamID();

        for (int i = _nextMatchIndex; i < GetMatches().Count; i++)
        {
            int index = i;
            if (GetMatches()[i].GetMatchStatus()) continue;

            if (GetMatches()[i].GetHomeTeamID() == myTeamID || GetMatches()[i].GetAwayTeamID() == myTeamID) break;

            matchList.Add(GetMatches()[index]);
        }

        Debug.Log($"Simulating {matchList.Count} matches");

        foreach (Match match in matchList)
        {
            matchSimulator.SimulateMatch(match, -1);
            //float progress = (float)(i + 1) / matchList.Count;
            //Debug.Log((int)(100 * progress) + "%");
            yield return null;
        }

        SortStandings();
        GetNextGame(SeasonStage.RegularSeason, GameManager.Instance.GetCurrentWeek());
        Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, GetTeam(GameManager.Instance.GetTeamID()));
    }

    IEnumerator SimulateMatchesWithProgress(List<Match> matchesToSim)
    {
        int matches = matchesToSim.Count;
        Debug.Log($"Simming {matches} matches");
        MatchSimulator matchSimulator = ConfigManager.Instance.GetCurrentConfig().MatchSimulator;
        bool forceWin = GameManager.Instance.GetCurrentForceWinState();
        Match myMatch = null;
        for (int i = 0; i < matches; i++)
        {
            Match match = matchesToSim[i];

            if (forceWin)
            {
                matchSimulator.SimulateMatch(match, GameManager.Instance.GetTeamID());
            } else
            {
                matchSimulator.SimulateMatch(match, -1);
            }

            float progress = (float)(i + 1) / matches;
            Debug.Log((int)(100 * progress) + "%");

            if (match.IsMyTeamMatch(GameManager.Instance.GetTeamID()))
            {
                CheckMyTeamResult(match);
                myMatch = match;
            }
            yield return null;
        }

        SortStandings();
        
        //Navigation.Instance.GoToScreen(true, CanvasKey.Standings, GetTeams());
        //OnRegularSeasonFinished?.Invoke(_teams);
    }

    private void CheckMyTeamResult(Match match)
    {
        int teamID = GameManager.Instance.GetTeamID();

        FirebaseAnalytics.LogEvent("game_played");

        if (match.IsHomeTeam(teamID))
        {
            (int, int) stat = match.GetWinStatForTeam(teamID);

            if (stat.Item1 > 0)
            {
                FirebaseAnalytics.LogEvent("game_win", new Parameter("points_scored", match.GetScore().Item1), new Parameter("points_conceded", match.GetScore().Item2));
            } else if (stat.Item2 > 0)
            {
                FirebaseAnalytics.LogEvent("game_loss", new Parameter("points_scored", match.GetScore().Item1), new Parameter("points_conceded", match.GetScore().Item2));
            }
        } else
        {
            (int, int) stat = match.GetWinStatForTeam(teamID);

            if (stat.Item1 > 0)
            {
                FirebaseAnalytics.LogEvent("game_loss", new Parameter("points_scored", match.GetScore().Item2), new Parameter("points_conceded", match.GetScore().Item1));
            }
            else if (stat.Item2 > 0)
            {
                FirebaseAnalytics.LogEvent("game_win", new Parameter("points_scored", match.GetScore().Item2), new Parameter("points_conceded", match.GetScore().Item1));
            }
        }
    }

    private void SortStandings()
    {
        _teams = _teams.OrderByDescending(x => x.GetCurrentSeasonStats().GetWinPercentage()).ToList();

        int seedIndex = 0;
        _teams.ForEach(x => { x.SetSeed(seedIndex); seedIndex++; });
    }

    private void SimulateGameweek(SeasonStage seasonStage, int week)
    {
        switch (seasonStage)
        {
            case SeasonStage.RegularSeason:
                List<Match> matches = GetMatchesFromWeek(week);
                if (matches.Count > 0)
                {
                    if (MyTeamInMatches(matches))
                    {
                        StartCoroutine(TransitionAnimation.Instance.StartTransitionWithWaitForCompletion(() => { GetNextGame(seasonStage, week); GetTeam(GameManager.Instance.GetTeamID()); Navigation.Instance.GoToScreen(false, CanvasKey.MatchResult, matches.First(x => x.IsMyTeamMatch(GameManager.Instance.GetTeamID()))); }, SimulateMatchesWithProgress(matches)));
                    } else
                    {
                        StartCoroutine(TransitionAnimation.Instance.StartTransitionWithWaitForCompletion(() => { GetNextGame(seasonStage, week); Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, GetTeam(GameManager.Instance.GetTeamID())); }, SimulateToNextMatchWithProgress()));
                    }
                } else
                {
                    OnRegularSeasonFinished?.Invoke(_teams, SeasonStage.Playoffs);
                }
                break;
            case SeasonStage.Playoffs:
                if (PlayoffSystem.Instance.IsTeamInPlayoffs() && PlayoffSystem.Instance.MyTeamSeriesOver())
                {
                    StartCoroutine(TransitionAnimation.Instance.StartTransitionWithWaitForCompletion(() => { }, PlayoffSystem.Instance.SimulateRestOfPlayoffRound()));
                } else if (PlayoffSystem.Instance.IsTeamInPlayoffs())
                {
                    StartCoroutine(TransitionAnimation.Instance.StartTransitionWithWaitForCompletion(() => { }, PlayoffSystem.Instance.SimulateGameweekInPlayoffRound()));
                }
                else
                {
                    StartCoroutine(TransitionAnimation.Instance.StartTransitionWithWaitForCompletion(() => { }, PlayoffSystem.Instance.SimulateEntirePlayoffs()));
                }
                break;
            case SeasonStage.OffSeason:
                Debug.Log("We are now in the offseason");
                break;
            default:
                break;
        }
    }

    private bool MyTeamInMatches(List<Match> matches)
    {
        foreach (Match match in matches)
        {
            if (match.GetHomeTeamID() == GameManager.Instance.GetTeamID() || match.GetAwayTeamID() == GameManager.Instance.GetTeamID())
            {
                return true;
            }
        }

        return false;
    }

    public bool HasNextGame(int week = 0)
    {
        if (week == 0)
            week = GameManager.Instance.GetCurrentWeek();

        int myTeamID = GameManager.Instance.GetTeamID();
        return _seasonMatches.Where(x => x.GetWeek() == week + 1 && (x.GetHomeTeamID() == myTeamID || x.GetAwayTeamID() == myTeamID)).ToList().Count > 0;
    }

    public void GetNextGame(SeasonStage seasonStage, int week)
    {
        if (seasonStage != SeasonStage.RegularSeason) return;
        if (!HasNextGame()) return;

        if (week < _seasonMatches.Last().GetWeek())
        {
            int myTeamID = GameManager.Instance.GetTeamID();
            Match match = _seasonMatches.Where(x => x.GetWeek() == week + 1 && (x.GetHomeTeamID() == myTeamID || x.GetAwayTeamID() == myTeamID)).ToList()[0];
            Debug.Log($"{match.GetHomeTeamID()} - {match.GetAwayTeamID()}");
            _nextMatchIndex = _seasonMatches.IndexOf(match);
        }
    }

    public Match GetNextMatchData()
    {
        return _seasonMatches[_nextMatchIndex];
    }
}
