using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamViewer : MonoBehaviour, ISettable
{
    const int MinutesPerGame = 48;
    const int PlayersOnCourt = 5;

    [SerializeField][ReadOnly] int _currentShowedTeam;
    [SerializeField] RectTransform _playerRoot;
    [SerializeField] RectTransform _pickRoot;
    [SerializeField] TextMeshProUGUI _teamNameText;

    [Header("Rotation")]
    [SerializeField] Transform _rotationRoot;
    [SerializeField] Button _autoAssignRotationButton;
    [SerializeField] LocalSaveManager _localSaveManager;

    Team _team;
    readonly List<PlayerRotationItem> _rotationItems = new List<PlayerRotationItem>();
    bool _rotationDirty;

    void Awake()
    {
        if (_autoAssignRotationButton != null)
        {
            _autoAssignRotationButton.onClick.RemoveListener(AutoAssignRotation);
            _autoAssignRotationButton.onClick.AddListener(AutoAssignRotation);
        }
    }

    void OnDisable()
    {
        if (!_rotationDirty)
            return;

        if (_localSaveManager == null)
            _localSaveManager = FindFirstObjectByType<LocalSaveManager>();

        _localSaveManager?.SaveGame(false);
        _rotationDirty = false;
    }

    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;
        if (team == null)
            return;

        _team = team;
        _currentShowedTeam = team.GetTeamID();
        if (_teamNameText != null)
            _teamNameText.text = team.GetTeamName();

        if (_playerRoot != null)
        {
            List<Player> players = team.GetPlayersFromTeam().OrderBy(x => x.GetPosition()).ThenByDescending(x => x.CalculateRatingForPosition()).ToList();
            List<PlayerItem> playerItems = _playerRoot.GetComponentsInChildren<PlayerItem>(true).ToList();
            SetPlayers(players, playerItems);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_playerRoot);
        }

        if (_pickRoot != null)
        {
            List<DraftPick> draftPicks = team.GetDraftPicks().OrderBy(x => x.GetTotalPickNumber()).ToList();
            List<DraftPickItem> pickItems = _pickRoot.GetComponentsInChildren<DraftPickItem>(true).ToList();
            SetDraftPicks(_currentShowedTeam, draftPicks, pickItems);
        }

        if (_rotationRoot != null)
            PopulateRotation(team);
    }

    void PopulateRotation(Team team)
    {
        List<string> starterIds = team.GetLineup().Where(id => !string.IsNullOrEmpty(id)).ToList();
        List<Player> orderedPlayers = team.GetPlayersFromTeam()
            .OrderBy(player =>
            {
                int starterIndex = starterIds.IndexOf(player.GetTradeableID());
                return starterIndex < 0 ? int.MaxValue : starterIndex;
            })
            .ThenByDescending(player => player.GetMinutes())
            .ThenBy(player => player.GetPosition())
            .ThenByDescending(player => player.CalculateRatingForPosition())
            .ToList();

        _rotationItems.Clear();
        _rotationItems.AddRange(_rotationRoot.GetComponentsInChildren<PlayerRotationItem>(true));
        for (int i = 0; i < _rotationItems.Count; i++)
        {
            bool hasPlayer = i < orderedPlayers.Count;
            _rotationItems[i].gameObject.SetActive(hasPlayer);
            if (!hasPlayer)
                continue;

            Player player = orderedPlayers[i];
            _rotationItems[i].SetDetails(
                player,
                team,
                starterIds.Contains(player.GetTradeableID()),
                HandleMinutesChange,
                HandleStarterRequest);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_rotationRoot as RectTransform);
    }

    void HandleMinutesChange(Player player, int change)
    {
        int currentMinutes = player.GetMinutes();
        int totalMinutes = _team.GetPlayersFromTeam().Sum(item => item.GetMinutes());
        int newMinutes = Mathf.Clamp(currentMinutes + change, 0, MinutesPerGame);
        if (newMinutes > currentMinutes && totalMinutes >= MinutesPerGame * PlayersOnCourt)
            return;

        player.SetMinutes(newMinutes);
        _rotationDirty = true;
        RefreshRotationRows();
    }

    void HandleStarterRequest(Player player)
    {
        _team.SetPositionInLineup(player, player.GetPosition());
        _rotationDirty = true;
        RefreshRotationRows();
    }

    public void AutoAssignRotation()
    {
        if (_team == null)
            return;

        string[] positions = { "Point Guard", "Shooting Guard", "Small Forward", "Power Forward", "Center" };
        for (int i = 0; i < positions.Length; i++)
        {
            List<Player> players = _team.GetPlayersFromTeam()
                .Where(player => player.GetPosition() == positions[i])
                .OrderByDescending(player => player.CalculateRatingForPosition())
                .ToList();

            AssignMinutes(players);
            if (players.Count > 0)
                _team.SetPositionInLineup(players[0], positions[i]);
        }

        _rotationDirty = true;
        RefreshRotationRows();
    }

    static void AssignMinutes(List<Player> players)
    {
        if (players.Count == 0)
            return;

        int totalRating = players.Sum(player => Mathf.Max(1, player.CalculateRatingForPosition()));
        int assignedMinutes = 0;
        for (int i = 0; i < players.Count; i++)
        {
            int minutes = Mathf.FloorToInt(
                Mathf.Max(1, players[i].CalculateRatingForPosition()) / (float)totalRating * MinutesPerGame);
            players[i].SetMinutes(minutes);
            assignedMinutes += minutes;
        }

        players[0].SetMinutes(players[0].GetMinutes() + MinutesPerGame - assignedMinutes);
    }

    void RefreshRotationRows()
    {
        List<string> starterIds = _team.GetLineup();
        int totalMinutes = _team.GetPlayersFromTeam().Sum(player => player.GetMinutes());
        bool canIncrease = totalMinutes < MinutesPerGame * PlayersOnCourt;

        for (int i = 0; i < _rotationItems.Count; i++)
        {
            Player player = _rotationItems[i].Player;
            if (player == null || !_rotationItems[i].gameObject.activeSelf)
                continue;

            _rotationItems[i].Refresh(
                player.GetMinutes(),
                starterIds.Contains(player.GetTradeableID()),
                canIncrease);
        }
    }

    private void SetDraftPicks(int teamID, List<DraftPick> draftPicks, List<DraftPickItem> pickItems)
    {
        for (int i = 0; i < pickItems.Count; i++)
        {
            if (i < draftPicks.Count)
            {
                pickItems[i].gameObject.SetActive(true);
                pickItems[i].SetPickDetails(draftPicks[i], teamID);
            }
            else
            {
                pickItems[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetPlayers(List<Player> players, List<PlayerItem> playerItems)
    {
        for (int i = 0; i < playerItems.Count; i++)
        {
            if (i < players.Count)
            {
                playerItems[i].gameObject.SetActive(true);
                playerItems[i].SetPlayerDetails(players[i]);
            }
            else
            {
                playerItems[i].gameObject.SetActive(false);
            }
        }
    }
}
