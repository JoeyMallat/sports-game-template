using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DraftSystem : MonoBehaviour
{
    public static DraftSystem Instance { get; private set; }

    [SerializeField] DraftClass _upcomingDraftClass;
    [SerializeField] List<DraftClass> _futureDraftClasses = new List<DraftClass>();

    [SerializeField] List<DraftOrderItemWrapper> _teamPicks;
    [SerializeField] int _currentPick;

    public static event Action<Player, Team, int> OnPlayerPicked;
    public static event Action<List<DraftOrderItemWrapper>> OnGetDraftOrder;
    public static event Action<DraftClass> OnDraftClassUpdated;
    public static event Action OnDraftEnded;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Player.OnPlayerScouted += UpdateDraftClass;
        DraftPlayerUI.OnPlayerDrafted += PickPlayer;
        DraftPlayerUI.OnPlayerDrafted += UpdateDraftClass;
        GameManager.OnPostSeasonStarted += UpdateDraftOrder;
        OnDraftEnded += RemoveDraftClass;
        OnDraftEnded += RemoveDraftPicksFromTeams;
        GameManager.OnNewSeasonStarted += GenerateDraftClass;
        GameManager.OnNewSeasonStarted += ResetDraft;

        _currentPick = 0;
        GenerateDraftClass();
    }

    public void InitializeDraft()
    {
        UpdateDraftOrder(GameManager.Instance.GetSeasonStage(), GameManager.Instance.GetCurrentWeek());
        _currentPick = 0;
        GenerateDraftClass();
    }

    private void RemoveDraftPicksFromTeams()
    {
        foreach (Team team in LeagueSystem.Instance.GetTeams())
        {
            team.RemoveDraftPicksAndTradeOffers();
        }
    }

    public bool UserNowPicking()
    {
        foreach (var draftPick in LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetDraftPicks())
        {
            if (_currentPick + 1 == draftPick.GetTotalPickNumber())
                return true;
        }
        return false;
    }

    private void ResetDraft()
    {
        _currentPick = 0;
    }

    private void RemoveDraftClass()
    {
        _upcomingDraftClass = null;
    }

    private void UpdateDraftOrder(SeasonStage seasonStage, int week)
    {
        _teamPicks = GetDraftOrder(LeagueSystem.Instance.GetTeams());
    }

    private void UpdateDraftClass(Player player)
    {
        OnDraftClassUpdated?.Invoke(_upcomingDraftClass);
    }

    private void GenerateDraftClass()
    {
        int currentSeason = GameManager.Instance.GetCurrentSeason();
        _futureDraftClasses.RemoveAll(draftClass => draftClass == null || draftClass.GetSeason() < currentSeason);

        TradeValueSettings settings = ConfigManager.Instance.GetCurrentConfig().TradeValueSettings ?? TradeValueSettings.Default;
        for (int season = currentSeason; season < currentSeason + settings.FutureDraftClassHorizon; season++)
        {
            if (_futureDraftClasses.Any(draftClass => draftClass.GetSeason() == season))
                continue;

            _futureDraftClasses.Add(new DraftClass(UnityEngine.Random.Range(40, 85), season));
        }

        _futureDraftClasses = _futureDraftClasses.OrderBy(draftClass => draftClass.GetSeason()).ToList();
        _upcomingDraftClass = _futureDraftClasses.First(draftClass => draftClass.GetSeason() == currentSeason);
        OnDraftClassUpdated?.Invoke(_upcomingDraftClass);
    }

    public DraftClass GetDraftClass(int season)
    {
        return _futureDraftClasses.FirstOrDefault(draftClass => draftClass.GetSeason() == season);
    }

    public Dictionary<int, float> GetDraftClassQuality()
    {
        return _futureDraftClasses.ToDictionary(draftClass => draftClass.GetSeason(), draftClass => draftClass.GetQuality());
    }

    public List<DraftOrderItemWrapper> GetDraftOrder(List<Team> teams)
    {
        List<DraftOrderItemWrapper> draftPicks = new List<DraftOrderItemWrapper>();

        foreach (Team team in teams)
        {
            foreach (DraftPick pick in team.GetDraftPicks())
            {
                draftPicks.Add(new DraftOrderItemWrapper(pick.GetPickData().Item1, pick.GetPickData().Item2, team.GetTeamID(), team.GetTeamName()));
            }
        }

        draftPicks = draftPicks.OrderBy(x => x.GetDraftRound()).ThenBy(x => x.GetPickNumber()).ToList();

        OnGetDraftOrder?.Invoke(draftPicks);
        return draftPicks;
    }

    public void SimulatePick()
    {
        if (_teamPicks.Count == 0)
        {
            OnDraftEnded?.Invoke();
            return;
        }

        DraftOrderItemWrapper currentPickItem = _teamPicks[0];
        _teamPicks.RemoveAt(0);

        DecidePick(LeagueSystem.Instance.GetTeam(currentPickItem.GetTeamID()), _currentPick);
        _currentPick++;
    }

    public void SimEntireDraft()
    {
        SimulateDraft(ConfigManager.Instance.GetCurrentConfig().GetTotalDraftPicks() - 1);
    }

    private void SimulateDraft(int picks)
    {
        for (int i = 0; i < picks; i++)
        {
            SimulatePick();
        }
    }

    public void SimUntilUserPick()
    {
        Team team = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID());

        // If user has no picks, we can sim the entire draft
        if (team.GetDraftPicks().Count == 0) SimEntireDraft();

        // If user picks have been picked, we can sim the entire draft
        if (team.GetDraftPicks().Last().GetTotalPickNumber() < _currentPick) SimEntireDraft();

        for (int i = 0; i < team.GetDraftPicks().Count; i++)
        {
            DraftPick draftPick = team.GetDraftPicks()[i];
            if (draftPick.GetTotalPickNumber() > _currentPick)
            {
                SimulateDraft(draftPick.GetTotalPickNumber() - _currentPick - 1);
                return;
            }
        }
    }

    private Player DecidePick(Team team, int currentPick)
    {
        int selection = 0;
        if (currentPick < ConfigManager.Instance.GetCurrentConfig().GetTotalDraftPicks() - 2)
        {
            selection = UnityEngine.Random.Range(0, 2);
        }

        Player pickedPlayer = _upcomingDraftClass.PickPlayerAtID(selection, team, currentPick);
        OnPlayerPicked?.Invoke(pickedPlayer, team, currentPick);

        //Debug.Log($"{pickedPlayer.GetFullName()} has been picked at pick {currentPick} for {team.GetTeamName()}");

        OnDraftClassUpdated?.Invoke(_upcomingDraftClass);
        return pickedPlayer;
    }

    public void PickPlayer(Player player)
    {
        _teamPicks.RemoveAt(0);

        _upcomingDraftClass.PickPlayer(player, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()), _currentPick);
        OnPlayerPicked?.Invoke(player, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()), _currentPick);
        OnDraftClassUpdated?.Invoke(_upcomingDraftClass);
        _currentPick++;
    }
}
