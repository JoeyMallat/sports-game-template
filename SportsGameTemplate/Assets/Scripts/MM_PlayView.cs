using UnityEngine;

public class MM_PlayView : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _simButtons;
    [SerializeField] GameObject _skipButtons;

    [Header("Regular Season")]
    [SerializeField] RegularSeasonPlayView _regularSeasonGameObject;


    [Header("Playoffs")]
    [SerializeField] PlayoffsPlayView _playoffsGameObject;
    [SerializeField] GameObject _noGameGameObject;
    [SerializeField] GameObject _waitingForNextRoundGameObject;

    [Header("Off Season")]
    [SerializeField] GameObject _freeAgencyGameObject;


    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;

        _regularSeasonGameObject.gameObject.SetActive(false);
        _playoffsGameObject.gameObject.SetActive(false);
        _noGameGameObject.SetActive(false);
        _waitingForNextRoundGameObject.gameObject.SetActive(false);
        _freeAgencyGameObject.SetActive(false);
        _simButtons.SetActive(false);
        _skipButtons.SetActive(false);

        switch (GameManager.Instance.GetSeasonStage())
        {
            case SeasonStage.RegularSeason:
                if (LeagueSystem.Instance.HasNextGame())
                {
                    _regularSeasonGameObject.gameObject.SetActive(true);
                    _regularSeasonGameObject.SetDetails(team);
                    _simButtons.SetActive(true);
                }
                else
                {
                    _noGameGameObject.gameObject.SetActive(true);
                    _skipButtons.SetActive(true);
                }
                break;
            case SeasonStage.Playoffs:
                if (PlayoffSystem.Instance.IsTeamInPlayoffs() && PlayoffSystem.Instance.MyTeamSeriesOver())
                {
                    _waitingForNextRoundGameObject.gameObject.SetActive(true);
                    _skipButtons.SetActive(true);
                }
                else if (PlayoffSystem.Instance.IsTeamInPlayoffs())
                {
                    _playoffsGameObject.gameObject.SetActive(true);
                    _playoffsGameObject.SetDetails(team);
                    _simButtons.SetActive(true);
                }
                else
                {
                    _noGameGameObject.gameObject.SetActive(true);
                    _skipButtons.SetActive(true);
                }
                break;
            case SeasonStage.OffSeason:
                _freeAgencyGameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
}
