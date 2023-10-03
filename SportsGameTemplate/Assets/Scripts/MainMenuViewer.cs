using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuViewer : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _teamTab;
    [SerializeField] GameObject _tradeTab;
    [SerializeField] GameObject _officeTab;
    [SerializeField] GameObject _leagueTab;

    public void SetDetails<T>(T item) where T : class
    {
        ISettable _teamSettable = _teamTab.GetComponent<ISettable>();
        _teamSettable.SetDetails((item as Team).GetPlayersFromTeam());

        ISettable _tradeSettable = _tradeTab.GetComponent<ISettable>();
        _tradeSettable.SetDetails(item as Team);
        //ISettable _officeSettable = _officeTab.GetComponent<ISettable>();
        
        ISettable _leagueSettable = _leagueTab.GetComponent<ISettable>();
        _leagueSettable.SetDetails(LeagueSystem.Instance.GetTeams());
    }
}
