using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForceWinToggle : MonoBehaviour
{
    [SerializeField] Image _checkmarkImage;

    public void ToggleForceWin(){
        GameManager.Instance.SetCurrentForceWinState(!GameManager.Instance.GetCurrentForceWinState());
        _checkmarkImage.gameObject.SetActive(GameManager.Instance.GetCurrentForceWinState());
    }
}
