using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Sirenix.Serialization;
using UnityEngine;

public class LocalSaveManager : MonoBehaviour
{
    [SerializeField] string _filePath;

    public static event Action<SeasonStage, int> OnGameLoaded;

    public void SaveGame()
    {
        try
        {
            Notification.Instance.ShowNotification("Game was succesfully saved!", NotificationType.Success, 2);

            LocalSaveData localSaveData = new LocalSaveData();
            localSaveData.Teams = LeagueSystem.Instance.GetTeams();
            localSaveData.Matches = LeagueSystem.Instance.GetMatches();
            localSaveData.SeasonStage = GameManager.Instance.GetSeasonStage();
            localSaveData.CurrentSeason = GameManager.Instance.GetCurrentSeason();
            localSaveData.CurrentWeek = GameManager.Instance.GetCurrentWeek();
            localSaveData.TeamID = GameManager.Instance.GetTeamID();
            localSaveData.PlayoffRound = PlayoffSystem.Instance.GetPlayoffRound();
            localSaveData.PlayoffMatchups = PlayoffSystem.Instance.GetAllPlayoffsMatchups();
            localSaveData.ChecklistChecks = FindFirstObjectByType<ChecklistView>(FindObjectsInactive.Include).GetChecklist();
            localSaveData.TacticValues = FindFirstObjectByType<TacticsSettings>().GetValues();
            localSaveData.Staff = StaffSystem.Instance.GetAllStaff();

            byte[] bytes = SerializationUtility.SerializeValue(localSaveData, DataFormat.JSON);
            File.WriteAllBytes(_filePath, bytes);
            Debug.Log($"Data written to {_filePath}");

            string teamID = localSaveData.TeamID.ToString();
            File.WriteAllText(_filePath + "_preview", teamID);
            Debug.Log($"Save preview saved to {_filePath + "_preview"}");
        }
        catch
        {
            Debug.LogWarning("Data was not saved!");
            Notification.Instance.ShowNotification("Game was not saved. Restart the game and try again.", NotificationType.Warning, 2);
        }
    }

    public async Task LoadGame(string path)
    {
        if (File.Exists(path))
        {
            byte[] bytes = File.ReadAllBytes(path);
            LocalSaveData data = SerializationUtility.DeserializeValue<LocalSaveData>(bytes, DataFormat.JSON);
            LeagueSystem.Instance.SetTeams(data.Teams, data.NextMatchIndex, data.Matches);
            GameManager.Instance.SetLoadData(data.SeasonStage, data.CurrentSeason, data.CurrentWeek, data.TeamID);
            PlayoffSystem.Instance.SetLoadData(data.PlayoffRound, data.PlayoffMatchups);
            LeagueSystem.Instance.GetNextGame(data.SeasonStage, data.CurrentWeek);
            FindFirstObjectByType<ChecklistView>().SetChecklist(data.ChecklistChecks);
            FindFirstObjectByType<TacticsSettings>().SetDropdownValuesAfterLoading(data.TacticValues);
            if (data.Staff != null) StaffSystem.Instance.SetStaffFromLoad(data.Staff.Coach, data.Staff.Scout, data.Staff.Mascot, data.Staff.Negotiator);

            OnGameLoaded?.Invoke(GameManager.Instance.GetSeasonStage(), GameManager.Instance.GetCurrentWeek());
        }
    }

    public Team LoadTeamDetails(string path)
    {
        if (File.Exists(path) && File.Exists(_filePath + "_preview"))
        {
            string teamIDText = File.ReadAllText(_filePath + "_preview");
            int.TryParse(teamIDText, out int teamID);
            return LeagueSystem.Instance.GetTeam(teamID);
        }

        return null;
    }

    public void SetFilePath(string filePath)
    {
        _filePath = filePath;
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
    public List<bool> ChecklistChecks;
    public List<int> TacticValues;
    public AllStaff Staff;
}
