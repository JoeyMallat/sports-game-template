using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeagueSystem : MonoBehaviour
{
    public static LeagueSystem Instance;

    [SerializeField] List<Team> _teams;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(this); }

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

        DistributeDraftPicks();
    }

    public Team GetTeam(int id)
    {
        return _teams.Where(x => x.GetTeamID() == id).ToList()[0];
    }

    public List<Team> GetTeams()
    {
        return _teams;
    }

    private void DistributeDraftPicks()
    {
        // TODO: Base off season standings and use weighted lottery if enabled

        // Temporary solution
        int index = 1;
        for (int round = 1; round < ConfigManager.Instance.GetCurrentConfig().DraftRounds + 1; round++)
        {
            foreach (Team team in _teams)
            {
                team.AddDraftPick(new DraftPick(round, index));

                // Don't add to index when it's the last pick of the round
                if ((index == 30 && round % 2 != 0) || (index == 1 && round % 2 == 0)) break;

                // Add to index
                if (round % 2 == 0) index--;
                else index++;
            }
        }

    }
}
