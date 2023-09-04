using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player : ITradeable
{
    [SerializeField][ReadOnly] string _playerID;
    [SerializeField] string _firstName;
    [SerializeField] string _lastName;
    [SerializeField] string _position;
    [SerializeField] int _age;
    [ReadOnly][SerializeField] int _rating;
    [ReadOnly][SerializeField] int _tradeValue;
    [SerializeField] List<PlayerSkill> _skills;

    [Header("Appearance Settings")]
    [SerializeField] int _skinID;
    [SerializeField] int _eyesID;
    [SerializeField] int _noseID;
    [SerializeField] int _mouthID;
    [SerializeField] int _hairID;

    public Player(string firstname, string lastname, string position)
    {
        _playerID = RandomIDGenerator.GenerateRandomID();
        _firstName = firstname;
        _lastName = lastname;
        _position = position;
        _age = UnityEngine.Random.Range(20, 39);

        _skills = new();
    }

    public int CalculateTradeValue()
    {
        _tradeValue = CalculateRatingForPosition() * (40 - _age);
        return _tradeValue;
    }

    public int CalculateRatingForPosition()
    {
        _rating = ConfigManager.Instance.GetCurrentConfig().PositionConfig.GetPosition(_position).CalculateAverageRatingForPosition(_skills);
        return _rating;
    }

    public void SetRandomSkills(int averageRating, List<PositionStat> positionStats)
    {
        foreach (PositionStat positionStat in positionStats)
        {
            int ratingForSkill = CalculateRatingForSkill(averageRating, positionStat, positionStats);
            _skills.Add(new PlayerSkill(positionStat.GetSkill(), ratingForSkill));
            CalculateRatingForPosition();
            CalculateTradeValue();
        }
    }

    private int CalculateRatingForSkill(int rating, PositionStat positionStat, List<PositionStat> positionStats)
    {
        float averageWeight = 1f / positionStats.Count;
        float importance = positionStat.GetSkillWeight() - averageWeight;

        int baseRating = rating / 2;
        if (importance > 0) baseRating = Mathf.RoundToInt(Mathf.Clamp(rating * 1.1f, 0, 99));

        float randomness = Graphs.Instance.SkillImportanceGraph.Evaluate(importance) * rating / 4;

        // Debug.Log($"Importance of {positionStat.GetSkill()} stat is {importance}, resulting in {randomness} randomness");

        int skillRating = Mathf.Clamp(Mathf.RoundToInt(UnityEngine.Random.Range(baseRating - randomness, baseRating + randomness)), 0, 99);

        return skillRating;
    }

    public string GetPlayerID()
    {
        return _playerID;
    }
}
