using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForceWinToggle : MonoBehaviour
{
    const int ForceWinCost = 78;
    [SerializeField] Image _checkmarkImage;
    [SerializeField] TextMeshProUGUI _forceWinText;
    int _reservedCost;

    private void Awake()
    {
        Match.OnMatchPlayed += CheckWinToggle;
    }

    private void Start()
    {
        _forceWinText.text = $"Force win   <color=\"white\"> {ForceWinCost} DP";
    }

    private void CheckWinToggle(Match match)
    {
        if (match.IsMyTeamMatch(GameManager.Instance.GetTeamID()))
        {
            UpdateForceWinToggle(false);
        }
    }

    private void UpdateForceWinToggle(bool newState)
    {
        GameManager.Instance.SetCurrentForceWinState(newState);
        _checkmarkImage.gameObject.SetActive(GameManager.Instance.GetCurrentForceWinState());
    }


    public void ToggleForceWin()
    {
        _forceWinText.text = $"Force win   <color=\"white\"> {ForceWinCost} DP";

        if (GameManager.Instance.GetCurrentForceWinState())
        {
            DevelopmentPointsManager.Instance.Award(_reservedCost, DevelopmentPointsSource.Refund, DevelopmentPointsManager.CreateTransactionId("force-win-refund"), "Force win refund");
            _reservedCost = 0;
            GameManager.Instance.SetCurrentForceWinState(false);
            _checkmarkImage.gameObject.SetActive(GameManager.Instance.GetCurrentForceWinState());
        }
        else
        {
            int cost = ForceWinCost;
            if (DevelopmentPointsManager.Instance.Spend(cost, DevelopmentPointsSource.GameplayPurchase, DevelopmentPointsManager.CreateTransactionId("force-win"), "Force win"))
            {
                _reservedCost = cost;
                GameManager.Instance.SetCurrentForceWinState(true);
                _checkmarkImage.gameObject.SetActive(GameManager.Instance.GetCurrentForceWinState());
            }
            else
            {
                TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(true, CanvasKey.Store));
            }
        }

    }
}
