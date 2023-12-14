using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MM_TeamView : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _topScoringObjectsRoot;
    [SerializeField] GameObject _topAssistsObjectsRoot;
    [SerializeField] GameObject _topPlayersObjectsRoot;
    [SerializeField] Button _teamManagementButton;

    public void SetDetails<T>(T item) where T : class
    {
        List<Player> players = item as List<Player>;

        List<Player> topScoring = players.OrderByDescending(x => x.GetLatestSeason().GetAveragePoints()).Take(3).ToList();
        List<Player> topAssists = players.OrderByDescending(x => x.GetLatestSeason().GetAverageOfStat("assists")).Take(3).ToList();
        List<Player> topPlayers = players.OrderByDescending(x => x.CalculateRatingForPosition()).Take(5).ToList();

        SetTopScorers(_topScoringObjectsRoot.GetComponentsInChildren<StatObject>().ToList(), topScoring);
        SetTopAssisters(_topAssistsObjectsRoot.GetComponentsInChildren<StatObject>().ToList(), topAssists);
        SetTopPlayers(_topPlayersObjectsRoot.GetComponentsInChildren<PlayerItem>().ToList(), topPlayers);

        _teamManagementButton.onClick.RemoveAllListeners();
        _teamManagementButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Team, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID())));
    }

    private void SetTopScorers(List<StatObject> topObjects, List<Player> topPlayers)
    {
        for (int i = 0; i < topObjects.Count; i++)
        {
            int index = i;
            topObjects[i].SetDetails(new StatObjectWrapper(topPlayers[index].GetFullName(), new List<float> { topPlayers[index].GetLatestSeason().GetAveragePoints() }, topPlayers[index]));
        }
    }

    private void SetTopAssisters(List<StatObject> topObjects, List<Player> topPlayers)
    {
        for (int i = 0; i < topObjects.Count; i++)
        {
            int index = i;
            topObjects[i].SetDetails(new StatObjectWrapper(topPlayers[index].GetFullName(), new List<float> { topPlayers[index].GetLatestSeason().GetAverageOfStat("assists" )}, topPlayers[index]));
        }
    }

    private void SetTopPlayers(List<PlayerItem> topObjects, List<Player> topPlayers)
    {
        for (int i = 0; i < topObjects.Count; i++)
        {
            topObjects[i].SetPlayerDetails(topPlayers[i], true, false);
        }
    }
}
