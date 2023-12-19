using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using Firebase.Analytics;

public class GameManager : MonoBehaviour
{
    [SerializeField] SeasonStage _currentSeasonStage;
    [SerializeField] int _currentSeason;
    [SerializeField] int _currentWeek;
    [SerializeField] bool _draftCompleted;
    [InfoBox("@GetTeamName()")]
    [SerializeField][Range(0, 29)] int _selectedTeamID;
    [SerializeField] bool _teamPicked;

    public static event Action<SeasonStage, int> OnAdvance;
    public static event Action<SeasonStage, int> OnGameStarted;
    public static event Action<SeasonStage, int> OnPostSeasonStarted;
    public static GameManager Instance;

    [SerializeField] float _salaryCapIncrease;
    [SerializeField] bool _premiumStatus;
    [SerializeField] List<OwnedGameItem> _ownedGameItems;
    [SerializeField] int _gems;
    [SerializeField] bool _currentForceWinState;
    public static event Action<CloudSaveData> OnSalaryCapIncreased;
    public static event Action<CloudSaveData> OnPremiumStatusUpdated;
    public static event Action<CloudSaveData> OnGemsUpdated;
    public static event Action<CloudSaveData> OnInventoryUpdated;
    public static event Action OnNewSeasonStarted;

    public bool GetCurrentForceWinState()
    {
        return _currentForceWinState;
    }

    public void SetCurrentForceWinState(bool newForceWinState)
    {
        _currentForceWinState = newForceWinState;
    }

    public void SetGems(int gems)
    {
        _gems = gems;
        OnGemsUpdated?.Invoke(new CloudSaveData(_salaryCapIncrease, _premiumStatus, _gems, _ownedGameItems));
    }

    public void AddToGems(int gemsToEdit)
    {
        _gems += gemsToEdit;
        OnGemsUpdated.Invoke(new CloudSaveData(_salaryCapIncrease,_premiumStatus, _gems, _ownedGameItems));
    }

    public bool CheckBuyItem(int cost)
    {
        if (cost <= _gems)
        {
            AddToGems(-cost);
            return true;
        } else
        {
            return false;
        }
    }

    public void SetPremiumStatus(bool status)
    {
        _premiumStatus = status;
        OnPremiumStatusUpdated?.Invoke(new CloudSaveData(_salaryCapIncrease,_premiumStatus, _gems, _ownedGameItems));
    }

    public bool GetPremiumStatus()
    {
        return _premiumStatus;
    }

    public void SetSalaryCapIncrease(float salaryCapIncrease)
    {
        _salaryCapIncrease += salaryCapIncrease;
        OnSalaryCapIncreased?.Invoke(new CloudSaveData(_salaryCapIncrease, _premiumStatus, _gems, _ownedGameItems));
    }

    public float GetSalaryCapIncrease()
    {
        return _salaryCapIncrease;
    }

    public void RemoveItem(OwnedGameItem item)
    {
        if (item.GetAmountInInventory() > 1)
        {
            _ownedGameItems.Where(x => x.GetItemID() == item.GetItemID()).ToList()[0].UpdateAmount(-1);
        } else
        {
            _ownedGameItems.Remove(item);
        }

        OnInventoryUpdated?.Invoke(new CloudSaveData(_salaryCapIncrease,_premiumStatus, _gems, _ownedGameItems));
    }

    public void SetInventory(List<CloudInventoryItem> ownedGameItems)
    {
        if (ownedGameItems == null) return;

        _ownedGameItems = new List<OwnedGameItem>();
        foreach (CloudInventoryItem item in ownedGameItems)
        {
            _ownedGameItems.Add(item.ConvertToOwnedGameItem(item));
        }
    }

    public void SetLoadData(SeasonStage seasonStage, int season, int week, int teamID)
    {
        _currentSeasonStage = seasonStage;
        _currentSeason = season;
        _currentWeek = week;
        _selectedTeamID = teamID;
        _teamPicked = true;
    }

    public int GetCurrentWeek()
    {
        return _currentWeek;
    }

    public int GetCurrentSeason()
    {
        return _currentSeason;
    }

    private void Start()
    {
        _ownedGameItems = new List<OwnedGameItem>();

        LeagueSystem.OnRegularSeasonFinished += ChangeSeasonStage;
        PlayoffSystem.OnPlayoffsFinished += ChangeSeasonStage;
        OnGameStarted += SetTeam;
    }

    public List<OwnedGameItem> GetItems()
    {
        return _ownedGameItems;
    }

    private void SetTeam(SeasonStage seasonStage, int week)
    {
        _teamPicked = true;
    }

    public void StartGame()
    {
        TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(_selectedTeamID)));
        FirebaseAnalytics.LogEvent("team_picked", new Parameter("team", LeagueSystem.Instance.GetTeam(_selectedTeamID).GetTeamName()));
        OnGameStarted?.Invoke(SeasonStage.RegularSeason, 0);
    }

    public bool GetTeamPickedStatus()
    {
        return _teamPicked;
    }

    public void ChangeSeasonStage(List<Team> teams, SeasonStage newSeasonStage)
    {
        _currentWeek = 0;
        _currentSeasonStage = newSeasonStage;
        Debug.Log($"Season stage changed to {newSeasonStage}");

        switch (_currentSeasonStage)
        {
            case SeasonStage.RegularSeason:
                OnNewSeasonStarted?.Invoke();
                _currentSeason++;
                break;
            case SeasonStage.Playoffs:
                Navigation.Instance.GoToScreen(true, CanvasKey.Playoffs);
                break;
            case SeasonStage.OffSeason:
                OnPostSeasonStarted?.Invoke(_currentSeasonStage, _currentWeek);
                break;
            default:
                break;
        }
    }

    public void StartNewSeason()
    {
        ChangeSeasonStage(LeagueSystem.Instance.GetTeams(), SeasonStage.RegularSeason);
        TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(_selectedTeamID)));
        FirebaseAnalytics.LogEvent("new_season_started", new Parameter("season", _currentSeason));
    }

    public void AddItem(GameItem reward)
    {
        int amount = _ownedGameItems.Where(x => x.GetItemID() == reward.GetItemID()).Count();

        if (amount == 0)
        {
            _ownedGameItems.Add(new OwnedGameItem(reward.GetItemID(), reward.GetGamesRemaining(), amount + 1));
        } else
        {
            _ownedGameItems.Where(x => x.GetItemID() == reward.GetItemID()).ToList()[0].UpdateAmount(1);
        }

        OnInventoryUpdated?.Invoke(new CloudSaveData(_salaryCapIncrease, _premiumStatus, _gems, _ownedGameItems));
    }

    public void Advance()
    {
        _currentWeek++;

        OnAdvance?.Invoke(_currentSeasonStage, _currentWeek);
    }

    public SeasonStage GetSeasonStage()
    {
        return _currentSeasonStage;
    }

    private string GetTeamName()
    {
        return LeagueSystem.Instance.GetTeam(_selectedTeamID).GetTeamName();
    }

    public int GetGems()
    {
        return _gems;
    }

    public int GetTeamID()
    {
        return _selectedTeamID;
    }

    private void Awake()
    {
        Application.targetFrameRate = 120;
        TeamSelection.OnSelectedTeam += ((x) => _selectedTeamID = x.GetTeamID());

        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(this);
        }
    }
}
