using UnityEngine;

public class NewMainMenuViewer : MonoBehaviour
{
    [SerializeField] HomeRosterComponent _homeRosterComponent;

    private void OnEnable()
    {
        Team team = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID());
        _homeRosterComponent.SetDetails(team);
    }
}
