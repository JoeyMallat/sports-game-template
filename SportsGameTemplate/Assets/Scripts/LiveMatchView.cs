using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LiveMatchView : MonoBehaviour, ISettable
{
    [Header("Team details")]
    [SerializeField] Image _homeTeamLogo;
    [SerializeField] Image _homePossessionIndicator;
    [SerializeField] TextMeshProUGUI _homeTeamName;
    [SerializeField] TextMeshProUGUI _homeTeamScore;
    [SerializeField] Image _awayTeamLogo;
    [SerializeField] Image _awayPossessionIndicator;
    [SerializeField] TextMeshProUGUI _awayTeamName;
    [SerializeField] TextMeshProUGUI _awayTeamScore;

    [Header("Score Animation")]
    [SerializeField] Color _scoreHighlightColor = new Color(1f, 0.82f, 0.2f, 1f);
    [SerializeField, Min(1f)] float _scoreBounceScale = 1.15f;
    [SerializeField, Min(0.05f)] float _scoreBounceDuration = 0.22f;

    [Header("Time")]

    [SerializeField] TextMeshProUGUI _timeRemainingInQuarter; //Format: 12:34
    [SerializeField] TextMeshProUGUI _currentQuarter; //Format: Q4
    [SerializeField] TextMeshProUGUI _shotClock; //Format: 24

    [Header("Action Bar")]
    [SerializeField] Image _playerPortrait;
    [SerializeField] Image _playerTeamLogo; //Change to team logo of player currently in action
    [SerializeField] Image _teamColorIndicator; //Change to team color of player currently in action
    [SerializeField] TextMeshProUGUI _playerName; //Format: #10 Firstname Lastname
    [SerializeField] TextMeshProUGUI _actionDescription; //Format: "Player pulls up for a 3-point shot!"

    [Header("Court")]
    [SerializeField] LiveMatchCourt _court;

    [Header("Playback")]
    [SerializeField, Min(1f)] float _betweenActionSpeed = 12f;
    [SerializeField, Min(0f)] float _minimumShotOutcomeTime = 0.15f;
    [SerializeField, Min(0f)] float _maximumShotOutcomeTime = 2.5f;
    [SerializeField, Min(0f)] float _shotResultHoldDuration = 0.5f;
    [SerializeField, Min(0f)] float _timeoutPresentationDuration = 1.5f;
    [SerializeField, Min(0f)] float _timeoutEndMessageDuration = 0.75f;
    [SerializeField] Color _homeTeamColor = new Color(0.15f, 0.45f, 0.95f, 1f);
    [SerializeField] Color _awayTeamColor = new Color(0.95f, 0.25f, 0.2f, 1f);

    Match _match;
    Team _homeTeam;
    Team _awayTeam;
    IBasketballMatchTimeline _timeline;
    readonly Dictionary<string, Player> _playersById = new Dictionary<string, Player>();
    Coroutine _playbackCoroutine;
    Coroutine _homeScoreBounceCoroutine;
    Coroutine _awayScoreBounceCoroutine;
    Color _homeScoreNormalColor;
    Color _awayScoreNormalColor;
    Vector3 _homeScoreNormalScale;
    Vector3 _awayScoreNormalScale;
    int _possessionStartPeriod = -1;
    int _possessionStartSeconds = -1;

    void Awake()
    {
        _homeScoreNormalColor = _homeTeamScore.color;
        _awayScoreNormalColor = _awayTeamScore.color;
        _homeScoreNormalScale = _homeTeamScore.rectTransform.localScale;
        _awayScoreNormalScale = _awayTeamScore.rectTransform.localScale;
    }

    public void SetDetails<T>(T item) where T : class
    {
        Match match = item as Match;
        if (match == null)
        {
            Debug.LogWarning("LiveMatchView requires a Match.");
            return;
        }

        Initialize(match);
    }

    public void Initialize(Match match)
    {
        if (match == null)
            return;

        StopPlayback();
        _match = match;
        _homeTeam = LeagueSystem.Instance.GetTeam(match.GetHomeTeamID());
        _awayTeam = LeagueSystem.Instance.GetTeam(match.GetAwayTeamID());

        CachePlayers();
        SetTeamDetails();
        ResetPresentation();

        if (!match.HasDetailedTimeline() && !match.GetMatchStatus())
        {
            int forcedWinner = GameManager.Instance.GetCurrentForceWinState() ? GameManager.Instance.GetTeamID() : -1;
            BasketballMatchSimulationRouter.Simulate(match, forcedWinner);
        }

        _timeline = match.GetTimeline();
        if (_timeline == null || _timeline.EventCount == 0)
        {
            ShowFinalScoreWithoutTimeline();
            return;
        }

        _playbackCoroutine = StartCoroutine(PlayTimeline());
    }

    IEnumerator PlayTimeline()
    {
        BasketballGameClock displayedClock = new BasketballGameClock(1, 720);

        for (int eventIndex = 0; eventIndex < _timeline.EventCount; eventIndex++)
        {
            BasketballMatchEvent matchEvent = _timeline.GetEvent(eventIndex);
            yield return AnimateGap(displayedClock, matchEvent.Clock);

            BasketballMatchState state = _timeline.GetStateAt(eventIndex);
            displayedClock = matchEvent.Clock;

            if (IsShot(matchEvent.EventType))
            {
                yield return PresentShot(matchEvent, state);
                continue;
            }

            if (matchEvent.EventType == BasketballEventType.Timeout)
            {
                yield return PresentTimeout(matchEvent, state);
                continue;
            }

            ApplyEvent(matchEvent, state);

            float presentationDuration = Mathf.Max(0f, matchEvent.RecommendedAnimationDuration);
            if (!IsActionMoment(matchEvent.EventType))
                presentationDuration /= Mathf.Max(1f, _betweenActionSpeed);
            if (presentationDuration > 0f)
                yield return new WaitForSeconds(presentationDuration);
        }

        _playbackCoroutine = null;
    }

    IEnumerator AnimateGap(BasketballGameClock from, BasketballGameClock to)
    {
        if (from.Period != to.Period || from.SecondsRemaining <= to.SecondsRemaining)
        {
            SetClock(to.Period, to.SecondsRemaining);
            yield break;
        }

        int elapsedGameSeconds = from.SecondsRemaining - to.SecondsRemaining;
        float duration = elapsedGameSeconds / Mathf.Max(1f, _betweenActionSpeed);
        if (duration <= 0f)
            yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            int secondsRemaining = Mathf.RoundToInt(Mathf.Lerp(from.SecondsRemaining, to.SecondsRemaining, progress));
            SetClock(to.Period, secondsRemaining);
            yield return null;
        }

        SetClock(to.Period, to.SecondsRemaining);
    }

    IEnumerator PresentShot(BasketballMatchEvent matchEvent, BasketballMatchState state)
    {
        SetClock(state.CurrentClock.Period, state.CurrentClock.SecondsRemaining);
        SetActionPlayer(matchEvent);
        _actionDescription.text = GetShotAttemptDescription(matchEvent);

        bool homeAttacksRight = matchEvent.Clock.Period % 2 == 1;
        bool attacksRight = matchEvent.TeamId == _homeTeam.GetTeamID() ? homeAttacksRight : !homeAttacksRight;
        float outcomeDuration = GetShotOutcomeDuration(matchEvent);

        yield return _court.ShowShot(
            matchEvent.CourtPosition,
            attacksRight,
            matchEvent.IsMade,
            matchEvent.IsBlocked,
            outcomeDuration,
            () => RevealShotOutcome(matchEvent, state));

        if (_shotResultHoldDuration > 0f)
            yield return new WaitForSeconds(_shotResultHoldDuration);
    }

    IEnumerator PresentTimeout(BasketballMatchEvent matchEvent, BasketballMatchState state)
    {
        ApplyEvent(matchEvent, state);
        Team timeoutTeam = matchEvent.TeamId == _homeTeam.GetTeamID() ? _homeTeam : _awayTeam;
        _actionDescription.text = $"{timeoutTeam.GetTeamName()} timeout";

        float timeoutDuration = Mathf.Max(_timeoutPresentationDuration, matchEvent.RecommendedAnimationDuration);
        if (timeoutDuration > 0f)
            yield return new WaitForSeconds(timeoutDuration);

        _actionDescription.text = "Timeout over — play resumes";
        if (_timeoutEndMessageDuration > 0f)
            yield return new WaitForSeconds(_timeoutEndMessageDuration);
    }

    void RevealShotOutcome(BasketballMatchEvent matchEvent, BasketballMatchState state)
    {
        _actionDescription.text = matchEvent.Description;
        _homeTeamScore.text = state.HomeScore.ToString();
        _awayTeamScore.text = state.AwayScore.ToString();
        if (matchEvent.Points > 0)
            StartScoreBounce(matchEvent.TeamId == _homeTeam.GetTeamID());
        SetPossession(state.PossessionTeamId);
    }

    void StartScoreBounce(bool homeTeamScored)
    {
        TextMeshProUGUI scoreText = homeTeamScored ? _homeTeamScore : _awayTeamScore;
        Color normalColor = homeTeamScored ? _homeScoreNormalColor : _awayScoreNormalColor;
        Vector3 normalScale = homeTeamScored ? _homeScoreNormalScale : _awayScoreNormalScale;
        Coroutine runningCoroutine = homeTeamScored ? _homeScoreBounceCoroutine : _awayScoreBounceCoroutine;

        if (runningCoroutine != null)
            StopCoroutine(runningCoroutine);

        scoreText.color = normalColor;
        scoreText.rectTransform.localScale = normalScale;
        Coroutine coroutine = StartCoroutine(BounceScore(scoreText, normalColor, normalScale, homeTeamScored));
        if (homeTeamScored)
            _homeScoreBounceCoroutine = coroutine;
        else
            _awayScoreBounceCoroutine = coroutine;
    }

    IEnumerator BounceScore(TextMeshProUGUI scoreText, Color normalColor, Vector3 normalScale, bool homeTeamScore)
    {
        float elapsed = 0f;
        while (elapsed < _scoreBounceDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / _scoreBounceDuration);
            float pulse = Mathf.Sin(progress * Mathf.PI);
            scoreText.rectTransform.localScale = normalScale * Mathf.Lerp(1f, _scoreBounceScale, pulse);
            scoreText.color = Color.Lerp(normalColor, _scoreHighlightColor, pulse);
            yield return null;
        }

        scoreText.rectTransform.localScale = normalScale;
        scoreText.color = normalColor;
        if (homeTeamScore)
            _homeScoreBounceCoroutine = null;
        else
            _awayScoreBounceCoroutine = null;
    }

    void ApplyEvent(BasketballMatchEvent matchEvent, BasketballMatchState state)
    {
        if (matchEvent.EventType == BasketballEventType.PeriodStarted)
        {
            _possessionStartPeriod = -1;
            _possessionStartSeconds = -1;
            _court.ClearShots();
        }
        else if (matchEvent.EventType == BasketballEventType.PossessionStarted)
        {
            _possessionStartPeriod = matchEvent.Clock.Period;
            _possessionStartSeconds = matchEvent.Clock.SecondsRemaining;
        }

        SetClock(state.CurrentClock.Period, state.CurrentClock.SecondsRemaining);
        _homeTeamScore.text = state.HomeScore.ToString();
        _awayTeamScore.text = state.AwayScore.ToString();
        SetPossession(state.PossessionTeamId);

        if (IsActionMoment(matchEvent.EventType))
            SetAction(matchEvent);

    }

    void SetTeamDetails()
    {
        _homeTeamName.text = _homeTeam.GetTeamName();
        _homeTeamLogo.sprite = _homeTeam.GetTeamLogo();
        _awayTeamName.text = _awayTeam.GetTeamName();
        _awayTeamLogo.sprite = _awayTeam.GetTeamLogo();
    }

    void ResetPresentation()
    {
        if (_court == null)
            _court = GetComponentInChildren<LiveMatchCourt>(true);

        StopScoreBounces();
        _court.ClearShots();
        _homeTeamScore.text = "0";
        _awayTeamScore.text = "0";
        SetClock(1, 720);
        SetPossession(-1);
        ClearAction();
        _possessionStartPeriod = -1;
        _possessionStartSeconds = -1;
    }

    void StopScoreBounces()
    {
        if (_homeScoreBounceCoroutine != null)
            StopCoroutine(_homeScoreBounceCoroutine);
        if (_awayScoreBounceCoroutine != null)
            StopCoroutine(_awayScoreBounceCoroutine);

        _homeScoreBounceCoroutine = null;
        _awayScoreBounceCoroutine = null;
        _homeTeamScore.color = _homeScoreNormalColor;
        _awayTeamScore.color = _awayScoreNormalColor;
        _homeTeamScore.rectTransform.localScale = _homeScoreNormalScale;
        _awayTeamScore.rectTransform.localScale = _awayScoreNormalScale;
    }

    void SetClock(int period, int secondsRemaining)
    {
        int clampedSeconds = Mathf.Max(0, secondsRemaining);
        _timeRemainingInQuarter.text = $"{clampedSeconds / 60}:{clampedSeconds % 60:00}";
        _currentQuarter.text = period <= 4 ? $"Q{period}" : period == 5 ? "OT" : $"{period - 4}OT";

        int shotClockSeconds = 24;
        if (_possessionStartPeriod == period && _possessionStartSeconds >= clampedSeconds)
            shotClockSeconds = Mathf.Clamp(24 - (_possessionStartSeconds - clampedSeconds), 0, 24);
        _shotClock.text = shotClockSeconds.ToString();
    }

    void SetPossession(int teamId)
    {
        _homePossessionIndicator.gameObject.SetActive(teamId == _homeTeam.GetTeamID());
        _awayPossessionIndicator.gameObject.SetActive(teamId == _awayTeam.GetTeamID());
    }

    void SetAction(BasketballMatchEvent matchEvent)
    {
        _actionDescription.text = matchEvent.Description;
        SetActionPlayer(matchEvent);
    }

    void SetActionPlayer(BasketballMatchEvent matchEvent)
    {
        if (!_playersById.TryGetValue(matchEvent.PlayerId, out Player player))
        {
            ClearActionPlayer();
            return;
        }

        Team playerTeam = player.GetTeamID() == _homeTeam.GetTeamID() ? _homeTeam : _awayTeam;
        _playerName.text = $"#{player.GetJerseyNumber()} {player.GetFullName()}";
        _playerPortrait.sprite = player.GetPlayerPortrait();
        _playerPortrait.enabled = _playerPortrait.sprite != null;
        _playerTeamLogo.sprite = playerTeam.GetTeamLogo();
        _playerTeamLogo.enabled = _playerTeamLogo.sprite != null;
        _teamColorIndicator.color = playerTeam == _homeTeam ? _homeTeamColor : _awayTeamColor;
    }

    static string GetShotAttemptDescription(BasketballMatchEvent matchEvent)
    {
        if (matchEvent.EventType == BasketballEventType.FreeThrow)
            return $"Free throw {matchEvent.FreeThrowNumber} of {matchEvent.FreeThrowTotal}...";

        switch (matchEvent.ShotType)
        {
            case BasketballShotType.ThreePointer:
                return "Three-point attempt...";
            case BasketballShotType.Layup:
                return "Layup attempt...";
            case BasketballShotType.Dunk:
                return "Dunk attempt...";
            default:
                return "Shot attempt...";
        }
    }

    float GetShotOutcomeDuration(BasketballMatchEvent matchEvent)
    {
        float importance = (int)matchEvent.Importance / (float)BasketballEventImportance.Critical;

        if ((matchEvent.DramaticMoment & BasketballDramaticMoment.GameWinningAttempt) != 0)
            importance = 1f;
        else if ((matchEvent.DramaticMoment & BasketballDramaticMoment.BuzzerBeater) != 0)
            importance = Mathf.Max(importance, 0.9f);
        if ((matchEvent.DramaticMoment & BasketballDramaticMoment.Overtime) != 0)
            importance = Mathf.Clamp01(importance + 0.1f);

        float minimum = Mathf.Max(0f, Mathf.Min(_minimumShotOutcomeTime, _maximumShotOutcomeTime));
        float maximum = Mathf.Max(minimum, Mathf.Max(_minimumShotOutcomeTime, _maximumShotOutcomeTime));
        return Mathf.Lerp(minimum, maximum, Mathf.Clamp01(importance));
    }

    static bool IsShot(BasketballEventType eventType)
    {
        return eventType == BasketballEventType.ShotAttempt || eventType == BasketballEventType.FreeThrow;
    }

    void ClearAction()
    {
        _actionDescription.text = string.Empty;
        ClearActionPlayer();
    }

    void ClearActionPlayer()
    {
        _playerName.text = string.Empty;
        _playerPortrait.sprite = null;
        _playerPortrait.enabled = false;
        _playerTeamLogo.sprite = null;
        _playerTeamLogo.enabled = false;
        _teamColorIndicator.color = Color.clear;
    }

    void CachePlayers()
    {
        _playersById.Clear();
        AddPlayers(_homeTeam);
        AddPlayers(_awayTeam);
    }

    void AddPlayers(Team team)
    {
        foreach (Player player in team.GetPlayersFromTeam())
            _playersById[player.GetTradeableID()] = player;
    }

    void ShowFinalScoreWithoutTimeline()
    {
        (int homeScore, int awayScore) = _match.GetScore();
        _homeTeamScore.text = homeScore.ToString();
        _awayTeamScore.text = awayScore.ToString();
        _actionDescription.text = "Detailed play-by-play is unavailable for this match.";
    }

    void StopPlayback()
    {
        if (_playbackCoroutine == null)
            return;

        StopCoroutine(_playbackCoroutine);
        _playbackCoroutine = null;
    }

    static bool IsActionMoment(BasketballEventType eventType)
    {
        switch (eventType)
        {
            case BasketballEventType.ShotAttempt:
            case BasketballEventType.FreeThrow:
            case BasketballEventType.Rebound:
            case BasketballEventType.Turnover:
            case BasketballEventType.Steal:
            case BasketballEventType.Block:
            case BasketballEventType.Foul:
            case BasketballEventType.Timeout:
            case BasketballEventType.PeriodEnded:
            case BasketballEventType.GameEnded:
                return true;
            default:
                return false;
        }
    }

    void OnDisable()
    {
        StopPlayback();
        StopScoreBounces();
    }
}
