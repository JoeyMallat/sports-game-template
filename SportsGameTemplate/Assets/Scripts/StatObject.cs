using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatObject : MonoBehaviour, ISettable
{
    [SerializeField] TextMeshProUGUI _statTitle;
    [SerializeField] List<TextMeshProUGUI> _statText;
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
                _statText[_statText.Count - i - 1].text = stats[i].ToString("F1");
            } else
            {
                _statText[_statText.Count - i - 1].gameObject.SetActive(false);
            }
        }
    }
}
