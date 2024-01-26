using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TabMenu : MonoBehaviour
{
    [SerializeField] List<TabMenuItem> _tabMenuItems;
    [SerializeField] Color _selectedColor;
    [SerializeField] Color _deselectedColor;
    [SerializeField] ScrollRect _scrollRect;
    [SerializeField] AnimationCurve _moveCurve;

    private void Awake()
    {
        _tabMenuItems = GetComponentsInChildren<TabMenuItem>().ToList();

        for (int i = 0; i < _tabMenuItems.Count; i++)
        {
            _tabMenuItems[i].SetTabMenu(i, SelectTab);
        }
    }

    public void SelectTab(int index)
    {
        for (int i = 0; i < _tabMenuItems.Count; i++)
        {
            if (i == index)
            {
                _tabMenuItems[i].ChangeColor(_selectedColor);
                StartCoroutine(MoveToTarget(_tabMenuItems[i].GetTarget(), 0.2f));
            }
            else
            {
                _tabMenuItems[i].ChangeColor(_deselectedColor);
            }
        }
    }

    private void ToggleButtons(bool status)
    {
        foreach (var tabMenuItem in _tabMenuItems)
        {
            tabMenuItem.enabled = status;
        }
    }

    private IEnumerator MoveToTarget(GameObject target, float seconds)
    {
        Vector2 startPos = _scrollRect.content.localPosition;
        Vector2 targetPos = _scrollRect.GetSnapToPositionToBringChildIntoView(target.GetComponent<RectTransform>());
        float elapsedTime = 0f;

        while (elapsedTime < seconds)
        {
            ToggleButtons(false);
            _scrollRect.content.localPosition = Vector2.Lerp(startPos, targetPos, _moveCurve.Evaluate(elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _scrollRect.content.localPosition = targetPos;
        ToggleButtons(true);
    }
}
