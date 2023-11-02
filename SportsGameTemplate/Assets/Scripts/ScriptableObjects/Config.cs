using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Main config", menuName = "Config/Main config")]
public class Config : ScriptableObject
{
    public SportsType SportsType;

    public PositionConfig PositionConfig;
    [SerializeReference] public ScheduleGenerator ScheduleGenerator;
    [SerializeReference] public MatchSimulator MatchSimulator;

    [Header("League & World Settings")]
    public int AverageSquadSize;
    public int AverageAgeInLeague;
    public TextAsset LeagueFile;
    public int GamesPerTeamInRegularSeason;
    public int SalaryCap;
    public int MinimumSalary;
    public int BestOfAmountInPlayoffs;

    [Header("Draft Settings")]
    public bool WeightedDraftLottery;
    public int DraftRounds;
    public int PlayersPerDraftRound;
    public AnimationCurve RookieSalaryScale;

    [Header("Calculations")]
    public AnimationCurve ContractLengthImpactOnTradeValue;
    public AnimationCurve DistributionOfPotential;

    public int GetTotalDraftPicks()
    {
        return DraftRounds * PlayersPerDraftRound;
    }
}
