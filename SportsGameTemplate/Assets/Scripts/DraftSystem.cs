using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DraftSystem : MonoBehaviour
{
    [SerializeField] DraftClass _upcomingDraftClass;

    [SerializeField] List<int> _teamPicks;

    private void Start()
    {
        _upcomingDraftClass = new DraftClass(UnityEngine.Random.Range(50, 80));
        _teamPicks = GetDraftOrder();

        //SimulateDraft();
    }

    private List<int> GetDraftOrder()
    {
        int[] teamIDs = new int[ConfigManager.Instance.GetCurrentConfig().GetTotalDraftPicks()];

        foreach (Team team in LeagueSystem.Instance.GetTeams())
        {
            foreach (DraftPick pick in team.GetDraftPicks())
            {
                teamIDs[pick.GetTotalPickNumber() - 1] = team.GetTeamID();
            }
        }

        return teamIDs.ToList();
    }

    private void SimulateDraft(int untilPick = 0)
    {
        int currentPickNumber = 0;
        foreach (var teamID in _teamPicks)
        {
            Team team = LeagueSystem.Instance.GetTeam(teamID);
            Player player = DecidePick(team, currentPickNumber);
            currentPickNumber++;
        }
    }

    private Player DecidePick(Team team, int currentPick)
    {
        int selection = 0;
        if (currentPick < ConfigManager.Instance.GetCurrentConfig().GetTotalDraftPicks() - 2)
        {
            selection = UnityEngine.Random.Range(0, 2);
        }

        return _upcomingDraftClass.PickPlayerAtID(selection, team, currentPick);
    }
}
