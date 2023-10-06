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
    [SerializeField] Vector2 _passSuccessRate;
    [SerializeField] Vector2 _twoPointSuccessRate;
    [SerializeField] Vector2 _threePointSuccessRate;
    [SerializeField] Vector2 _dunkSuccessRate;
    [SerializeField] Vector2 _layupSuccessRate;
    [SerializeField] Vector2 _dribbleSuccessRate;
    [SerializeField] Vector2 _blockSuccessRate;
    [SerializeField] Vector2 _stealSuccessRate;

    [SerializeField] List<NextMove> _nextMoves;

    public void SimulateMatch(Match match)
    {
        Team homeTeam = LeagueSystem.Instance.GetTeam(match.GetHomeTeamID());
        Team awayTeam = LeagueSystem.Instance.GetTeam(match.GetAwayTeamID());

        Team teamInPossession = homeTeam;

        for (int i = 1; i < _numberOfPeriods + 1; i++)
        {
            int secondsLeftInPeriod = _secondsPerPeriod;

            while (secondsLeftInPeriod > .1)
            {
                (int, PossessionResult) possession = SimulatePossession(match.GetMatchID(), homeTeam, awayTeam, _shotClock);
                Team teamNotInPossession = teamInPossession == homeTeam ? awayTeam : homeTeam;
                secondsLeftInPeriod -= possession.Item1;
                teamInPossession = DecideNextPossession(possession.Item2, teamInPossession, teamNotInPossession);

                match.AddPossessionResult(possession.Item2, teamInPossession);
            }
        }
        match.EndMatch();
    }

    private Team DecideNextPossession(PossessionResult possession, Team currentTeamInPossession, Team otherTeam)
    {
        switch (possession.GetPossessionResult())
        {
            case ResultAction.TwoPointerMissed:
                return DecideReboundingTeam(currentTeamInPossession, otherTeam);
            case ResultAction.ThreePointerMissed:
                return DecideReboundingTeam(currentTeamInPossession, otherTeam);
            case ResultAction.FreeThrowMissed:
                return DecideReboundingTeam(currentTeamInPossession, otherTeam);
            default:
                return otherTeam;
        }
    }

    private Team DecideReboundingTeam(Team teamOne, Team teamTwo)
    {
        // TODO: Add the lineup of the team
        Player teamOneRebounder = teamOne.GetPlayersFromTeam().OrderByDescending(x => x.GetSkills()[11].GetRatingForSkill() * UnityEngine.Random.Range(0.9f, 1.1f)).ToList()[0];
        Player teamTwoRebounder = teamTwo.GetPlayersFromTeam().OrderByDescending(x => x.GetSkills()[11].GetRatingForSkill() * UnityEngine.Random.Range(0.9f, 1.1f)).ToList()[0];

        float teamOneRebounderChance = teamOneRebounder.GetSkills()[11].GetRatingForSkill();
        float total = teamOneRebounder.GetSkills()[11].GetRatingForSkill() + teamTwoRebounder.GetSkills()[11].GetRatingForSkill();

        if (UnityEngine.Random.Range(0f, total) > teamOneRebounderChance)
        {
            return teamTwo;
        } else
        {
            return teamOne;
        }
    }

    private (int, PossessionResult) SimulatePossession(int matchID, Team teamInPossession, Team defendingTeam, int maxSeconds)
    {
        int secondsSpent = 0;
        bool inPossession = true;
        ResultAction resultOfPossession = ResultAction.TwoPointerMissed;
        Player playerWithBall = teamInPossession.GetPlayersFromTeam()[0];

        while (inPossession)
        {
            // TODO: Add the lineup of the team
            List<Player> best = teamInPossession.GetPlayersFromTeam().Shuffle().Take(5).ToList();
            playerWithBall = best[UnityEngine.Random.Range(0, 5)];
            Move move = DecideNextMove(secondsSpent);
            secondsSpent += UnityEngine.Random.Range(2, 5);
            DecideResult(playerWithBall, move, out bool possessionKept, out ResultAction resultAction);

            AssignActionToPlayer(matchID, resultAction, playerWithBall);

            resultOfPossession = resultAction;
            inPossession = possessionKept;

            if (secondsSpent >= maxSeconds) inPossession = false;
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

    private (bool, ResultAction) GetResult(Vector2 averageSucceedingChance, int amountOfSkills, float skillRating, bool keepPossessionWhenSucceeded, ResultAction actionSucceeded, ResultAction actionFailed)
    {
        float averageSkillRating = skillRating / (amountOfSkills * 99f);
        float chanceOfSucceeding = Mathf.Lerp(averageSucceedingChance.x, averageSucceedingChance.y, averageSkillRating);
        float random = UnityEngine.Random.Range(0f, 1f);

        if (random < chanceOfSucceeding)
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

    public void AssignActionToPlayer(int matchID, ResultAction resultAction, Player player)
    {
        switch (resultAction)
        {
            case ResultAction.Pass:
                player.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)>() { ("assists", 1 ) });
                break;
            case ResultAction.Rebound:
                player.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)>() { ("rebounds", 1) });
                break;
            case ResultAction.Steal:
                player.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)>() { ("steals", 1) });
                break;
            case ResultAction.Block:
                player.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)>() { ("rebounds", 1) });
                break;
            case ResultAction.TwoPointerMade:
                player.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)>() { ("twoPointersMade", 1), ("twoPointersAttempted", 1), ("twoPointersPoints", 2) });
                break;
            case ResultAction.TwoPointerMissed:
                player.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)>() { ("twoPointersAttempted", 1) });
                break;
            case ResultAction.ThreePointerMade:
                player.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)>() { ("threePointersMade", 1), ("threePointersAttempted", 1), ("threePointersPoints", 3) });
                break;
            case ResultAction.ThreePointerMissed:
                player.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)>() { ("threePointersAttempted", 1) });
                break;
            case ResultAction.FreeThrowMade:
                player.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)>() { ("freeThrowsMade", 1), ("freeThrowsAttempted", 1) });
                break;
            case ResultAction.FreeThrowMissed:
                player.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)>() { ("freeThrowsAttempted", 1) });
                break;
            default:
                break;
        }
    }
}
