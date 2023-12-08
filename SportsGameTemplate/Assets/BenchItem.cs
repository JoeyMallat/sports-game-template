using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public class BenchItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    Player _player;

    [SerializeField] Transform _onDragParent;
    [SerializeField] Transform _normalParent;

    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _playerName;
    [SerializeField] TextMeshProUGUI _playerRating;
    [SerializeField] TextMeshProUGUI _playerPosition;

    int _orderInList;
    List<Image> _imageList;

    public void SetPlayerDetails(Player player)
    {
        _player = player;
        _playerPortrait.sprite = player.GetPlayerPortrait();
        _playerName.text = player.GetFullName();
        _playerRating.text = player.CalculateRatingForPosition().ToString();
        _playerPosition.text = player.GetPosition();
    }

    public Player GetPlayer()
    {
        return _player;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _orderInList = transform.GetSiblingIndex();
        transform.SetParent(_onDragParent, false);
        _imageList = GetComponentsInChildren<Image>().ToList();

        _imageList.ForEach(x => x.raycastTarget = false);
        _playerRating.raycastTarget = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        List<GameObject> targets = eventData.hovered.Where(x => x.GetComponent<LineupItem>() != null).ToList();
        if (targets.Count > 0)
        {
            LineupItem lineupItem = targets[0].GetComponent<LineupItem>();

            if (lineupItem.GetPosition() == _player.GetPosition())
            {
                LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).SetPositionInLineup(_player, _player.GetPosition());
                gameObject.SetActive(false);
            }
        }

        _imageList.ForEach(x => x.raycastTarget = true);
        _playerRating.raycastTarget = true;
        transform.SetParent(_normalParent, false);
        transform.SetSiblingIndex(_orderInList);
    }
}
