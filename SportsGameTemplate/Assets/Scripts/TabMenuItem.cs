using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabMenuItem : MonoBehaviour
{
    int _tabItemIndex;
    [SerializeField] TextMeshProUGUI _tabTitle;
    [SerializeField] GameObject _tabTarget;

    public void SetTabMenu(int index, Action<int> method)
    {
        _tabItemIndex = index;
        Button button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => method.Invoke(_tabItemIndex));
    }

    public void ChangeColor(Color color)
    {
        _tabTitle.color = color;
    }

    public GameObject GetTarget()
    {
        return _tabTarget;
    }
}
