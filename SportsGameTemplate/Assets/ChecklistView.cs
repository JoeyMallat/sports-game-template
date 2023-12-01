using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChecklistView : MonoBehaviour
{
    [SerializeField] List<ChecklistItem> _checklistItems;
    [SerializeField] bool[] _checklistChecks;
    [SerializeField] Button _nextSeasonButton;

    private void Awake()
    {
        GameManager.OnPostSeasonStarted += UpdateChecklist;
        Player.OnPlayerTeamChanged += UpdateChecklist;
        Player.OnPlayerContractSigned += UpdateChecklist;
        DraftSystem.OnDraftEnded += CheckChecklist;
        DraftSystem.OnDraftEnded += OnDraftEnded;

        _checklistChecks = new bool[4] { false, false, false, false };
    }

    public void UpdateChecklist(SeasonStage seasonStage = 0, int week = 0)
    {
        CheckChecklist();
    }

    public void UpdateChecklist(Player player = null)
    {
        CheckChecklist();
    }

    public void OnDraftEnded()
    {
        _checklistChecks[0] = true;
    }


    public void CheckChecklist()
    {
        Team team = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID());

        // Check draft
        _checklistItems[0].SetChecklistItem(_checklistChecks[0], "Complete the draft", "", new List<string>() { "Start draft" }, new List<System.Action>() { () => Navigation.Instance.GoToScreen(false, CanvasKey.Draft) });

        // Check amount of players
        int playerCount = team.GetPlayersFromTeam().Count;
        _checklistItems[1].SetChecklistItem(playerCount >= 15, "Minimum of 15 players in team", $"Currently {playerCount}", new List<string>(), new List<System.Action>());
        _checklistChecks[1] = playerCount >= 15;

        // Check payroll
        int currentPayroll = team.GetTotalSalaryAmount();
        _checklistItems[2].SetChecklistItem(currentPayroll <= ConfigManager.Instance.GetCurrentConfig().SalaryCap, "Payroll under salary cap", $"Currently {currentPayroll.ConvertToMonetaryString()} / {ConfigManager.Instance.GetCurrentConfig().SalaryCap.ConvertToMonetaryString()}", new List<string>(), new List<System.Action>());
        _checklistChecks[2] = currentPayroll <= ConfigManager.Instance.GetCurrentConfig().SalaryCap;

        // Check watch ad or remove ads
        bool tempTrue = true;
        _checklistItems[3].SetChecklistItem(tempTrue, "Watch an ad or remove ads", "", new List<string>() { "Watch ad", "Remove ads" }, new List<System.Action>());
        _checklistChecks[3] = tempTrue;

        foreach (bool check in _checklistChecks)
        {
            if (check == true)
            {
                continue;
            } else
            {
                _nextSeasonButton.ToggleButtonStatus(false);
                return;
            }
        }

        _nextSeasonButton.ToggleButtonStatus(true);
        _nextSeasonButton.onClick.RemoveAllListeners();
        _nextSeasonButton.onClick.AddListener(() => UncheckAllChecklistItems());
        _nextSeasonButton.onClick.AddListener(() => GameManager.Instance.StartNewSeason());
        _nextSeasonButton.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()))));
    }

    private void UncheckAllChecklistItems()
    {
        _checklistChecks[0] = false;
        _checklistChecks[1] = false;
        _checklistChecks[2] = false;
        _checklistChecks[3] = false;
    }
}
