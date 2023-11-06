using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryGameItem : MonoBehaviour
{
    [SerializeField] Image _itemImage;
    [SerializeField] TextMeshProUGUI _itemName;
    [SerializeField] TextMeshProUGUI _itemBoosts;
    [SerializeField] TextMeshProUGUI _gamesRemaining;
    [SerializeField] Button _assignButton;

    public void SetItem(Player player, OwnedGameItem item, bool showButton)
    {
        GameItem details = ItemDatabase.Instance.GetGameItemByID(item.GetItemID());

        _itemImage.sprite = details.GetItemImage();
        _itemName.text = details.GetItemName();
        _itemBoosts.text = details.GetSkillBoostsString();
        _gamesRemaining.text = item.GetGamesRemainingString();

        
        if (!showButton)
        {
            _assignButton.gameObject.SetActive(false);
            return;
        }

        _assignButton.gameObject.SetActive(true);

        _assignButton.onClick.RemoveAllListeners();
        _assignButton.onClick.AddListener(() => player.AssignItem(item));
        _assignButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID())));
        _assignButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Team, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID())));
        _assignButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Player, player));

        // TODO: Remove item from inventory
    }
}
