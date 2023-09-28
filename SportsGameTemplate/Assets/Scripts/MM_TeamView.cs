using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MM_TeamView : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _topScoringObjectsRoot;
    [SerializeField] GameObject _topAssistsObjectsRoot;

    public void SetDetails<T>(T item) where T : class
    {
        List<Player> players = item as List<Player>;

        List<Player> topScoring = players.OrderByDescending(x => x.GetLatestSeason().GetPointsPerGame()).Take(3).ToList();
        List<Player> topAssists = players.OrderByDescending(x => x.GetLatestSeason().GetAssistsPerGame()).Take(3).ToList();

        SetTopScorers(_topScoringObjectsRoot.GetComponentsInChildren<StatObject>().ToList(), topScoring);
        SetTopAssisters(_topAssistsObjectsRoot.GetComponentsInChildren<StatObject>().ToList(), topAssists);
    }

    private void SetTopScorers(List<StatObject> topObjects, List<Player> topPlayers)
    {
        for (int i = 0; i < topObjects.Count; i++)
        {
            topObjects[i].SetDetails(new StatObjectWrapper(topPlayers[i].GetFullName(), new List<float> { topPlayers[i].GetLatestSeason().GetPointsPerGame() }));
        }
    }

    private void SetTopAssisters(List<StatObject> topObjects, List<Player> topPlayers)
    {
        for (int i = 0; i < topObjects.Count; i++)
        {
            topObjects[i].SetDetails(new StatObjectWrapper(topPlayers[i].GetFullName(), new List<float> { topPlayers[i].GetLatestSeason().GetAssistsPerGame() }));
        }
    }
}
