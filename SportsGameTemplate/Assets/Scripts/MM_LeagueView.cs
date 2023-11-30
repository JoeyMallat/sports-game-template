using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class MM_LeagueView : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _leaguePreviewRoot;
    [SerializeField] GameObject _topScorersRoot;
    [SerializeField] GameObject _topAssistersRoot;
    [SerializeField] Button _standingsButton;
    [SerializeField] Button _playoffsButton;
    [SerializeField] Button _statsCenterButton;

    public void SetDetails<T>(T item) where T : class
    {
        List<Team> league = item as List<Team>;

        int position = league.IndexOf(LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()));

        List<TeamItem> teamItems = _leaguePreviewRoot.GetComponentsInChildren<TeamItem>().ToList();
        SetTopScorers(_topScorersRoot.GetComponentsInChildren<StatObject>().ToList(), LeagueSystem.Instance.GetTopListOfStat(new List<string>() { "freeThrowsMade", "twoPointersPoints", "threePointersPoints" }, 3));
        SetTopAssisters(_topAssistersRoot.GetComponentsInChildren<StatObject>().ToList(), LeagueSystem.Instance.GetTopListOfStat("assists", 3));

        SetLeaguePreview(teamItems, league, position);

        _standingsButton.onClick.RemoveAllListeners();
        _standingsButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Standings, league));

        _playoffsButton.ToggleButtonStatus(GameManager.Instance.GetSeasonStage() != SeasonStage.RegularSeason);
        _playoffsButton.onClick.RemoveAllListeners();
        _playoffsButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Playoffs));
    }

    private void SetTopScorers(List<StatObject> topObjects, List<Player> topPlayers)
    {
        for (int i = 0; i < topObjects.Count; i++)
        {
            topObjects[i].SetDetails(new StatObjectWrapper($"{topPlayers[i].GetFullName()} - <color=#FF9900>{LeagueSystem.Instance.GetTeam(topPlayers[i].GetTeamID()).GetTeamName()}", new List<float> { topPlayers[i].GetLatestSeason().GetAverageOfStat(new List<string>() { "freeThrowsMade", "twoPointersPoints", "threePointersPoints" }) }));
        }
    }

    private void SetTopAssisters(List<StatObject> topObjects, List<Player> topPlayers)
    {
        for (int i = 0; i < topObjects.Count; i++)
        {
            topObjects[i].SetDetails(new StatObjectWrapper($"{topPlayers[i].GetFullName()} - <color=#FF9900>{LeagueSystem.Instance.GetTeam(topPlayers[i].GetTeamID()).GetTeamName()}", new List<float> { topPlayers[i].GetLatestSeason().GetAverageOfStat("assists")}));
        }
    }

    private void SetLeaguePreview(List<TeamItem> teamItems, List<Team> teams, int position)
    {
        List<Team> teamsToShow = new List<Team>();
        int mostWins = teams[0].GetCurrentSeasonStats().GetWins();

        if (position < 3)
        {
            teamsToShow.AddRange(teams.GetRange(0, 5));
        }
        else if (position > 27)
        {
            teamsToShow.AddRange(teams.GetRange(teams.Count - 5, 5));
        }
        else
        {
            teamsToShow.AddRange(teams.GetRange(position - 2, 5));
        }

        for (int i = 0; i < teamItems.Count; i++)
        {
            int teamPos = teams.IndexOf(teamsToShow[i]) + 1;
            teamItems[i].SetTeamDetails(teamPos, teamsToShow[i], mostWins);
        }
    }
}
