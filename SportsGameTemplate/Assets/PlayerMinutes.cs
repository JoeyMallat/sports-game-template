using UnityEngine;

public class PlayerMinutes : MonoBehaviour
{
    [SerializeField] PlayerItem _playerItem;
    [SerializeField] MinutesBar _minutesBar;

    public void SetPlayerDetails(Player player)
    {
        _playerItem.SetPlayerDetails(player);
        _minutesBar.AssignPlayer(player);
        _minutesBar.SetMinutes(player.GetMinutes());
    }
}
