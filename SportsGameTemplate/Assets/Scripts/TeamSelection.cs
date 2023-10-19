using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSelection : MonoBehaviour
{
    int selectedTeam;
    public static event Action<Team> OnSelectedTeam;

    public void SelectRandomTeam()
    {
        List<Team> teams = LeagueSystem.Instance.GetTeamsWithoutTeam(selectedTeam);

        int randomTeamID = UnityEngine.Random.Range(0, teams.Count);

        OnSelectedTeam?.Invoke(teams[randomTeamID]);
        selectedTeam = randomTeamID;
    }

    public void SelectTeam(int index)
    {
        OnSelectedTeam?.Invoke(LeagueSystem.Instance.GetTeam(index));
        selectedTeam = index;
    }
}
