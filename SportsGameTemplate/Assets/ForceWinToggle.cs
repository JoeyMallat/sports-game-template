using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.RemoteConfig;

public class ForceWinToggle : MonoBehaviour
{
    [SerializeField] Image _checkmarkImage;

    private void Awake()
    {
        GameManager.OnForceWinUpdated += UpdateForceWinToggle;
    }

    private void UpdateForceWinToggle(bool newState)
    {
        GameManager.Instance.SetCurrentForceWinState(newState);
        _checkmarkImage.gameObject.SetActive(GameManager.Instance.GetCurrentForceWinState());
    }


    public void ToggleForceWin(){
        if (GameManager.Instance.GetCurrentForceWinState())
        {
            GameManager.Instance.AddToGems(RemoteConfigService.Instance.appConfig.GetInt("forcewin_cost", 78));
            GameManager.Instance.SetCurrentForceWinState(false);
            _checkmarkImage.gameObject.SetActive(GameManager.Instance.GetCurrentForceWinState());
        } else
        {
            if (GameManager.Instance.CheckBuyItem(RemoteConfigService.Instance.appConfig.GetInt("forcewin_cost", 78)))
            {
                GameManager.Instance.SetCurrentForceWinState(true);
                _checkmarkImage.gameObject.SetActive(GameManager.Instance.GetCurrentForceWinState());
            } else
            {
                TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(true, CanvasKey.Store));
            }
        }

    }
}
