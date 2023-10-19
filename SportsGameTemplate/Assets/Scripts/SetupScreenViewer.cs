using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SetupScreenViewer : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _teamDataObject;
    [SerializeField] GameObject _bestPlayersRoot;
    [SerializeField] TextMeshProUGUI _teamNameText;
    [SerializeField] TextMeshProUGUI _ratingText;
    [SerializeField] TextMeshProUGUI _salaryText;
    [SerializeField] TextMeshProUGUI _mediaExpectationText;
    [SerializeField] TextMeshProUGUI _draftPickOneText;
    [SerializeField] TextMeshProUGUI _draftPickTwoText;

    private void Awake()
    {
        TeamSelection.OnSelectedTeam += SetDetails;
        _teamDataObject.SetActive(false);
    }

    public void SetDetails<T>(T item) where T : class
    {
        _teamDataObject.SetActive(true);

        Team team = item as Team;

        int rating = team.GetAverageTeamRating();
        string salary = team.GetTotalSalaryAmount().ConvertToMonetaryString();
        string mediaExpectation = LeagueSystem.Instance.GetTeams().OrderByDescending(x => x.GetAverageTeamRating()).ToList().IndexOf(team).GetMediaExpectation();
        List<Player> topPlayers = team.GetPlayersFromTeam().OrderByDescending(x => x.CalculateRatingForPosition()).ToList();
        List<DraftPick> draftPicks = team.GetDraftPicks();
        List<PlayerItem> playerItems = _bestPlayersRoot.GetComponentsInChildren<PlayerItem>().ToList();

        _teamNameText.text = team.GetTeamName();
        _ratingText.text = $"Average team rating  <color=\"white\">{rating} OVR";
        _salaryText.text = $"Current salary  <color=\"white\">{salary} / {ConfigManager.Instance.GetCurrentConfig().SalaryCap.ConvertToMonetaryString()}";
        _mediaExpectationText.text = $"Media expectation  <color=\"white\">{mediaExpectation}";

        _draftPickOneText.text = $"Round {draftPicks[0].GetPickData().Item1}  <color=\"white\">Pick {draftPicks[0].GetPickData().Item2}";
        _draftPickTwoText.text = $"Round {draftPicks[1].GetPickData().Item1}  <color=\"white\">Pick {draftPicks[1].GetPickData().Item2}";

        for (int i = 0; i < playerItems.Count; i++)
        {
            int index = i;
            playerItems[i].SetPlayerDetails(topPlayers[index], false, false);
        }
    }
}