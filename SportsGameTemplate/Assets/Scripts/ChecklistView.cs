using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChecklistView : MonoBehaviour
{
    [SerializeField] List<ChecklistItem> _checklistItems;
    [SerializeField] bool[] _checklistChecks;
    [SerializeField] Button _nextSeasonButton;

    public event Action<int> OnChecklistItemChecked;

    private void Awake()
    {
        _checklistChecks = new bool[4] { false, false, false, false };

        GameManager.OnPostSeasonStarted += UpdateChecklist;
        Player.OnPlayerTeamChanged += UpdateChecklist;
        Player.OnPlayerContractSigned += UpdateChecklist;
        DraftSystem.OnDraftEnded += CheckChecklist;
        DraftSystem.OnDraftEnded += OnDraftEnded;
        RewardedAdsManager.OnRewardedAdWatched += SetRewardedAdCheckmark;
        GameManager.OnPremiumStatusUpdated += SetPremiumStatus;
        LocalSaveManager.OnGameLoaded += UpdateChecklist;
        OnChecklistItemChecked += SetNextSeasonButton;
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
        ToggleChecklistItem(0, true);
    }

    public List<bool> GetChecklist()
    {
        return _checklistChecks.ToList();
    }

    public void SetChecklist(List<bool> bools)
    {
        for (int i = 0; i < bools.Count; i++)
        {
            _checklistChecks[i] = bools[i];
        }
    }

    public void CheckChecklist()
    {
        Team team = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID());

        // Check draft
        Debug.Log("Setting the checklist items");
        _checklistItems[0].SetChecklistItem(_checklistChecks[0], "Complete the draft", "", new List<string>() { "Start draft" }, new List<System.Action>() { () => { Navigation.Instance.GoToScreen(false, CanvasKey.Draft); CheckChecklist(); } }); 

        // Check amount of players
        int playerCount = team.GetPlayersFromTeam().Count;
        _checklistItems[1].SetChecklistItem(playerCount >= 15, "Minimum of 15 players in team", $"Currently {playerCount}", new List<string>(), new List<System.Action>() { () => CheckChecklist() });
        ToggleChecklistItem(1, playerCount >= 15);

        // Check payroll
        int currentPayroll = team.GetTotalSalaryAmount();
        _checklistItems[2].SetChecklistItem(currentPayroll <= ConfigManager.Instance.GetCurrentConfig().SalaryCap * (1 + GameManager.Instance.GetSalaryCapIncrease()), "Payroll under salary cap", $"Currently {currentPayroll.ConvertToMonetaryString()} / {(ConfigManager.Instance.GetCurrentConfig().SalaryCap * (1 + GameManager.Instance.GetSalaryCapIncrease())).ConvertToMonetaryString()}", new List<string>(), new List<System.Action>() { () => CheckChecklist() });
        ToggleChecklistItem(2, currentPayroll <= ConfigManager.Instance.GetCurrentConfig().SalaryCap * (1 + GameManager.Instance.GetSalaryCapIncrease()));

        ToggleChecklistItem(3, GameManager.Instance.GetPremiumStatus());
        _checklistItems[3].SetChecklistItem(_checklistChecks[3], "Watch an ad or remove ads", "", new List<string>() { "Watch ad", "Remove ads" }, new List<System.Action>() { () => CheckChecklist() });
    }

    private void SetNextSeasonButton(int amountChecked)
    {
        if (amountChecked < _checklistChecks.Length) return;

        _nextSeasonButton.ToggleButtonStatus(true);
        _nextSeasonButton.onClick.RemoveAllListeners();
        _nextSeasonButton.onClick.AddListener(() => UncheckAllChecklistItems());
        _nextSeasonButton.onClick.AddListener(() => GameManager.Instance.StartNewSeason());
        _nextSeasonButton.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()), true)));

    }

    private void SetPremiumStatus(CloudSaveData cloudSaveData)
    {
        ToggleChecklistItem(3, cloudSaveData.PremiumStatus);
    }

    private void SetRewardedAdCheckmark(string code)
    {
        if (code == "checklist")
        {
            ToggleChecklistItem(3, true);
            _checklistItems[3].SetChecklistItem(_checklistChecks[3], "Watch an ad or remove ads", "", new List<string>() { "Watch ad", "Remove ads" }, new List<System.Action>());
        }
    }

    public void GoToStore()
    {
        TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(true, CanvasKey.Store));
    }

    private void ToggleChecklistItem(int index, bool status)
    {
        _checklistChecks[index] = status;
        OnChecklistItemChecked(_checklistChecks.Count(x => x == true));
    }

    private void UncheckAllChecklistItems()
    {
        ToggleChecklistItem(0, false);
        ToggleChecklistItem(1, false);
        ToggleChecklistItem(2, false);
        ToggleChecklistItem(3, false);
    }
}
