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

        _dropdowns = GetComponentsInChildren<TMP_Dropdown>().ToList();

        if (_dropdownValues == null || _dropdownValues.Count == 0)
        {
            _dropdownValues = new List<int>();
            _dropdowns.ForEach(x => _dropdownValues.Add(0));
        }

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

            if (index >= _dropdownValues.Count) break;

            _dropdownValues[index] = _dropdowns[i].value;
        }

        return _dropdownValues;
    }

    public string GetMainTacticName()
    {
        if (_dropdowns == null)
        {
            _dropdowns = GetComponentsInChildren<TMP_Dropdown>(true).ToList();
        }

        if (_dropdowns.Count == 0 || _dropdowns[0].options.Count == 0)
        {
            return "Balanced";
        }

        int selectedValue = _dropdownValues != null && _dropdownValues.Count > 0
            ? _dropdownValues[0]
            : _dropdowns[0].value;
        selectedValue = Mathf.Clamp(selectedValue, 0, _dropdowns[0].options.Count - 1);

        return _dropdowns[0].options[selectedValue].text;
    }
}
