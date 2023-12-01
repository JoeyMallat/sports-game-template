using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DraftOrderItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _pickNumberText;
    [SerializeField] TextMeshProUGUI _teamNameText;

    public void SetDraftOrderItem(DraftPick draftPick, Team team)
    {
        _pickNumberText.text = $"#{_pickNumberText}";
        _teamNameText.text = team.GetTeamName();
    }
    
    public void SetDraftOrderItem(DraftOrderItemWrapper draftOrderItem)
    {
        gameObject.SetActive(true);
        _pickNumberText.text = $"#{draftOrderItem.GetPickNumber()}";
        _teamNameText.text = draftOrderItem.GetTeamName();

        if (draftOrderItem.GetPickNumber() > 9)
        {
            _pickNumberText.characterSpacing = -10;
            _pickNumberText.fontSize = 220;
        } else
        {
            _pickNumberText.characterSpacing = 0;
            _pickNumberText.fontSize = 280;
        }
    }
}
