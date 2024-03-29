using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamAsset : MonoBehaviour
{
    [SerializeField] TeamAssets _teamAssetParent;

    [Header("Empty Asset")]
    [SerializeField] GameObject _emptyAssetOverlay;

    [Header("Player Asset")]
    [SerializeField] GameObject _playerAssetOverlay;
    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _positionText;
    [SerializeField] TextMeshProUGUI _ratingText;
    [SerializeField] TextMeshProUGUI _ageText;
    [SerializeField] TextMeshProUGUI _contractYearlyValueText;
    [SerializeField] TextMeshProUGUI _contractLengthText;
    [SerializeField] GameObject _buttonOverlay;

    [Header("Pick Asset")]
    [SerializeField] GameObject _pickAssetOverlay;
    [SerializeField] TextMeshProUGUI _pickNumberText;
    [SerializeField] TextMeshProUGUI _pickYearText;

    [Header("Buttons")]
    [SerializeField] Button _addAssetButton;
    [SerializeField] Button _playerProfileButton;
    [SerializeField] Button _removeFromTradeButton;
    [SerializeField] Button _closeOverlayButton;

    public static event Action<int, ITradeable> OnRemoveFromTrade;

    public void SetAssetDetails(ITradeable asset = null)
    {
        _teamAssetParent = GetComponentInParent<TeamAssets>();

        if (asset == null)
        {
            SetEmptyAssetDetails();
            return;
        }

        if (asset.GetType() == typeof(Player))
        {
            SetPlayerAssetDetails(asset as Player);
        }
        else if (asset.GetType() == typeof(DraftPick))
        {
            SetPickAssetDetails(asset as DraftPick);
        }
    }

    private void SetEmptyAssetDetails()
    {
        _emptyAssetOverlay.SetActive(true);
        _playerAssetOverlay.SetActive(false);
        _pickAssetOverlay.SetActive(false);

        _addAssetButton.onClick.RemoveAllListeners();
        if (_teamAssetParent.GetTeamID() == -1)
            _addAssetButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.TeamOverview, LeagueSystem.Instance.GetTeamsSortedByID()));
        else
            _addAssetButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Team, LeagueSystem.Instance.GetTeam(_teamAssetParent.GetTeamID())));
    }

    private void SetPlayerAssetDetails(Player player)
    {
        _emptyAssetOverlay.SetActive(false);
        _playerAssetOverlay.SetActive(true);
        _pickAssetOverlay.SetActive(false);

        _playerPortrait.sprite = player.GetPlayerPortrait();
        _nameText.text = player.GetFullName();
        _positionText.text = player.GetPosition();
        _ratingText.text = player.CalculateRatingForPosition().ToString();
        _ageText.text = player.GetAge().ToString();
        _contractYearlyValueText.text = player.GetContract().GetYearlySalary().ConvertToMonetaryString();
        _contractLengthText.text = $"{player.GetContract().GetYearsOnContract()} YRS";

        SetButtons(player);
    }

    private void SetPickAssetDetails(DraftPick pick)
    {
        _emptyAssetOverlay.SetActive(false);
        _playerAssetOverlay.SetActive(false);
        _pickAssetOverlay.SetActive(true);

        _pickNumberText.text = pick.GetPickDataString();
        // TODO: Pick season

        SetButtons(pick);
    }

    private void SetButtons<T>(T item)
    {
        _playerProfileButton.onClick.RemoveAllListeners();
        _removeFromTradeButton.onClick.RemoveAllListeners();
        _closeOverlayButton.onClick.RemoveAllListeners();

        if (item.GetType() == typeof(Player))
        {
            _playerProfileButton.gameObject.SetActive(true);
            _playerProfileButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Player, item as Player));
            _playerProfileButton.onClick.AddListener(() => ToggleOverlay(false));
        }
        else
            _playerProfileButton.gameObject.SetActive(false);

        _removeFromTradeButton.onClick.AddListener(() => OnRemoveFromTrade(_teamAssetParent.GetTeamIndex(), item as ITradeable));
        _removeFromTradeButton.onClick.AddListener(() => ToggleOverlay(false));

        _closeOverlayButton.onClick.AddListener(() => ToggleOverlay(false));
    }

    public void ToggleOverlay(bool open)
    {
        _buttonOverlay.SetActive(open);
    }
}