using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Main config", menuName = "Config/Main config")]
public class Config : ScriptableObject
{
    public SportsType SportsType;
    public TextAsset LeagueFile;
    public int AverageSquadSize;
    public PositionConfig PositionConfig;
    [SerializeReference] public ScheduleGenerator ScheduleGenerator;
}
