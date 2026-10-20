using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRotationItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _positionText;
    [SerializeField] TextMeshProUGUI _numberText;
    [SerializeField] TextMeshProUGUI _firstNameText;
    [SerializeField] TextMeshProUGUI _lastNameText;
    [SerializeField] Image _portrait;
    [SerializeField] TextMeshProUGUI _overallRatingText;
    [SerializeField] Image _teamLogo;
    [SerializeField] TextMeshProUGUI _allocatedMinutesText;
    [SerializeField] Button _decreaseMinutesButton;
    [SerializeField] Button _increaseMinutesButton;
    [SerializeField] GameObject _starterTag;
    [SerializeField] Button _makeStarterButton;

    Player _player;
    Action<Player, int> _minutesChangeRequested;
    Action<Player> _starterRequested;

    public Player Player => _player;

    public void SetDetails(
        Player player,
        Team team,
        bool isStarter,
        Action<Player, int> minutesChangeRequested,
        Action<Player> starterRequested)
    {
        BindReferences();
        _player = player;
        _minutesChangeRequested = minutesChangeRequested;
        _starterRequested = starterRequested;

        _positionText.text = GetPositionAbbreviation(player.GetPosition());
        _numberText.text = $"#{player.GetJerseyNumber()}";
        _firstNameText.text = player.GetFirstName();
        _lastNameText.text = player.GetLastName();
        _overallRatingText.text = player.CalculateRatingForPosition().ToString();
        _teamLogo.sprite = team.GetTeamLogo();

        Sprite portrait = player.GetPlayerPortrait();
        _portrait.sprite = portrait;
        _portrait.overrideSprite = portrait;
        _portrait.enabled = portrait != null;

        _decreaseMinutesButton.onClick.RemoveAllListeners();
        _decreaseMinutesButton.onClick.AddListener(() => _minutesChangeRequested?.Invoke(_player, -1));
        _increaseMinutesButton.onClick.RemoveAllListeners();
        _increaseMinutesButton.onClick.AddListener(() => _minutesChangeRequested?.Invoke(_player, 1));
        _makeStarterButton.onClick.RemoveAllListeners();
        _makeStarterButton.onClick.AddListener(() => _starterRequested?.Invoke(_player));

        Refresh(player.GetMinutes(), isStarter, true);
    }

    public void Refresh(int minutes, bool isStarter, bool canIncrease)
    {
        _allocatedMinutesText.text = minutes.ToString();
        _decreaseMinutesButton.interactable = minutes > 0;
        _increaseMinutesButton.interactable = minutes < 48 && canIncrease;
        _starterTag.SetActive(isStarter);
        _makeStarterButton.gameObject.SetActive(!isStarter);
    }

    void BindReferences()
    {
        if (_positionText == null) _positionText = FindComponent<TextMeshProUGUI>("PositionAbbrev");
        if (_numberText == null) _numberText = FindComponent<TextMeshProUGUI>("Number");
        if (_firstNameText == null) _firstNameText = FindComponent<TextMeshProUGUI>("Firstname");
        if (_lastNameText == null) _lastNameText = FindComponent<TextMeshProUGUI>("Lastname");
        if (_portrait == null) _portrait = FindComponent<Image>("Player Portrait");
        if (_overallRatingText == null) _overallRatingText = FindComponent<TextMeshProUGUI>("Rating");
        if (_teamLogo == null) _teamLogo = FindComponent<Image>("TeamLogo");
        if (_allocatedMinutesText == null) _allocatedMinutesText = FindComponent<TextMeshProUGUI>("Minutes");
        if (_decreaseMinutesButton == null) _decreaseMinutesButton = FindComponent<Button>("-Button");
        if (_increaseMinutesButton == null) _increaseMinutesButton = FindComponent<Button>("+Button");
        if (_makeStarterButton == null) _makeStarterButton = FindComponent<Button>("StarterButton");
        if (_starterTag == null)
        {
            Transform starter = FindTransform("Starter");
            _starterTag = starter == null ? FindComponent<TextMeshProUGUI>("StarterText").gameObject : starter.gameObject;
        }
    }

    T FindComponent<T>(string objectName) where T : Component
    {
        T[] components = GetComponentsInChildren<T>(true);
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i].gameObject.name == objectName)
                return components[i];
        }

        return null;
    }

    Transform FindTransform(string objectName)
    {
        Transform[] transforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].gameObject.name == objectName)
                return transforms[i];
        }

        return null;
    }

    static string GetPositionAbbreviation(string position)
    {
        switch (position)
        {
            case "Point Guard": return "PG";
            case "Shooting Guard": return "SG";
            case "Small Forward": return "SF";
            case "Power Forward": return "PF";
            case "Center": return "C";
            default: return position;
        }
    }
}
