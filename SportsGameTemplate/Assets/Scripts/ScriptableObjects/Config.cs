using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Main config", menuName = "Config/Main config")]
public class Config : ScriptableObject
{
    public SportsType SportsType;

    public PositionConfig PositionConfig;
    [SerializeReference] public ScheduleGenerator ScheduleGenerator;

    [Header("League & World Settings")]
    public int AverageSquadSize;
    public int AverageAgeInLeague;
    public TextAsset LeagueFile;

    [Header("Draft Settings")]
    public bool WeightedDraftLottery;
    public int DraftRounds;
    public int PlayersPerDraftRound;
}
