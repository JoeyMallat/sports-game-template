using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Sirenix.Serialization;
using UnityEngine;

public class LocalSaveManager : MonoBehaviour
{
    [SerializeField] string _filePath;
    DateTime _freeSpinTime = DateTime.MinValue;
    bool _isRestoring;

    public static event Action<SeasonStage, int> OnGameLoaded;
    public static event Action OnCareerAvailable;
    public static bool IsCareerActive { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetCareerState()
    {
        IsCareerActive = false;
    }

    void OnEnable()
    {
        DevelopmentPointsManager.OnDevelopmentPointsSaveRequested += SaveCareerDevelopmentPoints;
        GameManager.OnPremiumStatusUpdated += SavePlayerData;
        GameManager.OnSalaryCapIncreased += SavePlayerData;
        GameManager.OnInventoryUpdated += SavePlayerData;
        GameManager.OnGameStarted += BeginNewCareer;
        MM_OfficeView.OnFreeSpin += SetFreeSpinTime;
    }

    void OnDisable()
    {
        DevelopmentPointsManager.OnDevelopmentPointsSaveRequested -= SaveCareerDevelopmentPoints;
        GameManager.OnPremiumStatusUpdated -= SavePlayerData;
        GameManager.OnSalaryCapIncreased -= SavePlayerData;
        GameManager.OnInventoryUpdated -= SavePlayerData;
        GameManager.OnGameStarted -= BeginNewCareer;
        MM_OfficeView.OnFreeSpin -= SetFreeSpinTime;
    }

    void SaveCareerDevelopmentPoints(DevelopmentPointsSaveData data)
    {
        SaveActiveCareer();
    }

    void SavePlayerData(CloudSaveData data)
    {
        SaveActiveCareer();
    }

    void SaveActiveCareer()
    {
        if (!_isRestoring && IsCareerActive && !string.IsNullOrWhiteSpace(_filePath))
            SaveGame(false);
    }

    void BeginNewCareer(SeasonStage seasonStage, int week)
    {
        IsCareerActive = true;
        _freeSpinTime = DateTime.MinValue;
        _isRestoring = true;

        if (DevelopmentPointsManager.Instance != null)
        {
            DevelopmentPointsManager.Instance.RestoreSaveData(null);
            int reward = DevelopmentPointsManager.Instance.Settings != null ? DevelopmentPointsManager.Instance.Settings.WelcomeReward : 10;
            DevelopmentPointsManager.Instance.Award(reward, DevelopmentPointsSource.WelcomeGrant,
                "welcome:" + DevelopmentPointsManager.Instance.CareerId, "Welcome grant");
        }

        _isRestoring = false;
        OnCareerAvailable?.Invoke();
    }

    void SetFreeSpinTime(DateTime freeSpinTime)
    {
        _freeSpinTime = freeSpinTime;
        SaveActiveCareer();
    }

    public DateTime GetFreeSpinTime()
    {
        return _freeSpinTime;
    }

    public void SaveGame()
    {
        SaveGame(true);
    }

    public void SaveGame(bool showNotification)
    {
        try
        {
            if (showNotification)
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
            localSaveData.SalaryCapIncrease = GameManager.Instance.GetSalaryCapIncrease();
            localSaveData.PremiumStatus = GameManager.Instance.GetPremiumStatus();
            localSaveData.LegacyGemAmount = GameManager.Instance.GetLegacyGemBalance();
            localSaveData.FreeSpinTimeTicks = _freeSpinTime.Ticks;
            localSaveData.Inventory = new List<CloudInventoryItem>();
            foreach (OwnedGameItem item in GameManager.Instance.GetItems())
                localSaveData.Inventory.Add(new CloudInventoryItem(item));
            if (DevelopmentPointsManager.Instance != null)
            {
                localSaveData.DevelopmentPointsCareerId = DevelopmentPointsManager.Instance.CareerId;
                localSaveData.DevelopmentPoints = DevelopmentPointsManager.Instance.CaptureSaveData();
            }

            byte[] bytes = SerializationUtility.SerializeValue(localSaveData, DataFormat.JSON);
            File.WriteAllBytes(_filePath, bytes);
            Debug.Log($"Data written to {_filePath}");

            string teamID = localSaveData.TeamID.ToString();
            File.WriteAllText(_filePath + "_preview", teamID);
            Debug.Log($"Save preview saved to {_filePath + "_preview"}");
        }
        catch (Exception e)
        {
            Debug.LogWarning("Data was not saved! " + e);
            if (showNotification)
                Notification.Instance.ShowNotification("Game was not saved. Restart the game and try again.", NotificationType.Warning, 2);
        }
    }

    public async Task LoadGame(string path)
    {
        if (File.Exists(path))
        {
            _filePath = path;
            _isRestoring = true;
            byte[] bytes = File.ReadAllBytes(path);
            LocalSaveData data = SerializationUtility.DeserializeValue<LocalSaveData>(bytes, DataFormat.JSON);
            LeagueSystem.Instance.SetTeams(data.Teams, data.NextMatchIndex, data.Matches);
            GameManager.Instance.SetLoadData(data.SeasonStage, data.CurrentSeason, data.CurrentWeek, data.TeamID);
            PlayoffSystem.Instance.SetLoadData(data.PlayoffRound, data.PlayoffMatchups);
            LeagueSystem.Instance.GetNextGame(data.SeasonStage, data.CurrentWeek);
            FindFirstObjectByType<ChecklistView>().SetChecklist(data.ChecklistChecks);
            FindFirstObjectByType<TacticsSettings>().SetDropdownValuesAfterLoading(data.TacticValues);
            if (data.Staff != null) StaffSystem.Instance.SetStaffFromLoad(data.Staff.Coach, data.Staff.Scout, data.Staff.Mascot, data.Staff.Negotiator);
            GameManager.Instance.SetGems(data.LegacyGemAmount, false);
            GameManager.Instance.SetInventory(data.Inventory);
            GameManager.Instance.RestoreSalaryCapIncrease(data.SalaryCapIncrease);
            GameManager.Instance.SetPremiumStatus(data.PremiumStatus, false);
            _freeSpinTime = data.FreeSpinTimeTicks > 0 ? new DateTime(data.FreeSpinTimeTicks) : DateTime.MinValue;
            if (DevelopmentPointsManager.Instance != null)
            {
                DevelopmentPointsManager.Instance.RestoreCareerIdentity(data.DevelopmentPointsCareerId);
                DevelopmentPointsManager.Instance.RestoreSaveData(data.DevelopmentPoints, data.LegacyGemAmount);
            }

            IsCareerActive = true;
            _isRestoring = false;
            OnGameLoaded?.Invoke(GameManager.Instance.GetSeasonStage(), GameManager.Instance.GetCurrentWeek());
            OnCareerAvailable?.Invoke();
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
    public string DevelopmentPointsCareerId;
    public DevelopmentPointsSaveData DevelopmentPoints;
    public float SalaryCapIncrease;
    public bool PremiumStatus;
    public int LegacyGemAmount;
    public List<CloudInventoryItem> Inventory;
    public long FreeSpinTimeTicks;
}
