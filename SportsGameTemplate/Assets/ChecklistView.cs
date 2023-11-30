using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChecklistView : MonoBehaviour
{
    [SerializeField] List<ChecklistItem> _checklistItems;

    private void Awake()
    {
        GameManager.OnPostSeasonStarted += UpdateChecklist;
    }

    public void UpdateChecklist(SeasonStage seasonStage = 0, int week = 0)
    {
        CheckChecklist();
    }


    public void CheckChecklist()
    {
        // Check draft
        _checklistItems[0].SetChecklistItem(GameManager.Instance.GetDraftStatus(), "Complete the draft", "", new List<string>() { "Start draft" }, new List<System.Action>() { () => Navigation.Instance.GoToScreen(false, CanvasKey.Draft) });

        // Check amount of players
        int playerCount = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetPlayersFromTeam().Count;
        _checklistItems[1].SetChecklistItem(playerCount >= 15, "Minimum of 15 players in team", $"Currently {playerCount}", new List<string>(), new List<System.Action>());

        // Check payroll

        // Check watch ad or remove ads
    }
}
