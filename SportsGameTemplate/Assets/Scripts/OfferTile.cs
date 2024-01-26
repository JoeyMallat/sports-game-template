using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfferTile : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _amountText;
    [SerializeField] Button _lowerButton;
    [SerializeField] Button _higherButton;
    [SerializeField] float _changeAmount;
    [SerializeField] bool _isSalaryAmount;

    public static event Action<float> OnAmountUpdated;
    public static event Action<int> OnLengthUpdated;

    public void InitializeTile(float currentAmount)
    {
        SetButtons(currentAmount);
        UpdateAmountText(currentAmount);
    }

    public void SetButtons(float currentAmount)
    {
        _lowerButton.onClick.RemoveAllListeners();
        _lowerButton.onClick.AddListener(() => LowerAmount(currentAmount));
        _lowerButton.onClick.AddListener(() => UpdateAmountText(LowerAmount(currentAmount)));

        _higherButton.onClick.RemoveAllListeners();
        _higherButton.onClick.AddListener(() => HigherAmount(currentAmount));
        _higherButton.onClick.AddListener(() => UpdateAmountText(HigherAmount(currentAmount)));
    }

    public float LowerAmount(float currentAmount)
    {
        return currentAmount - _changeAmount;
    }

    public float HigherAmount(float currentAmount)
    {
        return currentAmount + _changeAmount;
    }

    public void UpdateAmountText(float currentAmount)
    {
        if (_isSalaryAmount)
        {
            _amountText.text = $"{currentAmount.ConvertToMonetaryString()}";
            OnAmountUpdated?.Invoke(currentAmount);
        }
        else
        {
            if (currentAmount < 6)
            {
                _amountText.text = $"{currentAmount} YRS";
                OnLengthUpdated?.Invoke((int)currentAmount);
            }
        }

        SetButtons(currentAmount);
    }
}
