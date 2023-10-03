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

        List<Player> topScoring = players.OrderByDescending(x => x.GetLatestSeason().GetPercentageOfStat(new List<string>() { "twoPointersPoints", "threePointersPoints" })).Take(3).ToList();
        List<Player> topAssists = players.OrderByDescending(x => x.GetLatestSeason().GetPercentageOfStat("assists")).Take(3).ToList();
        List<Player> topPlayers = players.OrderByDescending(x => x.CalculateRatingForPosition()).Take(5).ToList();

        SetTopScorers(_topScoringObjectsRoot.GetComponentsInChildren<StatObject>().ToList(), topScoring);
        SetTopAssisters(_topAssistsObjectsRoot.GetComponentsInChildren<StatObject>().ToList(), topAssists);
        SetTopPlayers(_topPlayersObjectsRoot.GetComponentsInChildren<PlayerItem>().ToList(), topPlayers);

        _teamManagementButton.onClick.RemoveAllListeners();
        // TODO: Change to player chosen team
        _teamManagementButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Team, LeagueSystem.Instance.GetTeam(0)));
    }

    private void SetTopScorers(List<StatObject> topObjects, List<Player> topPlayers)
    {
        for (int i = 0; i < topObjects.Count; i++)
        {
            topObjects[i].SetDetails(new StatObjectWrapper(topPlayers[i].GetFullName(), new List<float> { topPlayers[i].GetLatestSeason().GetPercentageOfStat(new List<string>() { "twoPointersPoints", "threePointersPoints" }) }));
        }
    }

    private void SetTopAssisters(List<StatObject> topObjects, List<Player> topPlayers)
    {
        for (int i = 0; i < topObjects.Count; i++)
        {
            topObjects[i].SetDetails(new StatObjectWrapper(topPlayers[i].GetFullName(), new List<float> { topPlayers[i].GetLatestSeason().GetPercentageOfStat("assists" )}));
        }
    }

    private void SetTopPlayers(List<PlayerItem> topObjects, List<Player> topPlayers)
    {
        for (int i = 0; i < topObjects.Count; i++)
        {
            topObjects[i].SetPlayerDetails(topPlayers[i], false, false);
        }
    }
}
