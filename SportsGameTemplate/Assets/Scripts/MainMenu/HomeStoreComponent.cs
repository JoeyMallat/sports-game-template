using UnityEngine;
using UnityEngine.UI;

public class HomeStoreComponent : MonoBehaviour
{
    [SerializeField] Button _button;

    private void Awake()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        if (_button == null)
        {
            return;
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Store));
    }
}
