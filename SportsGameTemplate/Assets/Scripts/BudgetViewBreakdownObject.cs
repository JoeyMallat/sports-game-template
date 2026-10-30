using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BudgetViewBreakdownObject : MonoBehaviour
{
    [SerializeField] Image _categoryColor;
    [SerializeField] TextMeshProUGUI _title;
    [SerializeField] TextMeshProUGUI _amount;
    [SerializeField] TextMeshProUGUI _percentageOfTotalSpend;

    public void SetDetails(string title, int amount, int totalSpending, Color categoryColor)
    {
        _categoryColor.color = categoryColor;
        _title.text = title;
        _amount.text = BudgetView.FormatCurrency(amount);
        _percentageOfTotalSpend.text = totalSpending > 0
            ? $"{amount / (float)totalSpending * 100f:F1}%"
            : "0.0%";
    }
}
