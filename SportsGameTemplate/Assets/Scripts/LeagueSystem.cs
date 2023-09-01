using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeagueSystem : MonoBehaviour
{
    [SerializeField] List<Team> _teams;

    private void Awake()
    {
        ReadTeamsFromFile();
    }

    private void ReadTeamsFromFile()
    {
        _teams = new List<Team>();

        TextAsset teamFile = ConfigManager.Instance.GetCurrentConfig().LeagueFile;
        SquadCreator squadCreator = new SquadCreator();

        string[] rows = teamFile.text.Split('\n');

        int id = 0;
        foreach (string row in rows)
        {
            string[] teamData = row.Split(',');
            string teamName = teamData[0];
            int.TryParse(teamData[1], out int rating);

            List<Player> players = squadCreator.CreateSquad(rating);
            Team newTeam = new Team(id, teamName, rating, players);
            _teams.Add(newTeam);

            id++;
        }
    }

    public List<Team> GetTeams()
    {
        return _teams;
    }
}
