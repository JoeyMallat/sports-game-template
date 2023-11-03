using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MM_PlayView : MonoBehaviour, ISettable
{
    [Header("Regular Season")]
    [SerializeField] RegularSeasonPlayView _regularSeasonGameObject;


    [Header("Playoffs")]
    [SerializeField] PlayoffsPlayView _playoffsGameObject;
    [SerializeField] GameObject _noGameGameObject;


    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;

        _regularSeasonGameObject.gameObject.SetActive(false);
        _playoffsGameObject.gameObject.SetActive(false);
        _noGameGameObject.SetActive(false);


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
                break;
            case SeasonStage.OffSeason:
                break;
            default:
                break;
        }
    }
}
