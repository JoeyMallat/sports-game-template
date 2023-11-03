using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartProgressBar : MonoBehaviour
{
    [SerializeField] Color _selectedColor;
    [SerializeField] Color _deselectedColor;
    [SerializeField] Image _selectionImage;
    [SerializeField] AnimationCurve _selectionMovementAnimation;

    [SerializeField] TextMeshProUGUI _draftText;
    [SerializeField] TextMeshProUGUI _freeAgencyText;
    [SerializeField] TextMeshProUGUI _preseasonText;

    [SerializeField] int _currentState;

    private void Awake()
    {
        SetPartProgressBar(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetPartProgressBar(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetPartProgressBar(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetPartProgressBar(3);
        }
    }

    public void SetPartProgressBar(int state)
    {
        _draftText.color = _deselectedColor;
        _freeAgencyText.color = _deselectedColor;
        _preseasonText.color = _deselectedColor;

        switch (state)
        {
            case 1:
                _draftText.color = _selectedColor;
                LeanTween.moveLocal(_selectionImage.gameObject, new Vector3(-305, 0), 0.5f).setEase(_selectionMovementAnimation);
                break;
            case 2:
                _freeAgencyText.color = _selectedColor;
                LeanTween.moveLocal(_selectionImage.gameObject, new Vector3(0, 0), 0.5f).setEase(_selectionMovementAnimation);
                break;
            case 3:
                _preseasonText.color = _selectedColor;
                LeanTween.moveLocal(_selectionImage.gameObject, new Vector3(305, 0), 0.5f).setEase(_selectionMovementAnimation);
                break;
            default:
                _draftText.color = _deselectedColor;
                _freeAgencyText.color = _deselectedColor;
                _preseasonText.color = _deselectedColor;
                break;
        }
    }
}
