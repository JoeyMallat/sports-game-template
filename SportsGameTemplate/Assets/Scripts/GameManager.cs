using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] SeasonStage _currentSeasonStage;
    [SerializeField] int _currentWeek;
    [InfoBox("@GetTeamName()")]
    [SerializeField][Range(0, 29)] int _selectedTeamID;
    [SerializeField] bool _teamPicked;

    public static event Action<SeasonStage, int> OnAdvance;
    public static event Action<SeasonStage, int> OnGameStarted;

    public static GameManager Instance;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Advance();
        }
    }

    public int GetCurrentWeek()
    {
        return _currentWeek;
    }

    private void Start()
    {
        LeagueSystem.OnRegularSeasonFinished += ChangeSeasonStage;
        PlayoffSystem.OnPlayoffsFinished += ChangeSeasonStage;
        OnGameStarted += SetTeam;
    }

    private void SetTeam(SeasonStage seasonStage, int week)
    {
        _teamPicked = true;
    }

    public void StartSeason()
    {
        TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(_selectedTeamID)));
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
                break;
            case SeasonStage.Playoffs:
                Navigation.Instance.GoToScreen(true, CanvasKey.Playoffs);
                break;
            case SeasonStage.OffSeason:
                break;
            default:
                break;
        }
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

    public int GetTeamID()
    {
        return _selectedTeamID;
    }

    private void Awake()
    {
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
