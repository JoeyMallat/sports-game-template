using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeBudgetComponent : MonoBehaviour, ISettable
{
    [SerializeField] Button _button;

    [SerializeField] TextMeshProUGUI _budgetText;

    public void SetDetails<T>(T item, bool showMatchButton = false) where T : class
    {
        Team team = item as Team;

        if (team == null)
        {
            return;
        }

        float salaryCap = ConfigManager.Instance.GetCurrentConfig().SalaryCap * (1 + GameManager.Instance.GetSalaryCapIncrease());
        _budgetText.text = $"{BudgetView.FormatCurrency(team.GetTotalSalaryAmount())} / {BudgetView.FormatCurrency(salaryCap)}";

        if (_button == null)
        {
            return;
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Budget, team, showMatchButton));
    }
}
