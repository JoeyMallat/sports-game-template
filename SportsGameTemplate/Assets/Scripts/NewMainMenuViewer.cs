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

    public void SetDetails<T>(T item, bool showMatchButton = false) where T : class
    {
        OnEnable();
        _nextMatchButton.SetVisible(showMatchButton);
    }

    private void OnEnable()
    {
        Team team = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID());

        _homeTeamBarComponent.SetDetails(team, true);
        _homeRosterComponent.SetDetails(team, true);
        _homeTacticsComponent.SetDetails(team, true);
        _homeCoachesComponent.SetDetails(StaffSystem.Instance.GetAllStaff(), true);
        _homeStandingsComponent.SetDetails(LeagueSystem.Instance.GetTeams(), true);
        _homeTradeOffersComponent.SetDetails(team, true);
        _homeTradeBlockComponent.SetDetails(team, true);
        _homeTradeFinderComponent.SetDetails(team, true);
        _homeBudgetComponent.SetDetails(team, true);
        _homeFacilitiesComponent.SetDetails(team, true);
        _nextMatchButton.SetDetails(team);
    }
}
