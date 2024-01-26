using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DraftPlayerItem : MonoBehaviour
{
    [Header("Player Asset")]
    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _positionText;
    [SerializeField] TextMeshProUGUI _ratingText;
    [SerializeField] TextMeshProUGUI _ageText;
    [SerializeField] TextMeshProUGUI _potentialText;
    [SerializeField] Button _button;

    public void SetPlayerAssets(Player player)
    {
        _playerPortrait.sprite = player.GetPlayerPortrait();
        _nameText.text = player.GetFullName();
        _positionText.text = player.GetPosition();

        _ratingText.text = player.CalculateRatingForPosition().GetRatingRange(player.GetScoutingPercentage(), player.GetFullName().GetHashCode());
        _ageText.text = player.GetAge().ToString();
        _potentialText.text = player.GetPotential().GetPotentialRange(player.GetScoutingPercentage(), player.GetFullName().GetHashCode());
        SetButton(player);
    }

    private void SetButton(Player player)
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Draft_Player, player));
    }
}
