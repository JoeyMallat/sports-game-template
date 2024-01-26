using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatObject : MonoBehaviour, ISettable
{
    [SerializeField] TextMeshProUGUI _statTitle;
    [SerializeField] List<TextMeshProUGUI> _statText;
    [SerializeField] Button _playerLinkButton;

    public void SetDetails<T>(T item) where T : class
    {
        StatObjectWrapper wrapper = item as StatObjectWrapper;

        _statTitle.text = wrapper.ReadStatObject().Item1;
        List<float> stats = wrapper.ReadStatObject().Item2;

        stats.Reverse();

        for (int i = 0; i < _statText.Count; i++)
        {
            if (i < stats.Count)
            {
                _statText[_statText.Count - i - 1].gameObject.SetActive(true);

                if (Mathf.Approximately(stats[i], Mathf.RoundToInt(stats[i])))
                {
                    _statText[_statText.Count - i - 1].text = stats[i].ToString("F0");
                }
                else
                {
                    _statText[_statText.Count - i - 1].text = stats[i].ToString("F1");
                }
                _playerLinkButton.onClick.RemoveAllListeners();
                _playerLinkButton.interactable = false;

                if (wrapper.GetPlayerLink() != null)
                {
                    _playerLinkButton.interactable = true;
                    _playerLinkButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Player, wrapper.GetPlayerLink()));
                }
            }
            else
            {
                _statText[_statText.Count - i - 1].gameObject.SetActive(false);
            }
        }
    }
}
