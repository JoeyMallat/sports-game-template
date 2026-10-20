using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviour
{
    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _playerFirstName;
    [SerializeField] TextMeshProUGUI _playerLastName;
    [SerializeField] TextMeshProUGUI _jerseyNumber; //Format: "#23"
    [SerializeField] TextMeshProUGUI _playerPosition; //Format: "PG", "SG", "SF", "PF", "C"
    [SerializeField] TextMeshProUGUI _playerRating;
    [SerializeField] TextMeshProUGUI _pointsPerGame;
    [SerializeField] TextMeshProUGUI _assistsPerGame;
    [SerializeField] TextMeshProUGUI _reboundsPerGame; //Format (all stats): "10.2"
    [SerializeField] TextMeshProUGUI _salary; //Format: "$1,0m"
    [SerializeField] Image _teamLogo;

    readonly Dictionary<TextMeshProUGUI, Coroutine> _statAnimations = new Dictionary<TextMeshProUGUI, Coroutine>();
    readonly Dictionary<TextMeshProUGUI, Vector3> _statScales = new Dictionary<TextMeshProUGUI, Vector3>();
    readonly Dictionary<TextMeshProUGUI, Color> _statColors = new Dictionary<TextMeshProUGUI, Color>();
    int _matchPoints;
    int _matchAssists;
    int _matchRebounds;

    public void SetPlayerDetails(Player player, bool withLink = true, bool withPosition = true)
    {
        SetPortrait(player);

        if (_playerFirstName != null)
            _playerFirstName.text = _playerLastName == null ? player.GetFullName() : player.GetFirstName();

        if (_playerLastName != null)
            _playerLastName.text = player.GetLastName();

        if (_jerseyNumber != null)
            _jerseyNumber.text = $"#{player.GetJerseyNumber()}";

        if (_playerPosition != null)
            _playerPosition.text = withPosition ? GetPositionAbbreviation(player.GetPosition()) : string.Empty;

        if (_playerRating != null)
            _playerRating.text = player.CalculateRatingForPosition().ToString();

        PlayerSeason season = player.GetLatestSeason();

        if (_pointsPerGame != null)
            _pointsPerGame.text = season.GetAveragePoints().ToString("F1");

        if (_assistsPerGame != null)
            _assistsPerGame.text = season.GetAverageOfStat("assists").ToString("F1");

        if (_reboundsPerGame != null)
            _reboundsPerGame.text = season.GetAverageOfStat("rebounds").ToString("F1");

        if (_salary != null)
            _salary.text = $"${player.GetContract().GetYearlySalary().ConvertToMonetaryString()}";

        SetTeamLogo(player.GetTeamID());

        if (withLink)
            SetButton(player);
    }

    public void SetMatchDetails(Player player, bool isOnCourt)
    {
        StopMatchAnimations();

        SetPortrait(player);

        string displayName = string.IsNullOrEmpty(player.GetFirstName())
            ? player.GetLastName()
            : $"{player.GetFirstName()[0]}. {player.GetLastName()}";
        if (_playerFirstName != null)
        {
            _playerFirstName.text = displayName;
            if (_playerLastName != null)
                _playerLastName.text = string.Empty;
        }
        else if (_playerLastName != null)
        {
            _playerLastName.text = displayName;
        }

        SetNameColor(isOnCourt);

        if (_playerPosition != null)
            _playerPosition.text = GetPositionAbbreviation(player.GetPosition());
        if (_playerRating != null)
            _playerRating.text = player.CalculateRatingForPosition().ToString();

        HideMatchUnusedField(_jerseyNumber);
        HideMatchUnusedField(_salary);
        if (_teamLogo != null)
            _teamLogo.gameObject.SetActive(false);

        _matchPoints = 0;
        _matchAssists = 0;
        _matchRebounds = 0;
        SetStatText(_pointsPerGame, 0);
        SetStatText(_assistsPerGame, 0);
        SetStatText(_reboundsPerGame, 0);

        Button button = GetComponent<Button>();
        if (button != null)
            button.onClick.RemoveAllListeners();
    }

    public void UpdateMatchDetails(BasketballPlayerBoxScore boxScore, bool isOnCourt, Color highlightColor, float bounceScale, float bounceDuration)
    {
        SetNameColor(isOnCourt);
        if (boxScore == null)
            return;

        UpdateMatchStat(_pointsPerGame, ref _matchPoints, boxScore.Points, highlightColor, bounceScale, bounceDuration);
        UpdateMatchStat(_assistsPerGame, ref _matchAssists, boxScore.Assists, highlightColor, bounceScale, bounceDuration);
        UpdateMatchStat(_reboundsPerGame, ref _matchRebounds, boxScore.Rebounds, highlightColor, bounceScale, bounceDuration);
    }

    void SetNameColor(bool isOnCourt)
    {
        Color color = isOnCourt ? Color.white : new Color(0.65f, 0.65f, 0.65f, 1f);
        if (_playerFirstName != null)
            _playerFirstName.color = color;
        if (_playerLastName != null)
            _playerLastName.color = color;
    }

    void SetPortrait(Player player)
    {
        if (_playerPortrait == null)
            return;

        Sprite portrait = player.GetPlayerPortrait();
        _playerPortrait.sprite = portrait;
        _playerPortrait.overrideSprite = portrait;
        _playerPortrait.enabled = portrait != null;
        _playerPortrait.SetAllDirty();
    }

    static void HideMatchUnusedField(TextMeshProUGUI field)
    {
        if (field != null)
            field.gameObject.SetActive(false);
    }

    static void SetStatText(TextMeshProUGUI statText, int value)
    {
        if (statText != null)
            statText.text = value.ToString();
    }

    void UpdateMatchStat(TextMeshProUGUI statText, ref int currentValue, int newValue, Color highlightColor, float bounceScale, float bounceDuration)
    {
        if (statText == null || currentValue == newValue)
            return;

        currentValue = newValue;
        statText.text = newValue.ToString();
        StartStatBounce(statText, highlightColor, bounceScale, bounceDuration);
    }

    void StartStatBounce(TextMeshProUGUI statText, Color highlightColor, float bounceScale, float bounceDuration)
    {
        if (!_statScales.TryGetValue(statText, out Vector3 normalScale))
        {
            normalScale = statText.rectTransform.localScale;
            _statScales[statText] = normalScale;
            _statColors[statText] = statText.color;
        }

        if (_statAnimations.TryGetValue(statText, out Coroutine runningAnimation))
            StopCoroutine(runningAnimation);

        statText.rectTransform.localScale = normalScale;
        statText.color = _statColors[statText];
        _statAnimations[statText] = StartCoroutine(BounceStat(statText, highlightColor, bounceScale, bounceDuration));
    }

    IEnumerator BounceStat(TextMeshProUGUI statText, Color highlightColor, float bounceScale, float bounceDuration)
    {
        Vector3 normalScale = _statScales[statText];
        Color normalColor = _statColors[statText];
        float duration = Mathf.Max(0.001f, bounceDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float pulse = Mathf.Sin(progress * Mathf.PI);
            statText.rectTransform.localScale = normalScale * Mathf.Lerp(1f, Mathf.Max(1f, bounceScale), pulse);
            statText.color = Color.Lerp(normalColor, highlightColor, pulse);
            yield return null;
        }

        statText.rectTransform.localScale = normalScale;
        statText.color = normalColor;
        _statAnimations.Remove(statText);
    }

    public void StopMatchAnimations()
    {
        foreach (Coroutine animation in _statAnimations.Values)
            StopCoroutine(animation);
        _statAnimations.Clear();

        foreach (KeyValuePair<TextMeshProUGUI, Vector3> statScale in _statScales)
        {
            if (statScale.Key == null)
                continue;

            statScale.Key.rectTransform.localScale = statScale.Value;
            statScale.Key.color = _statColors[statScale.Key];
        }
    }

    private string GetPositionAbbreviation(string position)
    {
        switch (position)
        {
            case "Point Guard":
                return "PG";
            case "Shooting Guard":
                return "SG";
            case "Small Forward":
                return "SF";
            case "Power Forward":
                return "PF";
            case "Center":
                return "C";
            default:
                return position;
        }
    }

    private void SetTeamLogo(int teamID)
    {
        if (_teamLogo == null)
            return;

        bool hasTeam = teamID >= 0;
        _teamLogo.gameObject.SetActive(hasTeam);

        if (hasTeam)
            _teamLogo.sprite = LeagueSystem.Instance.GetTeam(teamID).GetTeamLogo();
    }

    private void SetButton(Player player)
    {
        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Player, player));
        }
    }

    void OnDisable()
    {
        StopMatchAnimations();
    }
}
