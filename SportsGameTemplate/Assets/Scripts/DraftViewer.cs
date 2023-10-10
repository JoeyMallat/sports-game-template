using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DraftViewer : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _picksRoot;
    [SerializeField] GameObject _playersRoot;
    [SerializeField] DraftPlayerItem _draftPlayerPrefab;
    [SerializeField] Transform _draftPlayerParent;
    List<DraftOrderItemWrapper> _draftOrderItemWrappers;

    [SerializeField] TextMeshProUGUI _draftText;
    [SerializeField] TextMeshProUGUI _draftInfoText;
    [SerializeField] TextMeshProUGUI _roundText;
    [SerializeField] TextMeshProUGUI _clockText;

    [SerializeField][TextArea(2, 3)] string _textBeforeDraft;
    [SerializeField][TextArea(2, 3)] string _textAfterPick;
    [SerializeField][TextArea(2, 3)] string _textAfterDraft;

    private void Start()
    {
        DraftSystem.OnGetDraftOrder += SetDetails;
        DraftSystem.OnPlayerPicked += RefreshDraftView;
        DraftSystem.OnDraftClockUpdated += UpdateClock;
        DraftSystem.OnDraftClassUpdated += SetDraftClassPlayers;
    }

    private void SetDraftClassPlayers(DraftClass draftClass)
    {
        List<DraftPlayerItem> draftPlayerItems = _playersRoot.GetComponentsInChildren<DraftPlayerItem>().ToList();
        List<Player> players = draftClass.GetPlayers();
        int playerItemsToBeCreated = players.Count - draftPlayerItems.Count;

        for (int i = 0; i < playerItemsToBeCreated; i++)
        {
            draftPlayerItems.Add(Instantiate(_draftPlayerPrefab, _draftPlayerParent));
        }

        for (int i = 0; i < draftPlayerItems.Count; i++)
        {
            if (i < players.Count)
            {
                draftPlayerItems[i].gameObject.SetActive(true);
                int index = i;
                draftPlayerItems[i].SetPlayerAssets(players[index]);
            } else
            {
                draftPlayerItems[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetDetails<T>(T item) where T : class
    {
        if (item.GetType() != typeof(List<DraftOrderItemWrapper>))
        {
            _draftOrderItemWrappers = FindFirstObjectByType<DraftSystem>().GetDraftOrder(LeagueSystem.Instance.GetTeams());
        } else
        {
            _draftOrderItemWrappers = item as List<DraftOrderItemWrapper>;
        }
        UpdateRoundText(1);
        _draftInfoText.text = _textBeforeDraft;

        List<DraftOrderItem> draftOrderItems = _picksRoot.GetComponentsInChildren<DraftOrderItem>(true).ToList();

        for (int i = 0; i < _draftOrderItemWrappers.Count; i++)
        {
            int index = i;
            if (i < draftOrderItems.Count)
            {
                draftOrderItems[i].SetDraftOrderItem(_draftOrderItemWrappers[index]);
            }
        }
    }

    private void RefreshDraftView(Player player, Team team, int pick)
    {
        _draftInfoText.text = _textAfterPick.Replace("{{playerData}}", $"{player.GetPosition()} {player.GetFullName()} ({player.CalculateRatingForPosition()} OVR)")
            .Replace("{{pickNumber}}", $"#{pick+1}")
            .Replace("{{teamName}}", team.GetTeamName());
        List<DraftOrderItem> draftOrderItems = _picksRoot.GetComponentsInChildren<DraftOrderItem>(true).ToList();

        for (int i = 0; i < _draftOrderItemWrappers.Count; i++)
        {
            int index = i;
            if (i < draftOrderItems.Count)
            {
                draftOrderItems[i].SetDraftOrderItem(_draftOrderItemWrappers[index]);
            }
        }

        UpdateRoundText(Mathf.RoundToInt((pick + 1) / ConfigManager.Instance.GetCurrentConfig().PlayersPerDraftRound) + 1);
        _picksRoot.GetComponentsInChildren<DraftOrderItem>(false).ToList().Last().gameObject.SetActive(false);
    }

    private void UpdateRoundText(int round)
    {
        _roundText.text = $"Round {round}";
    }

    private void UpdateClock(float minutes, float seconds)
    {
        int minutesInt = Mathf.RoundToInt(minutes);
        int secondsInt  = Mathf.RoundToInt(seconds);

        if (seconds >= 59)
        {
            secondsInt = 0;
            minutesInt--;
        }

        if (seconds < 9.5f)
        {
            _clockText.text = $"0{minutesInt}:0{secondsInt}";
        } else
        {
            _clockText.text = $"0{minutesInt}:{secondsInt}";
        }
    }

    private void SetEndGraphics()
    {
        _clockText.text = "00:00";
        _draftInfoText.text = _textAfterDraft;
    }
}

[System.Serializable]
public struct DraftOrderItemWrapper {
    [SerializeField] int _round;
    [SerializeField] int _pickNumber;
    [SerializeField] int _teamID;
    [SerializeField] string _teamName;

    public DraftOrderItemWrapper(int round, int pick, int teamID, string teamName)
    {
        _round = round;
        _pickNumber = pick;
        _teamID = teamID;
        _teamName = teamName;
    }

    public int GetDraftRound()
    {
        return _round;
    }

    public int GetPickNumber()
    {
        return _pickNumber;
    }

    public int GetTeamID()
    {
        return _teamID;
    }

    public string GetTeamName()
    {
        return _teamName;
    }
}
