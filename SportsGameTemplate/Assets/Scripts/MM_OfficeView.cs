using TMPro;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UI;

public class MM_OfficeView : MonoBehaviour, ISettable
{
    [SerializeField] Image _salaryFill;
    [SerializeField] TextMeshProUGUI _salaryText;

    [SerializeField] Button _increaseSalaryCapButton;
    [SerializeField] TextMeshProUGUI _increaseSalaryCapText;

    [SerializeField] TextMeshProUGUI _currentBalanceText;

    [SerializeField] Button _freeSpinButton;
    [SerializeField] Button _paidSpinButton;
    [SerializeField] Button _storeButton;
    [SerializeField] TextMeshProUGUI _paidSpinCostText;

    private void Awake()
    {
        GameManager.OnGemsUpdated += UpdateBalance;
    }

    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;

        float percentage = (float)team.GetTotalSalaryAmount() / (float)ConfigManager.Instance.GetCurrentConfig().SalaryCap;

        _salaryFill.fillAmount = percentage;
        _salaryText.text = $"{team.GetTotalSalaryAmount().ConvertToMonetaryString()}  <color=\"white\">/  {ConfigManager.Instance.GetCurrentConfig().SalaryCap.ConvertToMonetaryString()}";

        _increaseSalaryCapButton.onClick.RemoveAllListeners();


        _increaseSalaryCapText.text = $"Increase salary cap\n<color=\"white\"> {RemoteConfigService.Instance.appConfig.GetInt("increasesalarycap_cost", 46)} <sprite name=\"Gem\">";

        _currentBalanceText.text = $"Balance   <color=\"white\">{GameManager.Instance.GetGems()} <sprite name=\"Gem\">";

        _freeSpinButton.onClick.RemoveAllListeners();
        _freeSpinButton.onClick.AddListener(() => GoToBallGame());

        _paidSpinButton.onClick.RemoveAllListeners();
        _paidSpinButton.onClick.AddListener(() => GoToBallGame(RemoteConfigService.Instance.appConfig.GetInt("wheelspin_cost", 12)));
        _paidSpinCostText.text = $"Spin wheel <color=\"white\">\n{RemoteConfigService.Instance.appConfig.GetInt("wheelspin_cost", 12)} <sprite name=\"Gem\">";

        _storeButton.onClick.RemoveAllListeners();
        _storeButton.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(true, CanvasKey.Store)));
    }

    private void GoToBallGame(int cost = 0)
    {
        TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.BallGame, cost));
        FindAnyObjectByType<BallSystem>().SetStartingState();
    }

    private void UpdateBalance(CloudSaveData data)
    {
        _currentBalanceText.text = $"Balance   <color=\"white\">{data.GemAmount} <sprite name=\"Gem\">";
    }
}
