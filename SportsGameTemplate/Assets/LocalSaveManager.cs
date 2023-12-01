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

    public void SaveGame(SeasonStage seasonStage, int week)
    {
        byte[] bytes = SerializationUtility.SerializeValue(LeagueSystem.Instance.GetTeams(), DataFormat.JSON);
        File.WriteAllBytes(Application.persistentDataPath + _filePath, bytes);
    }

    public void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + _filePath))
        {
            byte[] bytes = File.ReadAllBytes(Application.persistentDataPath + _filePath);
            List<Team> teams = SerializationUtility.DeserializeValue<List<Team>>(bytes, DataFormat.JSON);
            LeagueSystem.Instance.SetTeams(teams);
        }
    }
}
