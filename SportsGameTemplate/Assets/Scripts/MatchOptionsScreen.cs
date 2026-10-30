using System;
using UnityEngine;
using UnityEngine.UI;

public class MatchOptionsScreen : MonoBehaviour, ISettable
{
    [SerializeField] Button _skipToResultButton;
    [SerializeField] Button _liveSimButton;

    public void SetDetails<T>(T item, bool showMatchButton = false) where T : class
    {
        Match match = item as Match;

        if (match == null)
        {
            Debug.LogError("Invalid item type. Expected Match.");
            return;
        }

        SetButtonsInteractable(true);
        _skipToResultButton.onClick.RemoveAllListeners();
        _skipToResultButton.onClick.AddListener(SkipToResult);

        _liveSimButton.onClick.RemoveAllListeners();
        _liveSimButton.onClick.AddListener(() => LiveSim(match));
    }

    private void LiveSim(Match match)
    {
        SetButtonsInteractable(false);
        Navigation.Instance.GoToScreen(false, CanvasKey.LiveMatch, match);
    }

    private void SkipToResult()
    {
        SetButtonsInteractable(false);
        GameManager.Instance.Advance();
    }

    void SetButtonsInteractable(bool interactable)
    {
        _skipToResultButton.interactable = interactable;
        _liveSimButton.interactable = interactable;
    }
}
