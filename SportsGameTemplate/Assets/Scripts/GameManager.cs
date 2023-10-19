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

    public static event Action<SeasonStage, int> OnAdvance;

    public static GameManager Instance;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Advance();
        }
    }

    public void ChangeSeasonStage(SeasonStage newSeasonStage)
    {
        _currentWeek = 0;
        _currentSeasonStage = newSeasonStage;

        switch (_currentSeasonStage)
        {
            case SeasonStage.RegularSeason:
                break;
            case SeasonStage.Playoffs:
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
