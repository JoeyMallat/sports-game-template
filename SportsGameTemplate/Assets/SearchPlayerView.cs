using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SearchPlayerView : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _statObjectRoot;
    [SerializeField] TextMeshProUGUI _noPlayersFoundText;

    [SerializeField] TMP_Dropdown _position;

    [SerializeField] TMP_Dropdown _overallMinimum;
    [SerializeField] TMP_Dropdown _overallMaximum;

    [SerializeField] TMP_Dropdown _ageMinimum;
    [SerializeField] TMP_Dropdown _ageMaximum;

    [SerializeField] TMP_Dropdown _statSelected1;
    [SerializeField] TMP_InputField _statValue1;

    [SerializeField] TMP_Dropdown _statSelected2;
    [SerializeField] TMP_InputField _statValue2;

    List<StatObject> _statObjects;

    public void SetDetails<T>(T item) where T : class
    {
        _statObjects = _statObjectRoot.GetComponentsInChildren<StatObject>().ToList();

        foreach (StatObject statObject in _statObjects)
        {
            statObject.gameObject.SetActive(false);
        }

        _overallMaximum.value = _overallMaximum.options.Count;
        _ageMaximum.value = _ageMaximum.options.Count;
        ToggleNoPlayersFound(true);

        SearchPlayers();
    }

    public void SearchPlayers()
    {
        bool filterPosition = CheckIfFilterOnPosition();
        bool filterOverall = CheckIfFilterOnNumber(_overallMinimum, _overallMaximum);
        bool filterAge = CheckIfFilterOnNumber(_ageMinimum, _ageMaximum);

        List<Player> players = new List<Player>();
        players.AddRange(LeagueSystem.Instance.GetAllPlayers());

        if (filterPosition)
            players = FilterPosition(players);

        if (filterOverall)
        {
            players = FilterOverall(players);
        }

        if (filterAge)
        {
            players = FilterAge(players);
        }

        players = FilterOnStats(players, _statSelected1, _statValue1);
        players = FilterOnStats(players, _statSelected2, _statValue2);

        DisplayPlayers(players);
    }

    private List<Player> FilterPosition(List<Player> players)
    {
        return players.Where(x => x.GetPosition() == _position.options[_position.value].text).ToList();
    }


    private List<Player> FilterOverall(List<Player> players)
    {
        return players.Where(x => x.CalculateRatingForPosition() >= int.Parse(_overallMinimum.options[_overallMinimum.value].text)
                    && x.CalculateRatingForPosition() <= int.Parse(_overallMaximum.options[_overallMaximum.value].text)).ToList();
    }


    private List<Player> FilterAge(List<Player> players)
    {
        return players.Where(x => x.GetAge() >= int.Parse(_ageMinimum.options[_ageMinimum.value].text)
                    && x.GetAge() <= int.Parse(_ageMaximum.options[_ageMaximum.value].text)).ToList();
    }

    private List<Player> FilterOnStats(List<Player> players, TMP_Dropdown stat, TMP_InputField input)
    {
        float.TryParse(input.text, out float result);

        switch (stat.options[stat.value].text)
        {
            case "points":
                return players.Where(x => x.GetLatestSeason().GetAveragePoints() >= result).ToList();
            default:
                return players.Where(x => x.GetLatestSeason().GetAverageOfStat(stat.options[stat.value].text) >= result).ToList();
        }
    }

    private void DisplayPlayers(List<Player> players)
    {
        ToggleNoPlayersFound(false);
        List<Player> playersShown = new List<Player>();

        players = players.OrderByDescending(x => x.CalculateRatingForPosition()).ToList();

        if (players.Count <= 20 && players.Count > 0)
        {
            playersShown = players;
        }
        else if (players.Count > 20)
        {
            playersShown = players.Take(20).ToList();
        }
        else
        {
            ToggleNoPlayersFound(true);
            foreach (StatObject statObject in _statObjects)
            {
                statObject.gameObject.SetActive(false);
            }
            return;
        }

        foreach (StatObject statObject in _statObjects)
        {
            statObject.gameObject.SetActive(false);
        }

        Debug.Log(playersShown.Count);
        int index = 0;
        foreach (Player player in playersShown)
        {
            _statObjects[index].gameObject.SetActive(true);
            _statObjects[index].SetDetails(new StatObjectWrapper($"{player.GetFullName()} <size=75%>({player.GetPosition()})", new List<float>() { player.CalculateRatingForPosition(), player.GetAge(), player.GetLatestSeason().GetAveragePoints(), player.GetLatestSeason().GetAverageOfStat("assists"), player.GetLatestSeason().GetAverageOfStat("rebounds") }, player));
            index++;
        }
    }

    private bool CheckIfFilterOnPosition()
    {
        return _position.value switch
        {
            0 => false,
            _ => true,
        };
    }

    private bool CheckIfFilterOnNumber(TMP_Dropdown minimum, TMP_Dropdown maximum)
    {
        if (minimum.value == 0 && maximum.value == 0)
        {
            return false;
        }

        return true;
    }

    private void ToggleNoPlayersFound(bool status)
    {
        _noPlayersFoundText.gameObject.SetActive(status);
    }
}
