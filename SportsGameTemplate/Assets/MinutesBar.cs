using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinutesBar : MonoBehaviour
{
    [SerializeField] Slider _slider;
    [SerializeField] TextMeshProUGUI _minuteText;

    int _minuteCap;
    Player _player;

    public static event Action<string> OnMinutesChanged;

    private void OnEnable()
    {
        TeamTacticsView.OnMinutesForPositionRemainingUpdated += SetMinuteCap;
    }

    private void OnDisable()
    {
        TeamTacticsView.OnMinutesForPositionRemainingUpdated -= SetMinuteCap;
    }

    private void SetMinuteCap(int value, string position)
    {
        if (_player == null) return;

        if (_player.GetPosition() != position) return;

        value = Mathf.Clamp(Mathf.Max(_player.GetMinutes() + value), 0, 48);

        _minuteCap =  value;
    }

    public void AssignPlayer(Player player)
    {
        _player = player;
    }

    public void SetMinutes(int minutes)
    {
        _slider.SetValueWithoutNotify(minutes);
        OnSliderValueChanged(minutes);
    }

    public void OnSliderValueChanged(float value)
    {
        if (value <= _minuteCap)
        {
            _minuteText.text = value.ToString("F0");
            _player.SetMinutes((int)value);
            _slider.value = value;
        }
        else
        {
            _minuteText.text = _minuteCap.ToString("F0");
            _player.SetMinutes(_minuteCap);
            _slider.value = _minuteCap;
            OnSliderValueChanged(_minuteCap);
        }

        OnMinutesChanged?.Invoke(_player.GetPosition());
    }
}
