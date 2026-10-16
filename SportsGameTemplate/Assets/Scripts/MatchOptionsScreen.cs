using System;
using UnityEngine;
using UnityEngine.UI;

public class MatchOptionsScreen : MonoBehaviour, ISettable
{
    [SerializeField] Button _skipToResultButton;
    [SerializeField] Button _liveSimButton;

    public void SetDetails<T>(T item) where T : class
    {
        Match match = item as Match;

        if (match == null)
        {
            Debug.LogError("Invalid item type. Expected Match.");
            return;
        }

        _skipToResultButton.onClick.RemoveAllListeners();
        _skipToResultButton.onClick.AddListener(() => SkipToResult(match));

        _liveSimButton.onClick.RemoveAllListeners();
        _liveSimButton.onClick.AddListener(() => LiveSim(match));
    }

    private void LiveSim(Match match)
    {
        Navigation.Instance.GoToScreen(false, CanvasKey.LiveMatch, match);
    }

    private void SkipToResult(Match match)
    {
        GameManager.Instance.Advance();
        Navigation.Instance.GoToScreen(false, CanvasKey.MatchResult, match);
    }
}
