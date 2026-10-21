using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeTradeOffersComponent : MonoBehaviour, ISettable
{
    [SerializeField] Button _button;

    [SerializeField] TextMeshProUGUI _unansweredTradeOffersText;

    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;

        if (team == null)
        {
            return;
        }

        _unansweredTradeOffersText.text = team.GetAllTradeOffers().Count.ToString();

        if (_button == null)
        {
            return;
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.TradeOffers));
    }
}
