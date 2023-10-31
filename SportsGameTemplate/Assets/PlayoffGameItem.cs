using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayoffGameItem : MonoBehaviour
{
    [SerializeField] Image _logoImage;

    public void SetItem(Team team = null)
    {
        if (team == null)
        {
            _logoImage.enabled = false;
        } else
        {
            _logoImage.enabled = true;
            _logoImage.sprite = team.GetTeamLogo();
        }
    }
}
