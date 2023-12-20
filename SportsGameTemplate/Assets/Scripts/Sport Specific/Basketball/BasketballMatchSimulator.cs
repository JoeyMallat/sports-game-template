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

    int _teamToForceWin;
    float _secondsPlayed;

    [SerializeField] List<NextMove> _nextMoves;

    public void SimulateMatch(Match match, int teamToForceWin)
    {
        Team homeTeam = LeagueSystem.Instance.GetTeam(match.GetHomeTeamID());
        Team awayTeam = LeagueSystem.Instance.GetTeam(match.GetAwayTeamID());

        _teamToForceWin = teamToForceWin;

        Team teamInPossession = homeTeam;
        Team teamNotInPossession = teamInPossession == homeTeam ? awayTeam : homeTeam;

        for (int i = 1; i < _numberOfPeriods + 1; i++)
        {
            SetLineupAndMinutes(homeTeam, match.GetMatchID(), 2);
            SetLineupAndMinutes(awayTeam, match.GetMatchID(), 2);

            int secondsLeftInPeriod = _secondsPerPeriod;
            _secondsPlayed = 0;

            while (secondsLeftInPeriod > .1)
            {
                (int, PossessionResult) possession = SimulatePossession(match.GetMatchID(), teamInPossession, teamNotInPossession, _shotClock);
                secondsLeftInPeriod -= possession.Item1;
                _secondsPlayed += possession.Item1;
                match.AddPossessionResult(possession.Item2, teamInPossession);

                teamInPossession = DecideNextPossession(match.GetMatchID(), possession.Item2, teamInPossession, teamNotInPossession);
                teamNotInPossession = teamInPossession == homeTeam ? awayTeam : homeTeam;

                if (_secondsPlayed >= 120)
                {
                    SetLineupAndMinutes(homeTeam, match.GetMatchID(), 2);
                    SetLineupAndMinutes(awayTeam, match.GetMatchID(), 2);
                    _secondsPlayed = 0;
                }
            }
        }
        match.EndMatch();
    }

    private Team DecideNextPossession(int matchID, PossessionResult possession, Team currentTeamInPossession, Team otherTeam)
    {
        switch (possession.GetPossessionResult())
        {
            case ResultAction.TwoPointerMissed:
                return DecideReboundingTeam(matchID, currentTeamInPossession, otherTeam);
            case ResultAction.ThreePointerMissed:
                return DecideReboundingTeam(matchID, currentTeamInPossession, otherTeam);
            case ResultAction.FreeThrowMissed:
                return DecideReboundingTeam(matchID, currentTeamInPossession, otherTeam);
            default:
                return otherTeam;
        }
    }

    private Team DecideReboundingTeam(int matchID, Team teamOne, Team teamTwo)
    {
        Player teamOneRebounder = GetCurrentLineup(teamOne.GetLineup(), teamOne.GetTeamID()).OrderByDescending(x => x.GetRatingForSkillWithItem(x.GetSkills()[11]) * UnityEngine.Random.Range(0.5f, 1.5f)).ToList()[0];
        Player teamTwoRebounder = GetCurrentLineup(teamTwo.GetLineup(), teamTwo.GetTeamID()).OrderByDescending(x => x.GetRatingForSkillWithItem(x.GetSkills()[11]) * UnityEngine.Random.Range(0.5f, 1.5f)).ToList()[0];

        float teamOneRebounderChance = teamOneRebounder.GetRatingForSkillWithItem(teamOneRebounder.GetSkills()[11]);
        float total = teamOneRebounder.GetRatingForSkillWithItem(teamOneRebounder.GetSkills()[11]) + teamTwoRebounder.GetRatingForSkillWithItem(teamOneRebounder.GetSkills()[11]);

        if (UnityEngine.Random.Range(0f, total) > teamOneRebounderChance)
        {
            AssignActionToPlayer(matchID, ResultAction.Rebound, teamTwoRebounder);
            return teamTwo;
        } else
        {
            AssignActionToPlayer(matchID, ResultAction.Rebound, teamOneRebounder);
            return teamOne;
        }
    }

    private (int, PossessionResult) SimulatePossession(int matchID, Team teamInPossession, Team defendingTeam, int maxSeconds)
    {
        //Debug.Log($"Team in possession: {teamInPossession.GetTeamName()}");
        int secondsSpent = 0;
        bool inPossession = true;
        ResultAction resultOfPossession = ResultAction.TwoPointerMissed;
        Player playerWithBall = teamInPossession.GetPlayersFromTeam()[0];

        while (inPossession)
        {
            List<Player> best = GetCurrentLineup(teamInPossession.GetLineup(), teamInPossession.GetTeamID());
            playerWithBall = best[UnityEngine.Random.Range(0, 5)];
            Move move = DecideNextMove(secondsSpent);
            (ResultAction, Player) turnover = DecideDefence(move, defendingTeam);
            secondsSpent += UnityEngine.Random.Range(2, 6);

            if (turnover.Item1 == ResultAction.Steal || turnover.Item1 == ResultAction.Block)
            {
                ResultAction resultAction = turnover.Item1;
                AssignActionToPlayer(matchID, resultAction, turnover.Item2);
                playerWithBall = turnover.Item2;
                resultOfPossession = resultAction;
                inPossession = false;
            } else
            {
                DecideResult(playerWithBall, move, out bool possessionKept, out ResultAction resultAction);
                AssignActionToPlayer(matchID, resultAction, playerWithBall);
                resultOfPossession = resultAction;
                inPossession = possessionKept;
            }

            if (secondsSpent >= maxSeconds) inPossession = false;
        } 

        PossessionResult result = new PossessionResult(playerWithBall, resultOfPossession);

        return (secondsSpent, result);
    }

    private (ResultAction, Player) DecideDefence(Move move, Team defendingTeam)
    {
        List<Player> defendingLineup = GetCurrentLineup(defendingTeam.GetLineup(), defendingTeam.GetTeamID());

        switch (move)
        {
            case Move.Pass:
                Player player = defendingLineup.OrderByDescending(x => x.GetRatingForSkillWithItem(x.GetSkills()[6]) * UnityEngine.Random.Range(0.5f, 1.5f)).ToList()[0];
                (bool, ResultAction) result = GetResult(_stealSuccessRate, 1, player.GetRatingForSkillWithItem(player.GetSkills()[6]), true, ResultAction.Steal, ResultAction.Pass);
                return (result.Item2, player);
            case Move.TwoPointer:
                player = defendingLineup.OrderByDescending(x => x.GetRatingForSkillWithItem(x.GetSkills()[12]) * UnityEngine.Random.Range(0.5f, 1.5f)).ToList()[0];
                result = GetResult(_blockSuccessRate, 1, player.GetRatingForSkillWithItem(player.GetSkills()[12]), true, ResultAction.Steal, ResultAction.Pass);
                return (result.Item2, player);
            case Move.ThreePointer:
                player = defendingLineup.OrderByDescending(x => x.GetRatingForSkillWithItem(x.GetSkills()[12]) * UnityEngine.Random.Range(0.5f, 1.5f)).ToList()[0];
                result = GetResult(_blockSuccessRate, 1, player.GetRatingForSkillWithItem(player.GetSkills()[12]), true, ResultAction.Steal, ResultAction.Pass);
                return (result.Item2, player);
            case Move.Dunk:
                player = defendingLineup.OrderByDescending(x => x.GetRatingForSkillWithItem(x.GetSkills()[12]) * UnityEngine.Random.Range(0.5f, 1.5f)).ToList()[0];
                result = GetResult(_blockSuccessRate, 1, player.GetRatingForSkillWithItem(player.GetSkills()[12]), true, ResultAction.Steal, ResultAction.Pass);
                return (result.Item2, player);
            case Move.Layup:
                player = defendingLineup.OrderByDescending(x => x.GetRatingForSkillWithItem(x.GetSkills()[12]) * UnityEngine.Random.Range(0.5f, 1.5f)).ToList()[0];
                result = GetResult(_blockSuccessRate, 1, player.GetRatingForSkillWithItem(player.GetSkills()[12]), true, ResultAction.Steal, ResultAction.Pass);
                return (result.Item2, player);
            case Move.Dribble:
                player = defendingLineup.OrderByDescending(x => x.GetRatingForSkillWithItem(x.GetSkills()[6]) * UnityEngine.Random.Range(0.5f, 1.5f)).ToList()[0];
                result = GetResult(_stealSuccessRate, 1, player.GetRatingForSkillWithItem(player.GetSkills()[6]), true, ResultAction.Steal, ResultAction.Pass);
                return (result.Item2, player);
            default:
                player = defendingLineup.OrderByDescending(x => x.GetRatingForSkillWithItem(x.GetSkills()[12]) * UnityEngine.Random.Range(0.5f, 1.5f)).ToList()[0];
                result = GetResult(_blockSuccessRate, 1, player.GetRatingForSkillWithItem(player.GetSkills()[12]), true, ResultAction.Steal, ResultAction.Pass);
                return (result.Item2, player);
        }
    }

    private void DecideResult(Player playerWithBall, Move move, out bool possessionKept, out ResultAction resultAction)
    {
        possessionKept = true;
        resultAction = ResultAction.TwoPointerMissed;

        //Debug.Log($"Player from {LeagueSystem.Instance.GetTeam(playerWithBall.GetTeamID()).GetTeamName()} has attempted a {move}");
        switch (move)
        {
            case Move.Pass:
                float passingSkill1 = playerWithBall.GetRatingForSkillWithItem(playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Passing).ToList()[0]);
                float passingSkill2 = playerWithBall.GetRatingForSkillWithItem(playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Playmaking).ToList()[0]);
                if (playerWithBall.GetTeamID() == _teamToForceWin)
                {
                    (possessionKept, resultAction) = GetResult(_passSuccessRate, 2, 99 + 99, true, ResultAction.Pass, ResultAction.Steal);
                    break;
                }

                (possessionKept, resultAction) = GetResult(_passSuccessRate, 2, passingSkill1 + passingSkill2, true, ResultAction.Pass, ResultAction.Steal);

                break;
            case Move.TwoPointer:
                float twoPointerSkill1 = playerWithBall.GetRatingForSkillWithItem(playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Short_Range_Shooting).ToList()[0]);
                float twoPointerSkill2 = playerWithBall.GetRatingForSkillWithItem(playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Mid_Range_Shooting).ToList()[0]);
                if (playerWithBall.GetTeamID() == _teamToForceWin)
                {
                    (possessionKept, resultAction) = GetResult(_twoPointSuccessRate, 2, 99 + 99, false, ResultAction.TwoPointerMade, ResultAction.TwoPointerMissed);
                    break;
                }

                (possessionKept, resultAction) = GetResult(_twoPointSuccessRate, 2, twoPointerSkill1 + twoPointerSkill2, false, ResultAction.TwoPointerMade, ResultAction.TwoPointerMissed);

                break;
            case Move.ThreePointer:
                float threePointerSkill1 = playerWithBall.GetRatingForSkillWithItem(playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Mid_Range_Shooting).ToList()[0]);
                float threePointerSkill2 = playerWithBall.GetRatingForSkillWithItem(playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Three_Point_Shooting).ToList()[0]);
                if (playerWithBall.GetTeamID() == _teamToForceWin)
                {
                    (possessionKept, resultAction) = GetResult(_threePointSuccessRate, 2, 99 + 99, false, ResultAction.ThreePointerMade, ResultAction.ThreePointerMissed);
                    break;
                }

                (possessionKept, resultAction) = GetResult(_threePointSuccessRate, 2, threePointerSkill1 + threePointerSkill2, false, ResultAction.ThreePointerMade, ResultAction.ThreePointerMissed);

                break;
            case Move.Dunk:
                float dunkSkill1 = playerWithBall.GetRatingForSkillWithItem(playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Dunking).ToList()[0]);
                if (playerWithBall.GetTeamID() == _teamToForceWin)
                {
                    (possessionKept, resultAction) = GetResult(_dunkSuccessRate, 1, 99, false, ResultAction.TwoPointerMade, ResultAction.TwoPointerMissed);
                    break;
                }

                (possessionKept, resultAction) = GetResult(_dunkSuccessRate, 1, dunkSkill1, false, ResultAction.TwoPointerMade, ResultAction.TwoPointerMissed);

                break;
            case Move.Layup:
                float layupSkill1 = playerWithBall.GetRatingForSkillWithItem(playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Layups).ToList()[0]);
                float layupSkill2 = playerWithBall.GetRatingForSkillWithItem(playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Speed).ToList()[0]);
                if (playerWithBall.GetTeamID() == _teamToForceWin)
                {
                    (possessionKept, resultAction) = GetResult(_layupSuccessRate, 2, 99 + 99, false, ResultAction.TwoPointerMade, ResultAction.TwoPointerMissed);
                    break;
                }

                (possessionKept, resultAction) = GetResult(_layupSuccessRate, 2, layupSkill1 + layupSkill2, false, ResultAction.TwoPointerMade, ResultAction.TwoPointerMissed);

                break;
            case Move.Dribble:
                float dribbleSkill1 = playerWithBall.GetRatingForSkillWithItem(playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Dribbling).ToList()[0]);
                float dribbleSkill2 = playerWithBall.GetRatingForSkillWithItem(playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Speed).ToList()[0]);
                float dribbleSkill3 = playerWithBall.GetRatingForSkillWithItem(playerWithBall.GetSkills().Where(x => x.GetSkill() == Skill.Playmaking).ToList()[0]);
                if (playerWithBall.GetTeamID() == _teamToForceWin)
                {
                    (possessionKept, resultAction) = GetResult(_dribbleSuccessRate, 3, 99 + 99 + 99, true, ResultAction.Dribble, ResultAction.Steal);
                    break;
                } else
                {
                    (possessionKept, resultAction) = GetResult(_dribbleSuccessRate, 3, dribbleSkill1 + dribbleSkill2 + dribbleSkill3, true, ResultAction.Dribble, ResultAction.Steal);
                }
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

        //Debug.Log($"Average skill rating: {averageSkillRating}\nChance of succeeding: {chanceOfSucceeding}\nrandom: {random}");

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
                int[] passOutcome = new int[4] { 0, 0, 0, 1};
                int random = UnityEngine.Random.Range(0, 4);
                player.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)>() { ("assists", passOutcome[random] ) });
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

    private void SetLineupAndMinutes(Team team, int matchID, int minutes)
    {
        team.GenerateLineup();

        foreach (Player player in GetCurrentLineup(team.GetLineup(), team.GetTeamID()))
        {
            AssignMinutesToPlayer(matchID, player, minutes);
        }
    }

    private void AssignMinutesToPlayer(int matchID, Player player, int minutes)
    {
        player.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)>() { ("minutes", minutes) });
    }

    private List<Player> GetCurrentLineup(List<string> playerIDs, int teamID)
    {
        Team team = LeagueSystem.Instance.GetTeam(teamID);
        List<Player> currentLineup = team.GetPlayersFromIDs(playerIDs);
        return currentLineup;
    }
}
