using System;
using UnityEngine;

public class ContractNegotiationsSystem : MonoBehaviour
{
    Player _currentPlayer;
    float _offeredAmount;
    float _offeredLength;

    [SerializeField] AnimationCurve _contractLengthCurve;
    [SerializeField] AnimationCurve _contractAmountCurve;

    public static event Action<int, int> OnNewSalaryAmountCalculated;
    public static event Action<Player, bool, bool> OnPlayerDecisionMade; // First bool: salary - second bool: length

    private void Awake()
    {
        ContractNegotiationsView.OnNegotiationsStarted += InitializeNegotiations;
        ContractNegotiationsView.OnContractSigned += SignContract;
        OfferTile.OnAmountUpdated += UpdateSalaryCapImpact;
        OfferTile.OnAmountUpdated += SetContractSalary;
        OfferTile.OnLengthUpdated += SetContractLength;
    }

    private void SignContract()
    {
        _currentPlayer.SignContract((int)_offeredAmount, (int)_offeredLength);

        if (_currentPlayer.GetTeamID() != GameManager.Instance.GetTeamID())
        {
            LeagueSystem.Instance.GetTeam(_currentPlayer.GetTeamID()).RemovePlayer(_currentPlayer);
            LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).AddPlayer(_currentPlayer);
        }

        Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()));
        Navigation.Instance.GoToScreen(true, CanvasKey.Player, _currentPlayer);
    }

    private void SetContractSalary(float offeredAmount)
    {
        _offeredAmount = offeredAmount;
        CalculatePlayerDecision();
    }

    private void SetContractLength(int length)
    {
        _offeredLength = length;
        CalculatePlayerDecision();
    }

    private void CalculatePlayerDecision()
    {
        float ageImpactOnLength = _contractLengthCurve.Evaluate(_currentPlayer.GetAge() / 40f);
        float ageImpactOnSalary = _contractAmountCurve.Evaluate(_currentPlayer.GetAge() / 40f);

        float lowestAcceptingAmount = Mathf.Clamp(_currentPlayer.GetContract().GetYearlySalary() * (float)Mathf.Lerp(0f, 3f, ageImpactOnSalary) * StaffSystem.Instance.GetLowerSalaryPercentage(), ConfigManager.Instance.GetCurrentConfig().MinimumSalary, Mathf.Infinity);
        float shortestAcceptedContract = Mathf.Clamp(Mathf.Lerp(1f, 5f, ageImpactOnLength), 1, 5);

        OnPlayerDecisionMade?.Invoke(_currentPlayer, _offeredAmount >= lowestAcceptingAmount, _offeredLength >= shortestAcceptedContract);
    }

    private void UpdateSalaryCapImpact(float offeredAmount)
    {
        if (_currentPlayer.GetTeamID() == GameManager.Instance.GetTeamID())
        {
            int currentSalaryAmount = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetTotalSalaryAmount();
            int changeInPlayerSalary = (int)offeredAmount - _currentPlayer.GetContract().GetYearlySalary();

            OnNewSalaryAmountCalculated?.Invoke(currentSalaryAmount, changeInPlayerSalary);
        }
        else
        {
            int currentSalaryAmount = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetTotalSalaryAmount();
            int changeInPlayerSalary = (int)offeredAmount;

            OnNewSalaryAmountCalculated?.Invoke(currentSalaryAmount, changeInPlayerSalary);
        }

    }

    private void InitializeNegotiations(Player player)
    {
        _currentPlayer = player;
    }
}
