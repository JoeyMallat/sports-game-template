using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DraftSystem : MonoBehaviour
{
    [SerializeField] DraftClass _upcomingDraftClass;
    [SerializeField] Vector2 _timerForDraft;
    Vector2 _timeLeft;
    Vector2 _simPickOnSecond;

    [SerializeField] List<DraftOrderItemWrapper> _teamPicks;
    [SerializeField] int _currentPick;

    public static event Action<Player, Team, int> OnPlayerPicked;
    public static event Action<List<DraftOrderItemWrapper>> OnGetDraftOrder;
    public static event Action<float, float> OnDraftClockUpdated;
    public static event Action<DraftClass> OnDraftClassUpdated;

    private void Start()
    {
        Player.OnPlayerScouted += UpdateDraftClass;
        DraftPlayerUI.OnPlayerDrafted += PickPlayer;
        DraftPlayerUI.OnPlayerDrafted += UpdateDraftClass;

        _currentPick = 0;
        GenerateDraftClass();
        _teamPicks = GetDraftOrder(LeagueSystem.Instance.GetTeams());
        _timeLeft = _timerForDraft;

        //SimulateDraft(10);
    }

    private void Update()
    {
        //RunClock();
    }

    private void UpdateDraftClass(Player player)
    {
        OnDraftClassUpdated?.Invoke(_upcomingDraftClass);
    }

    private void GenerateDraftClass()
    {
        _upcomingDraftClass = new DraftClass(UnityEngine.Random.Range(40, 70));
        OnDraftClassUpdated?.Invoke(_upcomingDraftClass);
    }

    private void RunClock()
    {
        if (_timeLeft.x >= 0 && _timeLeft.y > 0)
        {
            _timeLeft.y -= Time.deltaTime;

            if (_timeLeft.x == _simPickOnSecond.x && _timeLeft.y == _simPickOnSecond.y)
            {
                SimulatePick();
                ResetClock();
            }

            if (_timeLeft.y <= 0 && _timeLeft.x > 0)
            {
                _timeLeft.x--;
                _timeLeft.y = 60;
            }
        } else
        {
            SimulatePick();
            ResetClock();
        }

        OnDraftClockUpdated?.Invoke(_timeLeft.x, _timeLeft.y);
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
        DraftOrderItemWrapper currentPickItem = _teamPicks[0];
        _teamPicks.RemoveAt(0);

        DecidePick(LeagueSystem.Instance.GetTeam(currentPickItem.GetTeamID()), _currentPick);
        _currentPick++;
    }

    public void SimEntireDraft()
    {
        SimulateDraft(ConfigManager.Instance.GetCurrentConfig().GetTotalDraftPicks());
    }

    private void SimulateDraft(int picks)
    {
        for (int i = 0; i < picks; i++)
        {
            SimulatePick();
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

        Debug.Log($"{pickedPlayer.GetFullName()} has been picked at pick {currentPick} for {team.GetTeamName()}");

        OnDraftClassUpdated?.Invoke(_upcomingDraftClass);
        ResetClock();
        return pickedPlayer;
    }

    public void PickPlayer(Player player)
    {
        _teamPicks.RemoveAt(0);

        _upcomingDraftClass.PickPlayer(player, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()), _currentPick);
        OnPlayerPicked?.Invoke(player, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()), _currentPick);
        OnDraftClassUpdated?.Invoke(_upcomingDraftClass);
        _currentPick++;
        ResetClock();
    }

    private void ResetClock()
    {
        _timeLeft = _timerForDraft;
        _simPickOnSecond = new Vector2(Mathf.RoundToInt(UnityEngine.Random.Range(0, _timerForDraft.x - 1)), Mathf.RoundToInt(UnityEngine.Random.Range(1, _timerForDraft.y - 1)));
        Debug.Log(_simPickOnSecond);
        OnDraftClockUpdated?.Invoke(_timeLeft.x, _timeLeft.y);
    }
}
