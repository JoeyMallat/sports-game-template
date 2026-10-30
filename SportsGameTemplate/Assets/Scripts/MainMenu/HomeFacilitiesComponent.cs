using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeFacilitiesComponent : MonoBehaviour, ISettable
{
    [SerializeField] Button _button;

    [SerializeField] Color _activeStarColor;
    [SerializeField] Color _disabledStarColor;

    [SerializeField] Image[] _stars;
    [SerializeField] TextMeshProUGUI _upgradesAvailableText; // Format: "x upgrades available"
    public void SetDetails<T>(T item, bool showMatchButton = false) where T : class
    {
        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i].color = i == 0 ? _activeStarColor : _disabledStarColor;
        }

        _upgradesAvailableText.text = "0 Upgrades available";

        if (_button == null)
        {
            return;
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Store));
    }
}
