using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextMatchButton : MonoBehaviour, ISettable
{
    [SerializeField] Button _button;

    Canvas _canvas;

    [SerializeField] Image _homeTeamLogo;
    [SerializeField] Image _awayTeamLogo;
    [SerializeField] TextMeshProUGUI _homeTeamNameText;
    [SerializeField] TextMeshProUGUI _homeTeamRecordText;
    [SerializeField] TextMeshProUGUI _awayTeamNameText;
    [SerializeField] TextMeshProUGUI _awayTeamRecordText;
    [SerializeField] TextMeshProUGUI _seasonStageText; // Regular Season, Playoffs, or Finals
    [SerializeField] TextMeshProUGUI _venueText;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    public void SetVisible(bool visible)
    {
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }

        if (_canvas != null)
        {
            _canvas.enabled = visible;
        }
    }

    public void SetDetails<T>(T item, bool showMatchButton = false) where T : class
    {
        Team userTeam = item as Team;

        if (userTeam == null)
        {
            return;
        }

        Match nextMatch = GetNextMatch(userTeam.GetTeamID());

        if (nextMatch == null)
        {
            SetSeasonOverDetails(userTeam);
            return;
        }

        Team homeTeam = LeagueSystem.Instance.GetTeam(nextMatch.GetHomeTeamID());
        Team awayTeam = LeagueSystem.Instance.GetTeam(nextMatch.GetAwayTeamID());

        _homeTeamLogo.sprite = homeTeam.GetTeamLogo();
        _awayTeamLogo.sprite = awayTeam.GetTeamLogo();
        _homeTeamNameText.text = homeTeam.GetTeamName();
        _homeTeamRecordText.text = GetRecord(homeTeam);
        _awayTeamNameText.text = awayTeam.GetTeamName();
        _awayTeamRecordText.text = GetRecord(awayTeam);
        _seasonStageText.text = GetSeasonStageText();
        _venueText.text = $"{homeTeam.GetTeamName()} Arena";

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() =>
            Navigation.Instance.GoToScreen(true, CanvasKey.Prematch, userTeam)));
    }

    private Match GetNextMatch(int teamID)
    {
        if (GameManager.Instance.GetSeasonStage() == SeasonStage.RegularSeason)
        {
            return LeagueSystem.Instance.GetNextAvailableMatchForTeam(teamID);
        }

        if (GameManager.Instance.GetSeasonStage() == SeasonStage.Playoffs &&
            PlayoffSystem.Instance.IsTeamInPlayoffs() &&
            !PlayoffSystem.Instance.MyTeamSeriesOver())
        {
            PlayoffMatchup matchup = PlayoffSystem.Instance.GetNextMatchData();
            return matchup != null ? matchup.GetNextMatch() : null;
        }

        return null;
    }

    private string GetSeasonStageText()
    {
        if (GameManager.Instance.GetSeasonStage() == SeasonStage.Playoffs)
        {
            return PlayoffSystem.Instance.GetPlayoffRound() == 3 ? "Finals" : "Playoffs";
        }

        return "Regular Season";
    }

    private string GetRecord(Team team)
    {
        TeamSeason season = team.GetCurrentSeasonStats();
        return $"{season.GetWins()} - {season.GetLosses()}";
    }

    private void SetSeasonOverDetails(Team userTeam)
    {
        _homeTeamNameText.text = userTeam.GetTeamName();
        _homeTeamRecordText.text = GetRecord(userTeam);
        _awayTeamNameText.text = string.Empty;
        _awayTeamRecordText.text = string.Empty;
        _seasonStageText.text = "Season Over";
        _venueText.text = "Start next season";

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => GameManager.Instance.StartNewSeason());
    }
}
