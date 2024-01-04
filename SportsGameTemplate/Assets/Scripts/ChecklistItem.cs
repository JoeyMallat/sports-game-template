using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ChecklistItem : MonoBehaviour
{
    [SerializeField] Image _checkmark;
    [SerializeField] TextMeshProUGUI _checklistText;
    [SerializeField] List<Button> _buttons;

    public void SetChecklistItem(bool completed, string title, string subtitle, List<string> buttonTexts, List<Action> buttonActions)
    {
        _checkmark.gameObject.SetActive(completed);
        _checkmark.enabled = completed;

        _checklistText.text = "";
        _checklistText.text = title;
        _checklistText.text += $"\n<size=50%><color=#FF9900>{subtitle}";

        if (buttonTexts.Count == 0)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].gameObject.SetActive(false);
            }
            return;
        }

        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].gameObject.SetActive(true);

            if (completed)
            {
                _buttons[i].ToggleButtonStatus(false);
            } else
            {
                _buttons[i].ToggleButtonStatus(true);
            }
            int index = i;
            _buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = buttonTexts[index];
            _buttons[i].onClick.RemoveAllListeners();

            if (index <= buttonActions.Count)
            {
                _buttons[i].onClick.AddListener(() => buttonActions[index]());
            }
        }
    }
}
