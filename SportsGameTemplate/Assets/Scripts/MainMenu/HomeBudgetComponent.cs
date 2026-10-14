using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeBudgetComponent : MonoBehaviour, ISettable
{
    [SerializeField] Button _button;

    [SerializeField] TextMeshProUGUI _budgetText; //Format: $123.5m / $130.4m

    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;

        if (team == null)
        {
            return;
        }

        float salaryCap = ConfigManager.Instance.GetCurrentConfig().SalaryCap * (1 + GameManager.Instance.GetSalaryCapIncrease());
        _budgetText.text = $"{team.GetTotalSalaryAmount().ConvertToMonetaryString()} / {salaryCap.ConvertToMonetaryString()}";

        if (_button == null)
        {
            return;
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Store));
    }
}
