using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] bool _isHigherButton;
    public static Action<bool, bool> OnHoldStatusChanged;

    bool _pressed = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        OnHoldStatusChanged?.Invoke(_isHigherButton, _pressed);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
        OnHoldStatusChanged?.Invoke(_isHigherButton, _pressed);
    }
}
