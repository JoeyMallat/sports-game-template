using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DraftPickItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _pickRoundAndNumber;
    [SerializeField] TextMeshProUGUI _pickSeason;
    public void SetPickDetails(DraftPick pick)
    {
        _pickRoundAndNumber.text = pick.GetPickDataString();
    }
}
