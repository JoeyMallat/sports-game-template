using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LineupItem : MonoBehaviour
{
    [SerializeField] string _positionString;

    [SerializeField] GameObject _noPlayerOverlay;
    [SerializeField] GameObject _playerOverlay;

    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _playerName;
    [SerializeField] TextMeshProUGUI _playerPosition;
    [SerializeField] TextMeshProUGUI _playerRating;
    [SerializeField] TextMeshProUGUI _addToLineUpText;

    public void SetPlayerDetails(Player player)
    {
        if (player == null)
        {
            _noPlayerOverlay.SetActive(true);
            _playerOverlay.SetActive(false);
            SetNoPlayerButton();
            return;
        }

        _noPlayerOverlay.SetActive(false);
        _playerOverlay.SetActive(true);

        _playerPortrait.sprite = player.GetPlayerPortrait();
        _playerPosition.text = player.GetPosition();
        _playerName.text = player.GetFullName();
        _playerRating.text = player.CalculateRatingForPosition().ToString();
    }

    private void SetNoPlayerButton()
    {
        _addToLineUpText.text = $"+\n<size=20%>Drag {_positionString}\nfrom bench";
    }

    public string GetPosition()
    {
        return _positionString;
    }
}
