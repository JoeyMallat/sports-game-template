using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MM_OfficeView : MonoBehaviour, ISettable
{
    [SerializeField] Image _salaryFill;
    [SerializeField] TextMeshProUGUI _salaryText;

    [SerializeField] Button _freeSpinButton;

    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;

        float percentage = (float)team.GetTotalSalaryAmount() / (float)ConfigManager.Instance.GetCurrentConfig().SalaryCap;

        _salaryFill.fillAmount = percentage;
        _salaryText.text = $"{team.GetTotalSalaryAmount().ConvertToMonetaryString()}  <color=\"white\">/  {ConfigManager.Instance.GetCurrentConfig().SalaryCap.ConvertToMonetaryString()}";

        _freeSpinButton.onClick.RemoveAllListeners();
        _freeSpinButton.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.BallGame)));
    }
}
