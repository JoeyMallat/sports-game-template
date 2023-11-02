using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ContractNegotiationsView : MonoBehaviour, ISettable
{
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
        _signContractButton.gameObject.SetActive(false);

        if (salary)
        {
            if (length)
            {
                _signContractButton.gameObject.SetActive(true);
                _playerAcceptingStatus.text = _playerWillAcceptText.Replace("{{PLAYERNAME}}", player.GetFullName());
                SetButton();
            } else
            {
                _playerAcceptingStatus.text = _willNotAcceptLengthText.Replace("{{PLAYERNAME}}", player.GetFullName());
            }
        } else
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
        _signContractButton.onClick.AddListener(() => OnContractSigned?.Invoke());
    }

    public void SetDetails<T>(T item) where T : class
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
    }

    private void UpdateSalaryCapImpact(int total, int change)
    {
        _totalSalaryAmount.fillAmount = (float)total / (float)ConfigManager.Instance.GetCurrentConfig().SalaryCap;
        _totalSalaryAmountAndChange.fillAmount = (float)(total + change) / (float)ConfigManager.Instance.GetCurrentConfig().SalaryCap;

        if (change != 0)
        {
            _salaryChangeIndication.text = $"{total.ConvertToMonetaryString()} -> {(total + change).ConvertToMonetaryString()}<color=\"white\"> / {ConfigManager.Instance.GetCurrentConfig().SalaryCap.ConvertToMonetaryString()}";
        } else
        {
            _salaryChangeIndication.text = $"{total.ConvertToMonetaryString()}<color=\"white\"> / {ConfigManager.Instance.GetCurrentConfig().SalaryCap.ConvertToMonetaryString()}";
        }
    }
}
