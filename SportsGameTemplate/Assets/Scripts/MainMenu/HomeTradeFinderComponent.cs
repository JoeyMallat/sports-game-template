using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeTradeFinderComponent : MonoBehaviour, ISettable
{
    [SerializeField] Button _button;
    [SerializeField] TextMeshProUGUI _recommendationsText;

    public void SetDetails<T>(T item) where T : class
    {
        _recommendationsText.text = "0";

        Team team = item as Team;

        if (_button == null || team == null)
        {
            return;
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() =>
            Navigation.Instance.GoToScreen(true, CanvasKey.PlayerSearch, team)));
    }
}
