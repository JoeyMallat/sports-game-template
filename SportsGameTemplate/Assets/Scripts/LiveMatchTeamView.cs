using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LiveMatchTeamView : MonoBehaviour
{
    [SerializeField] Transform _playerItemRoot;
    [SerializeField] PlayerItem _playerItemPrefab;
    [SerializeField] Image _teamLogo;
    [SerializeField] TextMeshProUGUI _teamNameText;

    readonly Dictionary<string, PlayerItem> _playerItemsById = new Dictionary<string, PlayerItem>();
    readonly Dictionary<string, int> _positionOrderByPlayerId = new Dictionary<string, int>();

    public void Initialize(Team team)
    {
        _teamLogo.sprite = team.GetTeamLogo();
        _teamNameText.text = team.GetTeamName();
        _playerItemsById.Clear();
        _positionOrderByPlayerId.Clear();

        List<Player> players = team.GetPlayersFromTeam();
        PlayerItem[] playerItems = _playerItemRoot.GetComponentsInChildren<PlayerItem>(true);
        for (int i = playerItems.Length; i < players.Count; i++)
        {
            PlayerItem playerItem = Instantiate(_playerItemPrefab, _playerItemRoot, false);
            playerItem.gameObject.SetActive(false);
        }

        playerItems = _playerItemRoot.GetComponentsInChildren<PlayerItem>(true);
        for (int i = 0; i < playerItems.Length; i++)
        {
            bool hasPlayer = i < players.Count;
            playerItems[i].gameObject.SetActive(hasPlayer);
            if (!hasPlayer)
                continue;

            Player player = players[i];
            playerItems[i].SetMatchDetails(player, false);
            _playerItemsById[player.GetTradeableID()] = playerItems[i];
            _positionOrderByPlayerId[player.GetTradeableID()] = GetPositionOrder(player.GetPosition());
        }
    }

    public void UpdateState(
        IReadOnlyDictionary<string, BasketballPlayerBoxScore> boxScores,
        IReadOnlyList<string> lineup,
        Color highlightColor,
        float bounceScale,
        float bounceDuration)
    {
        HashSet<string> playersOnCourt = new HashSet<string>(lineup);
        foreach (KeyValuePair<string, PlayerItem> playerItem in _playerItemsById)
        {
            boxScores.TryGetValue(playerItem.Key, out BasketballPlayerBoxScore boxScore);
            playerItem.Value.UpdateMatchDetails(
                boxScore,
                playersOnCourt.Contains(playerItem.Key),
                highlightColor,
                bounceScale,
                bounceDuration);
        }

        SortPlayerItems(boxScores, playersOnCourt);
    }

    void SortPlayerItems(IReadOnlyDictionary<string, BasketballPlayerBoxScore> boxScores, HashSet<string> playersOnCourt)
    {
        List<KeyValuePair<string, PlayerItem>> sortedPlayers = new List<KeyValuePair<string, PlayerItem>>(_playerItemsById);
        sortedPlayers.Sort((left, right) =>
        {
            int onCourtComparison = playersOnCourt.Contains(right.Key).CompareTo(playersOnCourt.Contains(left.Key));
            if (onCourtComparison != 0)
                return onCourtComparison;

            if (playersOnCourt.Contains(left.Key))
            {
                int positionComparison = _positionOrderByPlayerId[left.Key].CompareTo(_positionOrderByPlayerId[right.Key]);
                if (positionComparison != 0)
                    return positionComparison;
            }

            int leftPoints = boxScores.TryGetValue(left.Key, out BasketballPlayerBoxScore leftBoxScore) ? leftBoxScore.Points : 0;
            int rightPoints = boxScores.TryGetValue(right.Key, out BasketballPlayerBoxScore rightBoxScore) ? rightBoxScore.Points : 0;
            int pointsComparison = rightPoints.CompareTo(leftPoints);
            if (pointsComparison != 0)
                return pointsComparison;

            return left.Value.transform.GetSiblingIndex().CompareTo(right.Value.transform.GetSiblingIndex());
        });

        for (int i = 0; i < sortedPlayers.Count; i++)
            sortedPlayers[i].Value.transform.SetSiblingIndex(i);
    }

    static int GetPositionOrder(string position)
    {
        switch (position)
        {
            case "Point Guard": return 0;
            case "Shooting Guard": return 1;
            case "Small Forward": return 2;
            case "Power Forward": return 3;
            case "Center": return 4;
            default: return 5;
        }
    }

    public void StopAnimations()
    {
        foreach (PlayerItem playerItem in _playerItemsById.Values)
            playerItem.StopMatchAnimations();
    }
}
