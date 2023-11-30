using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MM_PlayView : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _simButtons;

    [Header("Regular Season")]
    [SerializeField] RegularSeasonPlayView _regularSeasonGameObject;


    [Header("Playoffs")]
    [SerializeField] PlayoffsPlayView _playoffsGameObject;
    [SerializeField] GameObject _noGameGameObject;

    [Header("Off Season")]
    [SerializeField] GameObject _freeAgencyGameObject;


    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;

        _regularSeasonGameObject.gameObject.SetActive(false);
        _playoffsGameObject.gameObject.SetActive(false);
        _noGameGameObject.SetActive(false);
        _freeAgencyGameObject.SetActive(false);
        _simButtons.SetActive(false);


        switch (GameManager.Instance.GetSeasonStage())
        {
            case SeasonStage.RegularSeason:
                if (LeagueSystem.Instance.HasNextGame())
                {
                    _regularSeasonGameObject.gameObject.SetActive(true);
                    _regularSeasonGameObject.SetDetails(team);
                } else
                {
                    _noGameGameObject.gameObject.SetActive(true);
                }
                _simButtons.SetActive(true);
                break;
            case SeasonStage.Playoffs:
                if (PlayoffSystem.Instance.IsTeamInPlayoffs())
                {
                    _playoffsGameObject.gameObject.SetActive(true);
                    _playoffsGameObject.SetDetails(team);
                } else
                {
                    _noGameGameObject.gameObject.SetActive(true);
                }
                _simButtons.SetActive(true);
                break;
            case SeasonStage.OffSeason:
                _freeAgencyGameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
}
