using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class PrematchView : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _lineupItemsRoot;
    [SerializeField] GameObject _benchItemsRoot;
    [SerializeField] Button _selectAutomaticallyButton;
    [SerializeField] Button _clearButton;
    [SerializeField] Button _goToGameButton;

    [Header("Team Details")]
    [SerializeField] TextMeshProUGUI _gameTypeText;
    [SerializeField] Image _homeTeamLogo;
    [SerializeField] TextMeshProUGUI _homeTeamName;
    [SerializeField] TextMeshProUGUI _homeTeamPosition;
    [SerializeField] TextMeshProUGUI _homeTeamRecord;
    [SerializeField] TextMeshProUGUI _homeTeamBestPlayer;

    [SerializeField] Image _awayTeamLogo;
    [SerializeField] TextMeshProUGUI _awayTeamName;
    [SerializeField] TextMeshProUGUI _awayTeamPosition;
    [SerializeField] TextMeshProUGUI _awayTeamRecord;
    [SerializeField] TextMeshProUGUI _awayTeamBestPlayer;

    private void Awake()
    {
        Team.OnLineupChanged += SetDetails;
    }

    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;
        if (team.GetTeamID() != GameManager.Instance.GetTeamID()) return;

        List<LineupItem> _lineupItems = _lineupItemsRoot.GetComponentsInChildren<LineupItem>(true).ToList();
        List<BenchItem> _benchItems = _benchItemsRoot.GetComponentsInChildren<BenchItem>(true).ToList();

        List<Player> players = team.GetPlayersFromIDs(team.GetLineup());
        List<Player> restPlayers = GetBench(team, players);

        for (int i = 0; i < _lineupItems.Count; i++)
        {
            int index = i;
            _lineupItems[i].SetPlayerDetails(players[index]);
        }

        for (int i = 0; i < _benchItems.Count; i++)
        {
            if (i < restPlayers.Count)
            {
                int index = i;
                _benchItems[i].gameObject.SetActive(true);
                _benchItems[i].SetPlayerDetails(restPlayers[index]);
            }
            else
            {
                _benchItems[i].gameObject.SetActive(false);
            }
        }

        _selectAutomaticallyButton.onClick.RemoveAllListeners();
        _selectAutomaticallyButton.onClick.AddListener(() => team.GenerateLineup());

        _clearButton.onClick.RemoveAllListeners();
        _clearButton.onClick.AddListener(() => team.EmptyLineup());

        _goToGameButton.ToggleButtonStatus(CheckLineupCompletion(players));
        _goToGameButton.onClick.RemoveAllListeners();
        _goToGameButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.MatchOptions));

        if (PlayoffSystem.Instance.IsTeamInPlayoffs() && GameManager.Instance.GetSeasonStage() == SeasonStage.Playoffs)
        {
            SetTeamDetails(PlayoffSystem.Instance.GetNextMatchData().GetNextMatch());
        } else
        {
            SetTeamDetails(LeagueSystem.Instance.GetNextMatchData());
        }
    }

    public void SetTeamDetails(Match match)
    {
        _gameTypeText.text = "League game";

        Team homeTeam = LeagueSystem.Instance.GetTeam(match.GetHomeTeamID());
        Team awayTeam = LeagueSystem.Instance.GetTeam(match.GetAwayTeamID());

        // Home team
        _homeTeamLogo.sprite = homeTeam.GetTeamLogo();
        _homeTeamName.text = homeTeam.GetTeamName();
        _homeTeamPosition.text = $"#{homeTeam.GetSeed() + 1}";
        _homeTeamRecord.text = homeTeam.GetCurrentSeasonStats().GetWins() + " - " + homeTeam.GetCurrentSeasonStats().GetLosses();

        Player homeBestPlayer = homeTeam.GetPlayersFromTeam().OrderByDescending(x => x.CalculateRatingForPosition()).First();
        _homeTeamBestPlayer.text = $"{homeBestPlayer.GetFullName()} ({homeBestPlayer.CalculateRatingForPosition()})";

        // Away team
        _awayTeamLogo.sprite = awayTeam.GetTeamLogo();
        _awayTeamName.text = awayTeam.GetTeamName();
        _awayTeamPosition.text = $"#{awayTeam.GetSeed() + 1}";
        _awayTeamRecord.text = awayTeam.GetCurrentSeasonStats().GetWins() + " - " + awayTeam.GetCurrentSeasonStats().GetLosses();

        Player awayBestPlayer = awayTeam.GetPlayersFromTeam().OrderByDescending(x => x.CalculateRatingForPosition()).First();
        _awayTeamBestPlayer.text = $"{awayBestPlayer.GetFullName()} ({awayBestPlayer.CalculateRatingForPosition()})";
    }

    public void SetTeamDetails(PlayoffMatchup match)
    {
        _gameTypeText.text = "Playoff Game";

        Team homeTeam = LeagueSystem.Instance.GetTeam(match.GetHomeTeamID());
        Team awayTeam = LeagueSystem.Instance.GetTeam(match.GetAwayTeamID());

        // Home team
        _homeTeamLogo.sprite = homeTeam.GetTeamLogo();
        _homeTeamName.text = homeTeam.GetTeamName();
        _homeTeamPosition.text = $"#{homeTeam.GetSeed()}";
        _homeTeamRecord.text = homeTeam.GetCurrentSeasonStats().GetWins() + " - " + homeTeam.GetCurrentSeasonStats().GetLosses();

        Player homeBestPlayer = homeTeam.GetPlayersFromTeam().OrderByDescending(x => x.CalculateRatingForPosition()).First();
        _homeTeamBestPlayer.text = $"{homeBestPlayer.GetFullName()} ({homeBestPlayer.CalculateRatingForPosition()})";

        // Away team
        _awayTeamLogo.sprite = awayTeam.GetTeamLogo();
        _awayTeamName.text = awayTeam.GetTeamName();
        _awayTeamPosition.text = $"#{awayTeam.GetSeed()}";
        _awayTeamRecord.text = awayTeam.GetCurrentSeasonStats().GetWins() + " - " + awayTeam.GetCurrentSeasonStats().GetLosses();

        Player awayBestPlayer = awayTeam.GetPlayersFromTeam().OrderByDescending(x => x.CalculateRatingForPosition()).First();
        _awayTeamBestPlayer.text = $"{awayBestPlayer.GetFullName()} ({awayBestPlayer.CalculateRatingForPosition()})";
    }

    private bool CheckLineupCompletion(List<Player> players)
    {
        foreach (Player player in players)
        {
            if (player == null) return false;
        }

        return true;
    }

    private List<Player> GetBench(Team team, List<Player> players)
    {
        List<Player> newPlayerList = new List<Player>();
        foreach (Player player in team.GetPlayersFromTeam())
        {
            if (!players.Contains(player))
            {
                newPlayerList.Add(player);
            }
        }

        return newPlayerList;
    }

    public void GoToPrematchView()
    {
        TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(true, CanvasKey.Prematch, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID())));
    }
}
