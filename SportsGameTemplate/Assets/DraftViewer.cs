using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DraftViewer : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _picksRoot;

    public void SetDetails<T>(T item) where T : class
    {
        List<Team> teams = item as List<Team>;

        List<DraftOrderItem> draftOrderItems = _picksRoot.GetComponentsInChildren<DraftOrderItem>(true).ToList();

        List<DraftOrderItemWrapper> draftOrder = GetDraftOrder(teams).OrderBy(x => x.GetDraftRound()).ThenBy(x => x.GetPickNumber()).ToList();

        for (int i = 0; i < draftOrder.Count; i++)
        {
            int index = i;
            if (i < draftOrderItems.Count)
            {
                draftOrderItems[i].SetDraftOrderItem(draftOrder[index]);
            }
        }
    }

    private List<DraftOrderItemWrapper> GetDraftOrder(List<Team> teams)
    {
        List<DraftOrderItemWrapper> draftPicks = new List<DraftOrderItemWrapper>();

        foreach (Team team in teams)
        {
            foreach (DraftPick pick in team.GetDraftPicks())
            {
                draftPicks.Add(new DraftOrderItemWrapper(pick.GetPickData().Item1, pick.GetPickData().Item2, team.GetTeamID(), team.GetTeamName()));
            }
        }

        return draftPicks;
    }
}

public struct DraftOrderItemWrapper {
    [SerializeField] int _round;
    [SerializeField] int _pickNumber;
    [SerializeField] int _teamID;
    [SerializeField] string _teamName;

    public DraftOrderItemWrapper(int round, int pick, int teamID, string teamName)
    {
        _round = round;
        _pickNumber = pick;
        _teamID = teamID;
        _teamName = teamName;
    }

    public int GetDraftRound()
    {
        return _round;
    }

    public int GetPickNumber()
    {
        return _pickNumber;
    }

    public int GetTeamID()
    {
        return _teamID;
    }

    public string GetTeamName()
    {
        return _teamName;
    }
}
