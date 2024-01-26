using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TacticsSettings : MonoBehaviour, ISettable
{
    List<TMP_Dropdown> _dropdowns;
    [SerializeField] List<int> _dropdownValues;

    private void Awake()
    {
        _dropdowns = GetComponentsInChildren<TMP_Dropdown>().ToList();

        if (_dropdownValues.Count == 0)
        {
            _dropdownValues = new List<int>();
            _dropdowns.ForEach(x => _dropdownValues.Add(0));
        }
    }

    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;

        for (int i = 0; i < _dropdowns.Count; i++)
        {
            int index = i;
            Debug.Log(_dropdownValues[index]);
            _dropdowns[i].value = _dropdownValues[index];
        }
    }

    public void SetDropdownValuesAfterLoading(List<int> values)
    {
        _dropdownValues = values;
    }

    public List<int> GetValues()
    {
        for (int i = 0; i < _dropdowns.Count; i++)
        {
            int index = i;
            _dropdownValues[index] = _dropdowns[i].value;
        }

        return _dropdownValues;
    }
}
