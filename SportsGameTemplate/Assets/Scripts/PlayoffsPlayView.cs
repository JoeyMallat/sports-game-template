using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayoffsPlayView : MonoBehaviour
{
    List<PlayoffGameItem> _playoffGameItems;

    [SerializeField] TextMeshProUGUI _leadText;

    [SerializeField] Image _homeTeamImage;
    [SerializeField] TextMeshProUGUI _homeTeamText;

    [SerializeField] Image _awayTeamImage;
    [SerializeField] TextMeshProUGUI _awayTeamText;

    public void SetDetails(Team team)
    {
        _playoffGameItems = GetComponentsInChildren<PlayoffGameItem>().ToList();

        PlayoffMatchup nextMatch = PlayoffSystem.Instance.GetNextMatchData();
        Team homeTeam = LeagueSystem.Instance.GetTeam(nextMatch.GetHomeTeamID());
        Team awayTeam = LeagueSystem.Instance.GetTeam(nextMatch.GetAwayTeamID());

        _homeTeamText.text = homeTeam.GetTeamName();
        _homeTeamImage.sprite = homeTeam.GetTeamLogo();

        _awayTeamText.text = awayTeam.GetTeamName();
        _awayTeamImage.sprite = awayTeam.GetTeamLogo();

        int homeWins = nextMatch.GetSeriesScore().Item1;
        int awayWins = nextMatch.GetSeriesScore().Item2;

        if (homeWins > awayWins)
        {
            string word = homeWins == (ConfigManager.Instance.GetCurrentConfig().BestOfAmountInPlayoffs + 1) / 2 ? "won" : "leads";
            _leadText.text = $"{homeTeam.GetTeamName()} {word} series {homeWins}-{awayWins}";
        }
        else if (homeWins == awayWins)
        {
            _leadText.text = $"Series is tied at {homeWins}-{awayWins}";
        }
        else
        {
            string word = awayWins == (ConfigManager.Instance.GetCurrentConfig().BestOfAmountInPlayoffs + 1) / 2 ? "won" : "leads";
            _leadText.text = $"{awayTeam.GetTeamName()} {word} series {awayWins}-{homeWins}";
        }

        for (int i = 0; i < _playoffGameItems.Count; i++)
        {
            int index = i;
            _playoffGameItems[i].SetItem(null);

            if (i < nextMatch.GetMatches().Count)
            {
                if (nextMatch.GetMatches()[index].GetMatchStatus())
                {
                    Team winner = nextMatch.GetMatches()[i].GetWinStatForTeam(homeTeam.GetTeamID()).Item1 == 1 ? homeTeam : awayTeam;
                    _playoffGameItems[i].SetItem(winner);
                }
            }
        }
    }
}
