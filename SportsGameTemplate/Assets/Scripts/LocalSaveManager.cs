using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LocalSaveManager : MonoBehaviour
{
    [SerializeField] string _filePath;

    private void Awake()
    {
        GameManager.OnAdvance += SaveGame;
    }

    private void Start()
    {
        //LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame(GameManager.Instance.GetSeasonStage(), GameManager.Instance.GetCurrentWeek());
    }

    public void SaveGame(SeasonStage seasonStage, int week)
    {
        try
        {
            LocalSaveData localSaveData = new LocalSaveData();
            localSaveData.Teams = LeagueSystem.Instance.GetTeams();
            localSaveData.Matches = LeagueSystem.Instance.GetMatches();
            localSaveData.SeasonStage = seasonStage;
            localSaveData.CurrentSeason = GameManager.Instance.GetCurrentSeason();
            localSaveData.CurrentWeek = week;
            localSaveData.TeamID = GameManager.Instance.GetTeamID();
            localSaveData.PlayoffRound = PlayoffSystem.Instance.GetPlayoffRound();
            localSaveData.PlayoffMatchups = PlayoffSystem.Instance.GetAllPlayoffsMatchups();

            byte[] bytes = SerializationUtility.SerializeValue(localSaveData, DataFormat.JSON);
            File.WriteAllBytes(Application.persistentDataPath + _filePath, bytes);
        } catch
        {
            Debug.LogWarning("Data was not saved!");
        }

    }

    public void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + _filePath))
        {
            byte[] bytes = File.ReadAllBytes(Application.persistentDataPath + _filePath);
            LocalSaveData data = SerializationUtility.DeserializeValue<LocalSaveData>(bytes, DataFormat.JSON);
            LeagueSystem.Instance.SetTeams(data.Teams, data.NextMatchIndex, data.Matches);
            GameManager.Instance.SetLoadData(data.SeasonStage, data.CurrentSeason, data.CurrentWeek, data.TeamID);
            PlayoffSystem.Instance.SetLoadData(data.PlayoffRound, data.PlayoffMatchups);

            Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(data.TeamID));
        }
    }
}

[System.Serializable]
public class LocalSaveData
{
    public int TeamID;
    public SeasonStage SeasonStage;
    public int CurrentWeek;
    public int CurrentSeason;
    public int NextMatchIndex;
    public List<Team> Teams;
    public List<Match> Matches;
    public int PlayoffRound;
    public List<PlayoffMatchup> PlayoffMatchups;
}
