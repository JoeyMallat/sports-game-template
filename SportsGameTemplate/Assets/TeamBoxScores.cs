using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;
using System.Linq;

public class TeamBoxScores : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _homeBoxRoot;
    [SerializeField] GameObject _awayBoxRoot;
    [SerializeField] StatObject _statObjectPrefab;

    [SerializeField] Image _homeTeamLogo;
    [SerializeField] Image _awayTeamLogo;

    [SerializeField] TextMeshProUGUI _homeTeamScore;
    [SerializeField] TextMeshProUGUI _awayTeamScore;

    [SerializeField] TextMeshProUGUI _homeTeamName;
    [SerializeField] TextMeshProUGUI _homeTeamSmall;

    [SerializeField] TextMeshProUGUI _awayTeamName;
    [SerializeField] TextMeshProUGUI _awayTeamSmall;

    [SerializeField] Button _backToMenuButton;

    public void SetDetails<T>(T item) where T : class
    {
        Match match = item as Match;

        Team homeTeam = LeagueSystem.Instance.GetTeam(match.GetHomeTeamID());
        Team awayTeam = LeagueSystem.Instance.GetTeam(match.GetAwayTeamID());

        _homeTeamScore.text = match.GetScore().Item1.ToString();
        _awayTeamScore.text = match.GetScore().Item2.ToString();

        _homeTeamLogo.sprite = homeTeam.GetTeamLogo();
        _awayTeamLogo.sprite = awayTeam.GetTeamLogo();
        _homeTeamName.text = homeTeam.GetTeamName();
        _awayTeamName.text = awayTeam.GetTeamName();
        _homeTeamSmall.text = homeTeam.GetTeamName();
        _awayTeamSmall.text = awayTeam.GetTeamName();

        SetPlayerBoxScores(match, homeTeam, _homeBoxRoot);
        SetPlayerBoxScores(match, awayTeam, _awayBoxRoot);

        _backToMenuButton.onClick.RemoveAllListeners();
        _backToMenuButton.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()))));
    }

    private void SetPlayerBoxScores(Match match, Team team, GameObject boxRoot)
    {
        int matchID = match.GetMatchID();

        List<Player> playersWhoPlayed = team.GetPlayersFromTeam().Where(x => x.GetLatestSeason().GetMatchStats().Count > 0).Where(xx => xx.GetLatestSeason().GetMatchStats().Last().GetMatchID() == matchID).ToList();
        playersWhoPlayed = playersWhoPlayed.OrderByDescending(x => x.GetLatestSeason().GetMatchStats().Last().GetTotal("minutes")).ToList();
        List<StatObject> statObjects = boxRoot.GetComponentsInChildren<StatObject>().ToList();

        int toBeSpawned = Mathf.Clamp(playersWhoPlayed.Count - statObjects.Count, 0, 999);

        for (int i = 0; i < toBeSpawned; i++)
        {
            StatObject statObject = Instantiate(_statObjectPrefab, boxRoot.transform, false);
            statObjects.Add(statObject);
        }

        for (int i = 0;i < statObjects.Count;i++)
        {
            if (i < playersWhoPlayed.Count)
            {
                int index = i;
                PlayerMatchStats stats = playersWhoPlayed[index].GetLatestSeason().GetMatchStats().Last();
                statObjects[i].gameObject.SetActive(true);
                statObjects[i].SetDetails(new StatObjectWrapper($"{playersWhoPlayed[index].GetFullName()} <color=#FF9900>{playersWhoPlayed[index].GetPosition()}", new List<float>() { stats.GetTotal("minutes"), stats.GetPoints(), stats.GetTotal("assists"), stats.GetTotal("rebounds") }, playersWhoPlayed[index]));
            } else
            {
                statObjects[i].gameObject.SetActive(false);
            }
        }
    }
}
