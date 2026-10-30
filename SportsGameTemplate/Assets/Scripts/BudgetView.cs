using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BudgetView : MonoBehaviour, ISettable
{
    [SerializeField] TextMeshProUGUI _totalSalarySpending;
    [SerializeField] TextMeshProUGUI _salaryCap;
    [SerializeField] TextMeshProUGUI _salaryLeft;
    [SerializeField] Image _starterFill;
    [SerializeField] Image _benchFill;
    [SerializeField] Image _reserveFill;
    [SerializeField] BudgetViewBreakdownObject _startersBreakdown;
    [SerializeField] BudgetViewBreakdownObject _benchBreakdown;
    [SerializeField] BudgetViewBreakdownObject _reserveBreakdown;
    [SerializeField] Transform _contractsRoot;
    [SerializeField] Button _viewAllButton;

    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null || Navigation.Instance == null || Navigation.Instance.CanvasDatabase == null)
            return;

        Navigation.Instance.CanvasDatabase[CanvasKey.Budget] = canvas;
        if (Navigation.Instance.CurrentScreen != CanvasKey.Budget)
            canvas.enabled = false;
    }

    public void SetDetails<T>(T item, bool showMatchButton = false) where T : class
    {
        Team team = item as Team;
        if (team == null)
            return;

        List<Player> players = team.GetPlayersFromTeam();
        HashSet<string> starterIds = new HashSet<string>(
            team.GetLineup().Where(id => !string.IsNullOrEmpty(id)));
        List<Player> starters = players.Where(player => starterIds.Contains(player.GetTradeableID())).ToList();
        List<Player> bench = players.Where(player => !starterIds.Contains(player.GetTradeableID()) && player.GetMinutes() > 8).ToList();
        List<Player> reserves = players.Where(player => !starterIds.Contains(player.GetTradeableID()) && player.GetMinutes() <= 8).ToList();

        int starterSpending = GetSalarySpending(starters);
        int benchSpending = GetSalarySpending(bench);
        int reserveSpending = GetSalarySpending(reserves);
        int totalSpending = starterSpending + benchSpending + reserveSpending;
        float salaryCap = ConfigManager.Instance.GetCurrentConfig().SalaryCap
            * (1f + GameManager.Instance.GetSalaryCapIncrease());

        _totalSalarySpending.text = FormatCurrency(totalSpending);
        _salaryCap.text = $"/ {FormatCurrency(salaryCap)}";
        _salaryLeft.text = $"{FormatCurrency(salaryCap - totalSpending)} left";

        SetFillAmounts(starterSpending, benchSpending, reserveSpending, salaryCap);
        _startersBreakdown.SetDetails("Starters", starterSpending, totalSpending, _starterFill.color);
        _benchBreakdown.SetDetails("Bench", benchSpending, totalSpending, _benchFill.color);
        _reserveBreakdown.SetDetails("Reserve", reserveSpending, totalSpending, _reserveFill.color);
        SetContractRows(players);

        _viewAllButton.onClick.RemoveAllListeners();
        _viewAllButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Team, team));
    }

    void SetFillAmounts(int starterSpending, int benchSpending, int reserveSpending, float salaryCap)
    {
        if (salaryCap <= 0f)
        {
            _starterFill.fillAmount = 0f;
            _benchFill.fillAmount = 0f;
            _reserveFill.fillAmount = 0f;
            return;
        }

        _starterFill.fillAmount = Mathf.Clamp01(starterSpending / salaryCap);
        _benchFill.fillAmount = Mathf.Clamp01((starterSpending + benchSpending) / salaryCap);
        _reserveFill.fillAmount = Mathf.Clamp01((starterSpending + benchSpending + reserveSpending) / salaryCap);
    }

    void SetContractRows(List<Player> players)
    {
        List<Player> orderedPlayers = players
            .OrderByDescending(player => player.GetContract().GetYearlySalary())
            .ThenByDescending(player => player.CalculateRatingForPosition())
            .ToList();
        List<PlayerItem> contractRows = _contractsRoot.GetComponentsInChildren<PlayerItem>(true).ToList();

        for (int i = 0; i < contractRows.Count; i++)
        {
            bool hasPlayer = i < orderedPlayers.Count;
            contractRows[i].gameObject.SetActive(hasPlayer);
            if (hasPlayer)
                contractRows[i].SetPlayerDetails(orderedPlayers[i]);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_contractsRoot as RectTransform);
    }

    static int GetSalarySpending(IEnumerable<Player> players)
    {
        return players.Sum(player => player.GetContract().GetYearlySalary());
    }

    public static string FormatCurrency(float amount)
    {
        float absoluteAmount = Mathf.Abs(amount);
        string suffix = absoluteAmount < 1000000f ? "K" : "M";
        float divisor = suffix == "K" ? 1000f : 1000000f;
        string sign = amount < 0f ? "-" : string.Empty;
        return $"{sign}${absoluteAmount / divisor:F1}{suffix}";
    }
}
