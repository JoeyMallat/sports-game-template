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
        Config config = ConfigManager.Instance == null ? null : ConfigManager.Instance.GetCurrentConfig();
        TradeValueSettings settings = config?.TradeValueSettings ?? TradeValueSettings.Default;
        _context = TradeValueContext.Create(teams, settings, classQuality);
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

    public int GetPackageMarketValue(IEnumerable<ITradeable> assets)
    {
        return GetPackageValue(assets, null);
    }

    public int GetPackageValueForTeam(IEnumerable<ITradeable> assets, Team team)
    {
        return GetPackageValue(assets, team);
    }

    private int GetPackageValue(IEnumerable<ITradeable> assets, Team team)
    {
        int nonPlayerValue = 0;
        List<int> playerValues = new List<int>();

        foreach (ITradeable asset in assets)
        {
            if (asset == null)
                continue;

            int assetValue = team == null ? GetMarketValue(asset) : GetValueForTeam(asset, team);
            if (asset is Player)
                playerValues.Add(assetValue);
            else
                nonPlayerValue += assetValue;
        }

        return CalculatePackageValue(playerValues, nonPlayerValue, _context.Settings.AdditionalPlayerValueMultiplier);
    }

    public static int CalculatePackageValue(IEnumerable<int> playerValues, int nonPlayerValue, float additionalPlayerMultiplier)
    {
        List<int> sortedPlayerValues = new List<int>(playerValues);
        sortedPlayerValues.Sort((left, right) => right.CompareTo(left));

        int packageValue = nonPlayerValue;
        float multiplier = UnityEngine.Mathf.Clamp01(additionalPlayerMultiplier);
        for (int i = 0; i < sortedPlayerValues.Count; i++)
            packageValue += UnityEngine.Mathf.RoundToInt(sortedPlayerValues[i] * UnityEngine.Mathf.Pow(multiplier, i));

        return packageValue;
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
