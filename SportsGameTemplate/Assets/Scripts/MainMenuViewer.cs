using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuViewer : MonoBehaviour, ISettable
{
    [SerializeField] Image _teamLogo;

    [SerializeField] GameObject _teamTab;
    [SerializeField] GameObject _tradeTab;
    [SerializeField] GameObject _officeTab;
    [SerializeField] GameObject _leagueTab;
    [SerializeField] GameObject _playTab;

    public void SetDetails<T>(T item) where T : class
    {
        _teamLogo.sprite = (item as Team).GetTeamLogo();

        ISettable _teamSettable = _teamTab.GetComponent<ISettable>();
        _teamSettable.SetDetails((item as Team).GetPlayersFromTeam());

        ISettable _tradeSettable = _tradeTab.GetComponent<ISettable>();
        _tradeSettable.SetDetails(item as Team);

        //ISettable _officeSettable = _officeTab.GetComponent<ISettable>();
        
        ISettable _leagueSettable = _leagueTab.GetComponent<ISettable>();
        _leagueSettable.SetDetails(LeagueSystem.Instance.GetTeams());

        ISettable _playSettable = _playTab.GetComponent<ISettable>();
        _playSettable.SetDetails(item as Team);
    }
}
