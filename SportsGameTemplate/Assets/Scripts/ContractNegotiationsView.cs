using System;
using Firebase.Analytics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContractNegotiationsView : MonoBehaviour, ISettable
{
    const int SalaryCapIncreaseCost = 46;
    [SerializeField] TextMeshProUGUI _playerNameText;
    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _ageText;
    [SerializeField] TextMeshProUGUI _ratingText;
    [SerializeField] TextMeshProUGUI _positionText;

    [SerializeField] OfferTile _salaryTile;
    [SerializeField] OfferTile _contractLengthTile;

    [SerializeField] Image _totalSalaryAmount;
    [SerializeField] Image _totalSalaryAmountAndChange;
    [SerializeField] TextMeshProUGUI _salaryChangeIndication;

    [SerializeField] Button _increaseSalaryCapButton;
    [SerializeField] TextMeshProUGUI _increaseSalaryCapText;

    [SerializeField] TextMeshProUGUI _playerAcceptingStatus;
    [SerializeField] Button _signContractButton;

    [SerializeField] string _playerWillAcceptText;
    [SerializeField] string _willNotAcceptLengthText;
    [SerializeField] string _willNotAcceptSalaryText;
    [SerializeField] string _willNotAcceptBothText;

    public static event Action<Player> OnNegotiationsStarted;
    public static event Action OnContractSigned;

    private void Awake()
    {
        ContractNegotiationsSystem.OnNewSalaryAmountCalculated += UpdateSalaryCapImpact;
        ContractNegotiationsSystem.OnPlayerDecisionMade += ShowPlayerDecision;
    }

    private void ShowPlayerDecision(Player player, bool salary, bool length)
    {
        _signContractButton.ToggleButtonStatus(false);

        if (salary)
        {
            if (length)
            {
                _signContractButton.ToggleButtonStatus(true);
                _playerAcceptingStatus.text = _playerWillAcceptText.Replace("{{PLAYERNAME}}", player.GetFullName());
                SetButton();
            }
            else
            {
                _playerAcceptingStatus.text = _willNotAcceptLengthText.Replace("{{PLAYERNAME}}", player.GetFullName());
            }
        }
        else
        {
            if (length)
            {
                _playerAcceptingStatus.text = _willNotAcceptSalaryText.Replace("{{PLAYERNAME}}", player.GetFullName());
            }
            else
            {
                _playerAcceptingStatus.text = _willNotAcceptBothText.Replace("{{PLAYERNAME}}", player.GetFullName());
            }
        }
    }

    private void SetButton()
    {
        _signContractButton.onClick.RemoveAllListeners();

        if (DevelopmentPointsManager.Instance.CanAfford(1))
        {
            _signContractButton.onClick.AddListener(() =>
            {
                if (DevelopmentPointsManager.Instance.Spend(1, DevelopmentPointsSource.GameplayPurchase, DevelopmentPointsManager.CreateTransactionId("player-contract"), "Player contract negotiation"))
                    OnContractSigned?.Invoke();
            });
            _signContractButton.onClick.AddListener(() => FirebaseAnalytics.LogEvent("extended_contract"));
        }
        else
        {
            _signContractButton.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(true, CanvasKey.Store)));
        }
    }

    public void SetDetails<T>(T item, bool showMatchButton = false) where T : class
    {
        Player player = item as Player;
        OnNegotiationsStarted?.Invoke(player);

        _playerNameText.text = player.GetFullName();
        _playerPortrait.sprite = player.GetPlayerPortrait();
        _ageText.text = player.GetAge().ToString();
        _ratingText.text = player.CalculateRatingForPosition().ToString();
        _positionText.text = player.GetPosition();

        _salaryTile.InitializeTile(player.GetContract().GetYearlySalary());
        _contractLengthTile.InitializeTile(player.GetContract().GetYearsOnContract());

        _increaseSalaryCapButton.onClick.RemoveAllListeners();
        _increaseSalaryCapButton.onClick.AddListener(() =>
        {
            int cost = SalaryCapIncreaseCost;
            if (DevelopmentPointsManager.Instance.Spend(cost, DevelopmentPointsSource.GameplayPurchase, DevelopmentPointsManager.CreateTransactionId("salary-cap"), "Salary cap increase"))
            {
                GameManager.Instance.SetSalaryCapIncrease(0.2f, true);
                SetDetails(player);
            }
            else
            {
                Navigation.Instance.GoToScreen(true, CanvasKey.Store);
            }
        });
        _increaseSalaryCapText.text = $"Increase salary cap by 20%\n<color=\"white\"> {SalaryCapIncreaseCost} DP";
    }

    private void UpdateSalaryCapImpact(int total, int change)
    {
        _totalSalaryAmount.fillAmount = (float)total / (ConfigManager.Instance.GetCurrentConfig().SalaryCap * (1 + GameManager.Instance.GetSalaryCapIncrease()));
        _totalSalaryAmountAndChange.fillAmount = (float)total / (ConfigManager.Instance.GetCurrentConfig().SalaryCap * (1 + GameManager.Instance.GetSalaryCapIncrease()));

        if (change != 0)
        {
            _salaryChangeIndication.text = $"{total.ConvertToMonetaryString()} -> {(total + change).ConvertToMonetaryString()}<color=\"white\"> / {(ConfigManager.Instance.GetCurrentConfig().SalaryCap * (1 + GameManager.Instance.GetSalaryCapIncrease())).ConvertToMonetaryString()}";
        }
        else
        {
            _salaryChangeIndication.text = $"{total.ConvertToMonetaryString()}<color=\"white\"> / {(ConfigManager.Instance.GetCurrentConfig().SalaryCap * (1 + GameManager.Instance.GetSalaryCapIncrease())).ConvertToMonetaryString()}";
        }
    }
}
