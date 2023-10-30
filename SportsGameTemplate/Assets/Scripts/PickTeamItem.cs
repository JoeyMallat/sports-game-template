using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PickTeamItem : MonoBehaviour
{
    [SerializeField] Image _teamLogo;
    [SerializeField] TextMeshProUGUI _teamName;
    [SerializeField] Button _onSelectButton;

    public void SetTeamDetails(Team team, Action onClickActions)
    {
        _teamLogo.sprite = team.GetTeamLogo();
        _teamName.text = team.GetTeamName();
        _onSelectButton.onClick.RemoveAllListeners();
        _onSelectButton.onClick.AddListener(() => onClickActions());
    }
}
