using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;

public class BasketballMatchSimulator : MatchSimulator
{
    [Header("Match settings")]
    const int _numberOfPeriods = 4;
    const int _secondsPerPeriod = 720;
    const int _shotClock = 24;
    const bool _overTimePossible = true;

    [Header("Match simulation settings")]
    [SerializeField] float _scoringRate;
    [SerializeField] float _passSuccessRate;
    [SerializeField] float _twoPointSuccessRate;
    [SerializeField] float _threePointSuccessRate;
    [SerializeField] float _dunkSuccessRate;
    [SerializeField] float _layupSuccessRate;
    [SerializeField] float _dribbleSuccessRate;

    [SerializeField] List<NextMove> _nextMoves;

    public void SimulateMatch(Match match)
    {
        Team homeTeam = LeagueSystem.Instance.GetTeam(match.GetHomeTeamID());
        Team awayTeam = LeagueSystem.Instance.GetTeam(match.GetAwayTeamID());

        for (int i = 1; i < _numberOfPeriods + 1; i++)
        {
            int secondsLeftInPeriod = _secondsPerPeriod;

            while (secondsLeftInPeriod > .1)
            {
                (int, PossessionResult) possession = SimulatePossession(homeTeam, awayTeam, _shotClock);
                Debug.Log($"Period {i} - {secondsLeftInPeriod} seconds left: {possession.Item2.GetPlayer().GetFullName()} with {possession.Item2.GetPossessionResult()}");
                secondsLeftInPeriod -= possession.Item1;
                match.AddPossessionResult(possession.Item2);
            }

            match.EndMatch();
        }
    }

    private (int, PossessionResult) SimulatePossession(Team teamInPossession, Team defendingTeam, int maxSeconds)
    {
        int secondsSpent = 0;
        bool inPossession = true;
        ResultAction resultOfPossession = ResultAction.TwoPointerMissed;
        Player playerWithBall = teamInPossession.GetPlayersFromTeam()[0];

        while (inPossession)
        {
            // TODO: Add the lineup of the team
            List<Player> best = teamInPossession.GetPlayersFromTeam().OrderByDescending(x => x.GetSkills()[3].GetRatingForSkill()).Take(5).ToList();
            playerWithBall = best[UnityEngine.Random.Range(0, 5)];
            Move move = DecideNextMove(secondsSpent);
            secondsSpent += UnityEngine.Random.Range(2, 5);
            DecideResult(playerWithBall, move, out bool possessionKept, out ResultAction resultAction);

            resultOfPossession = resultAction;
            inPossession = possessionKept;
        } 

        PossessionResult result = new PossessionResult(playerWithBall, resultOfPossession);

        return (secondsSpent, result);
    }

    private void DecideResult(Player playerWithBall, Move move, out bool possessionKept, out ResultAction resultAction)
    {
        possessionKept = true;
        resultAction = ResultAction.TwoPointerMissed;
        switch (move)
        {
            case Move.Pass:
                float passingSkill1 = playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Passing).ToList()[0].GetRatingForSkill();
                float passingSkill2 = playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Playmaking).ToList()[0].GetRatingForSkill();
                (possessionKept, resultAction) = GetResult(_passSuccessRate, 2, passingSkill1 + passingSkill2, true, ResultAction.Pass, ResultAction.Steal);
                break;
            case Move.TwoPointer:
                float twoPointerSkill1 = playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.ShortRangeShooting).ToList()[0].GetRatingForSkill();
                float twoPointerSkill2 = playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.MidRangeShooting).ToList()[0].GetRatingForSkill();
                (possessionKept, resultAction) = GetResult(_twoPointSuccessRate, 2, twoPointerSkill1 + twoPointerSkill2, false, ResultAction.TwoPointerMade, ResultAction.TwoPointerMissed);
                break;
            case Move.ThreePointer:
                float threePointerSkill1 = playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.MidRangeShooting).ToList()[0].GetRatingForSkill();
                float threePointerSkill2 = playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.ThreePointShooting).ToList()[0].GetRatingForSkill();
                (possessionKept, resultAction) = GetResult(_threePointSuccessRate, 2, threePointerSkill1 + threePointerSkill2, false, ResultAction.ThreePointerMade, ResultAction.ThreePointerMissed);
                break;
            case Move.Dunk:
                float dunkSkill1 = playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Dunking).ToList()[0].GetRatingForSkill();
                (possessionKept, resultAction) = GetResult(_dunkSuccessRate, 1, dunkSkill1, false, ResultAction.TwoPointerMade, ResultAction.TwoPointerMissed);
                break;
            case Move.Layup:
                float layupSkill1 = playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Layups).ToList()[0].GetRatingForSkill();
                float layupSkill2 = playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Speed).ToList()[0].GetRatingForSkill();
                (possessionKept, resultAction) = GetResult(_layupSuccessRate, 2, layupSkill1 + layupSkill2, false, ResultAction.TwoPointerMade, ResultAction.TwoPointerMissed);
                break;
            case Move.Dribble:
                float dribbleSkill1 = playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Dribbling).ToList()[0].GetRatingForSkill();
                float dribbleSkill2 = playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Speed).ToList()[0].GetRatingForSkill();
                float dribbleSkill3 = playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Playmaking).ToList()[0].GetRatingForSkill();
                (possessionKept, resultAction) = GetResult(_dribbleSuccessRate, 3, dribbleSkill1 + dribbleSkill2 + dribbleSkill3, true, ResultAction.Dribble, ResultAction.Steal);
                break;
            default:
                break;
        }
    }

    private (bool, ResultAction) GetResult(float averageSucceedingChance, int amountOfSkills, float skillRating, bool keepPossessionWhenSucceeded, ResultAction actionSucceeded, ResultAction actionFailed)
    {
        // TODO: Normalize to average success chance
        float averageSkillRating = (skillRating / amountOfSkills) / 100f;
        float random = UnityEngine.Random.Range(0f, averageSkillRating / (averageSucceedingChance / UnityEngine.Random.Range(1f, 1.4f)));

        if (averageSkillRating * averageSucceedingChance >= random)
        {
            return (keepPossessionWhenSucceeded, actionSucceeded);
        }
        else
        {
            return (false, actionFailed);
        }
    }

    private Move DecideNextMove(int second)
    {
        float total = 0;
        foreach (var nextMove in _nextMoves)
        {
            total += nextMove.GetOddsOfMove(second);
        }

        float random = UnityEngine.Random.Range(0, total);

        foreach (var move in _nextMoves)
        {
            if (random < move.GetOddsOfMove(second))
            {
                return move.GetMove();
            } else
            {
                random -= move.GetOddsOfMove(second);
            }
        }

        return Move.TwoPointer;
        
    }
}
