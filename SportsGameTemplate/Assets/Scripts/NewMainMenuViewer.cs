using UnityEngine;

public class NewMainMenuViewer : MonoBehaviour, ISettable
{
    [SerializeField] HomeRosterComponent _homeRosterComponent;
    [SerializeField] HomeTeamBarComponent _homeTeamBarComponent;
    [SerializeField] HomeTacticsComponent _homeTacticsComponent;
    [SerializeField] HomeCoachesComponent _homeCoachesComponent;
    [SerializeField] HomeStandingsComponent _homeStandingsComponent;
    [SerializeField] HomeTradeOffersComponent _homeTradeOffersComponent;
    [SerializeField] HomeTradeBlockComponent _homeTradeBlockComponent;
    [SerializeField] HomeTradeFinderComponent _homeTradeFinderComponent;
    [SerializeField] HomeBudgetComponent _homeBudgetComponent;
    [SerializeField] HomeFacilitiesComponent _homeFacilitiesComponent;
    [SerializeField] NextMatchButton _nextMatchButton;

    public void SetDetails<T>(T item) where T : class
    {
        OnEnable();
    }

    private void OnEnable()
    {
        Team team = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID());

        _homeTeamBarComponent.SetDetails(team);
        _homeRosterComponent.SetDetails(team);
        _homeTacticsComponent.SetDetails(team);
        _homeCoachesComponent.SetDetails(StaffSystem.Instance.GetAllStaff());
        _homeStandingsComponent.SetDetails(LeagueSystem.Instance.GetTeams());
        _homeTradeOffersComponent.SetDetails(team);
        _homeTradeBlockComponent.SetDetails(team);
        _homeTradeFinderComponent.SetDetails(team);
        _homeBudgetComponent.SetDetails(team);
        _homeFacilitiesComponent.SetDetails(team);
        _nextMatchButton.SetDetails(team);
    }
}
