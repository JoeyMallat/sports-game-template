using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DraftClass
{
    [SerializeField] int _season;
    [SerializeField] float _quality;
    [SerializeField] List<Player> _playersInDraftClass;

    public DraftClass(int rating, int season = 0)
    {
        _season = season;
        SquadCreator squadCreator = new SquadCreator();

        _playersInDraftClass = squadCreator.CreateDraftClass(rating);
        _playersInDraftClass = _playersInDraftClass.OrderByDescending(x => x.CalculateTradeValue() * UnityEngine.Random.Range(0.8f, 1.2f)).ToList();
        _quality = CalculateQuality();
    }

    public int GetSeason()
    {
        return _season;
    }

    public float GetQuality()
    {
        if (_quality <= 0f)
            _quality = CalculateQuality();

        return _quality;
    }

    private float CalculateQuality()
    {
        if (_playersInDraftClass == null || _playersInDraftClass.Count == 0)
            return 1f;

        List<int> ratings = _playersInDraftClass
            .Select(player => player.CalculateRatingForPosition())
            .OrderByDescending(rating => rating)
            .ToList();
        int topCount = Mathf.Min(ConfigManager.Instance.GetCurrentConfig().PlayersPerDraftRound, ratings.Count);
        float topEnd = (float)ratings.Take(Mathf.Min(5, topCount)).Average();
        float firstRoundDepth = (float)ratings.Take(topCount).Average();
        float rawQuality = (topEnd * 0.6f + firstRoundDepth * 0.4f) / 65f;

        return Mathf.Clamp(rawQuality, 0.7f, 1.3f);
    }

    public List<Player> GetPlayers()
    {
        return _playersInDraftClass;
    }

    public Player PickPlayerAtID(int id, Team team, int pickNumber)
    {
        Player chosenPlayer = _playersInDraftClass[id];
        _playersInDraftClass.RemoveAt(id);
        team.AddPlayer(chosenPlayer, pickNumber);
        return chosenPlayer;
    }

    public void PickPlayer(Player player, Team team, int pickNumber)
    {
        _playersInDraftClass.Remove(player);
        team.AddPlayer(player, pickNumber);
    }
}
