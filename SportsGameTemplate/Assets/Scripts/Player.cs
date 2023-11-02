using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Player : ITradeable
{
    [SerializeField][ReadOnly] string _playerID;
    [SerializeField] int _teamID;
    [SerializeField] string _firstName;
    [SerializeField] string _lastName;
    [SerializeField] string _position;
    [SerializeField] int _age;
    [SerializeField] Potential _potential;
    [SerializeField] float _percentageScouted;
    [ReadOnly][SerializeField] int _rating;
    [ReadOnly][SerializeField] int _tradeValue;
    [SerializeField] bool _onTradingBlock;
    [SerializeField] List<PlayerSkill> _skills;
    [SerializeField] List<PlayerSeason> _seasonStats;
    [SerializeField] List<TradeOffer> _tradeOffers;

    [SerializeField] Contract _contract;

    public static event Action<int, Player> OnAddedToTrade;
    public static event Action<Player> OnTradeOfferReceived;
    public static event Action<Player> OnPlayerScouted;

    [Header("Appearance Settings")]
    [SerializeField] int _portraitID;

    public Player(string firstname, string lastname, Position position, int teamRating, int teamID)
    {
        _playerID = RandomIDGenerator.GenerateRandomID();
        _firstName = firstname;
        _lastName = lastname;
        _position = position.GetPositionName();
        _age = UnityEngine.Random.Range(20, 39);
        _teamID = teamID;

        SetRandomSkills(teamRating, position.GetPositionStats());

        _contract = new Contract(CalculateRatingForPosition(), _age);
        _potential = SetPotential();
        _portraitID = UnityEngine.Random.Range(0, Resources.LoadAll<Sprite>("Faces/").Length);

        _seasonStats = new List<PlayerSeason>();
        _seasonStats.Add(new PlayerSeason(0, _teamID));
        _tradeOffers = new List<TradeOffer>();
    }

    public Player(bool rookie, string firstname, string lastname, Position position, int averageRating)
    {
        _playerID = RandomIDGenerator.GenerateRandomID();
        _firstName = firstname;
        _lastName = lastname;
        _position = position.GetPositionName();
        _age = UnityEngine.Random.Range(20, 22);
        _percentageScouted = 0f;

        SetRandomSkills(averageRating, position.GetPositionStats());
        _potential = SetPotential();

        _portraitID = UnityEngine.Random.Range(0, Resources.LoadAll<Sprite>("Faces/").Length);
        _teamID = -1;
    }

    public void ScoutPlayer()
    {
        if (_percentageScouted == 1) return;

        _percentageScouted += 0.2f;

        OnPlayerScouted?.Invoke(this);
    }

    public void ToggleTradingBlockStatus()
    {
        _onTradingBlock = !_onTradingBlock;
    }

    public bool GetTradingBlockStatus()
    {
        return _onTradingBlock;
    }

    public void AddToTrade(int teamID)
    {
        OnAddedToTrade?.Invoke(_teamID, this);
    }

    public void SignContract(int newSalary, int newLength)
    {
        _contract.SetNewContract(newSalary, newLength);
    }

    public void AddTradeOffer(TradeOffer tradeOffer)
    {
        _tradeOffers.Add(tradeOffer);
        OnTradeOfferReceived?.Invoke(this);
    }

    public List<TradeOffer> GetTradeOffers()
    {
        if (_tradeOffers.Count == 0)
        {
            return null;
        }

        return _tradeOffers;
    }

    public void ChangeTeam(int teamID)
    {
        _teamID = teamID;
    }

    public void ChangeTeam(int teamID, int pick)
    {
        _teamID = teamID;
        _contract = new Contract(pick);
    }

    public string GetFullName()
    {
        return $"{_firstName} {_lastName}".Replace("\r", "");
    }

    public int GetAge()
    {
        return _age;
    }

    public int GetTeamID()
    {
        return _teamID;
    }

    public string GetPosition()
    {
        return _position;
    }

    public PlayerSeason GetLatestSeason()
    {
        return _seasonStats.Last();
    }

    private Potential SetPotential()
    {
        float random = UnityEngine.Random.Range(0f, 1f);
        float valueOnGraph = ConfigManager.Instance.GetCurrentConfig().DistributionOfPotential.Evaluate(random);

        switch (valueOnGraph)
        {
            case >= 0.25f:
                return Potential.Starter;
            case >= 0.2f:
                return Potential.Bench;
            case >= 0.15f:
                return Potential.Filler;
            case >= 0.05f:
                return Potential.WorldClass;
            default:
                return Potential.Elite;
        }
    }

    public Potential GetPotential()
    {
        return _potential;
    }

    public float GetScoutingPercentage()
    {
        return _percentageScouted;
    }

    public int CalculateTradeValue()
    {
        _tradeValue = CalculateRatingForPosition() * (40 - _age);
        float contractLengthMultiplier = ConfigManager.Instance.GetCurrentConfig().ContractLengthImpactOnTradeValue.Evaluate(_age / 40);

        _tradeValue = Mathf.RoundToInt(_tradeValue * contractLengthMultiplier / ((float)_potential + 1));
        return _tradeValue;
    }

    public int CalculateRatingForPosition()
    {
        _rating = ConfigManager.Instance.GetCurrentConfig().PositionConfig.GetPosition(_position).CalculateAverageRatingForPosition(_skills);
        return _rating;
    }

    public void SetRandomSkills(int averageRating, List<PositionStat> positionStats)
    {
        _skills = new();
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

        int skillRating = Mathf.Clamp(Mathf.RoundToInt(UnityEngine.Random.Range(baseRating - randomness, baseRating + randomness)), 0, 99);

        return skillRating;
    }

    public List<PlayerSkill> GetSkills()
    {
        return _skills;
    }

    public string GetTradeableID()
    {
        return _playerID;
    }

    public Contract GetContract()
    {
        return _contract;
    }

    public Contract SetRookieContract(int pick)
    {
        _contract = new Contract(pick);
        return _contract;
    }

    public Sprite GetPlayerPortrait()
    {
        return Resources.Load<Sprite>($"Faces/{_portraitID}");
    }
}
