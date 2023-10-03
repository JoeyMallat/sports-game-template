using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Navigation : SerializedMonoBehaviour
{
    public static Navigation Instance;

    [SerializeField] Button _backButton;

    [SerializeField] List<Canvas> _openedCanvasses;

    public Dictionary<CanvasKey, Canvas> CanvasDatabase;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        _openedCanvasses = new List<Canvas>();

        Application.targetFrameRate = 120;
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, Player player)
    {
        AddToOpenCanvasses(GetCanvas(canvasKey));
        Canvas canvas = GetCanvas(canvasKey);

        if (!overlay)
            DisableAllCanvasses();

        canvas.enabled = true;

        ISettable settable = canvas.gameObject.GetComponent<ISettable>();
        
        if (settable != null)
        {
            settable.SetDetails(player);
        }

        SetBackButton();
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, Team team)
    {
        AddToOpenCanvasses(GetCanvas(canvasKey));
        Canvas canvas = GetCanvas(canvasKey);

        if (!overlay)
            DisableAllCanvasses();

        canvas.enabled = true;

        ISettable settable = canvas.gameObject.GetComponent<ISettable>();
        
        if (settable != null)
        {
            settable.SetDetails(team);
        }

        SetBackButton();
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, List<Team> teams)
    {
        AddToOpenCanvasses(GetCanvas(canvasKey));
        Canvas canvas = GetCanvas(canvasKey);

        if (!overlay)
            DisableAllCanvasses();

        canvas.enabled = true;

        ISettable settable = canvas.gameObject.GetComponent<ISettable>();

        if (settable != null)
        {
            settable.SetDetails(teams);
        }

        SetBackButton();
    }

    private void AddToOpenCanvasses(Canvas canvas)
    {
        if (_openedCanvasses.Contains(canvas) || canvas == GetCanvas(CanvasKey.MainMenu))
        {
            foreach (Canvas c in _openedCanvasses)
            {
                c.enabled = false;
            }

            _openedCanvasses = new List<Canvas>();
        }

        _openedCanvasses.Add(canvas);
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey)
    {
        AddToOpenCanvasses(GetCanvas(canvasKey));
        Canvas canvas = GetCanvas(canvasKey);

        if (!overlay)
            DisableAllCanvasses();

        canvas.enabled = true;

        SetBackButton();
    }

    public void CloseCanvas(Canvas canvas)
    {
        canvas.enabled = false;
    }

    public Canvas GetCanvas(CanvasKey key)
    {
        Canvas canvas = CanvasDatabase.GetValueOrDefault(key);

        if (canvas == null)
        {
            Debug.LogWarning($"Key {key} has not been assigned in the inspector!");
            return null;
        }

        return canvas;
    }

    private void SetBackButton()
    {
        _backButton.GetComponentInParent<Canvas>().enabled = true;

        if (_openedCanvasses.Count > 1)
        {
            _backButton.gameObject.SetActive(true);

            _backButton.onClick.RemoveAllListeners();
            _backButton.onClick.AddListener(() => CloseCanvas(_openedCanvasses.Last()));
            _backButton.onClick.AddListener(() => _openedCanvasses.RemoveAt(_openedCanvasses.Count - 1));
            _backButton.onClick.AddListener(() => SetBackButton());

        } else 
        {
            _backButton.gameObject.SetActive(false);
        }
    }

    private void DisableAllCanvasses()
    {
        Canvas[] canvasses = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Canvas canvas in canvasses)
        {
            canvas.enabled = false;
        }
    }
}
