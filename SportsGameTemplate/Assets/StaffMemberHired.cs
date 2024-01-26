using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaffMemberHired : MonoBehaviour
{
    [SerializeField] GameObject _hiredOverlay;
    [SerializeField] GameObject _notHiredOverlay;

    [SerializeField] Image _portrait;
    [SerializeField] TextMeshProUGUI _boostText;

    public void SetDetails(StaffMember coach)
    {
        if (coach.IsSet())
        {
            GetComponent<Button>().interactable = false;
            _hiredOverlay.SetActive(true);
            _notHiredOverlay.SetActive(false);

            _portrait.sprite = coach.GetPortrait();
            _boostText.text = coach.GetBoostTypeString();
        }
        else
        {
            GetComponent<Button>().interactable = true;
            _hiredOverlay.SetActive(false);
            _notHiredOverlay.SetActive(true);
        }
    }
}
