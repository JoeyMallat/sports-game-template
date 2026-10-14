using System.Collections.Generic;

public class TradeValueService
{
    private readonly IReadOnlyList<Team> _teams;
    private readonly TradeValueContext _context;
    private readonly Dictionary<int, TeamTradeProfile> _profiles = new Dictionary<int, TeamTradeProfile>();

    public TradeValueService(IReadOnlyList<Team> teams)
    {
        _teams = teams;
        Dictionary<int, float> classQuality = DraftSystem.Instance == null ? null : DraftSystem.Instance.GetDraftClassQuality();
        _context = TradeValueContext.Create(teams, draftClassQuality: classQuality);
    }

    public TradeValueContext Context => _context;

    public int GetMarketValue(ITradeable asset)
    {
        return asset.CalculateTradeValue();
    }

    public int GetValueForTeam(ITradeable asset, Team team)
    {
        TeamTradeProfile profile = GetProfile(team);
        return UnityEngine.Mathf.RoundToInt(GetMarketValue(asset) * profile.GetAssetMultiplier(asset, _context.Settings));
    }

    public int GetPackageValueForTeam(IEnumerable<ITradeable> assets, Team team)
    {
        int value = 0;
        foreach (ITradeable asset in assets)
            value += GetValueForTeam(asset, team);

        return value;
    }

    public TeamTradeProfile GetProfile(Team team)
    {
        if (!_profiles.TryGetValue(team.GetTeamID(), out TeamTradeProfile profile))
        {
            profile = TeamTradeProfile.Create(team, _teams);
            _profiles.Add(team.GetTeamID(), profile);
        }

        return profile;
    }
}
