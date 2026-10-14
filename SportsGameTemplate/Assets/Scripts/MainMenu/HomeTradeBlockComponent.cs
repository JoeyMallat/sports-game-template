using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeTradeBlockComponent : MonoBehaviour, ISettable
{
    [SerializeField] Button _button;

    [SerializeField] TextMeshProUGUI _playersOnTradeBlockText;

    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;

        if (team == null)
        {
            return;
        }

        int playersOnTradeBlock = team.GetPlayersFromTeam().Count(player => player.GetTradingBlockStatus());
        _playersOnTradeBlockText.text = $"{playersOnTradeBlock} / 5";

        if (_button == null)
        {
            return;
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Trade));
    }
}
