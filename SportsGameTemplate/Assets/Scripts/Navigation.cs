using Sirenix.OdinInspector;
using System;
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

        GameManager.OnAdvance += RefreshMainMenu;
        GameManager.OnGameStarted += RefreshMainMenu;

        GoToScreen(false, CanvasKey.Setup);
    }

    public void RefreshMainMenu(SeasonStage seasonStage, int week)
    {
        Canvas canvas = GetCanvas(CanvasKey.MainMenu);
        canvas.GetComponent<ISettable>().SetDetails(LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()));
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, Player player)
    {
        if (!overlay)
            DisableAllCanvasses();

        AddToOpenCanvasses(GetCanvas(canvasKey));
        Canvas canvas = GetCanvas(canvasKey);
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
        if (!overlay)
            DisableAllCanvasses();

        AddToOpenCanvasses(GetCanvas(canvasKey));
        Canvas canvas = GetCanvas(canvasKey);
        canvas.enabled = true;

        ISettable settable = canvas.gameObject.GetComponent<ISettable>();
        
        if (settable != null)
        {
            settable.SetDetails(team);
            Debug.Log("ISettable is being updated");
        }

        SetBackButton();
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, List<Team> teams)
    {
        if (!overlay)
            DisableAllCanvasses();

        AddToOpenCanvasses(GetCanvas(canvasKey));
        Canvas canvas = GetCanvas(canvasKey);
        canvas.enabled = true;

        ISettable settable = canvas.gameObject.GetComponent<ISettable>();

        if (settable != null)
        {
            settable.SetDetails(teams);
        }

        SetBackButton();
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, BallItem pickedBall)
    {
        if (!overlay)
            DisableAllCanvasses();

        AddToOpenCanvasses(GetCanvas(canvasKey));
        Canvas canvas = GetCanvas(canvasKey);
        canvas.enabled = true;

        ISettable settable = canvas.gameObject.GetComponent<ISettable>();

        if (settable != null)
        {
            settable.SetDetails(pickedBall);
        }

        SetBackButton();
    }

    private void AddToOpenCanvasses(Canvas canvas)
    {
        if (canvas == GetCanvas(CanvasKey.MainMenu))
        {
            foreach (Canvas c in _openedCanvasses)
            {
                c.enabled = false;
            }

            _openedCanvasses = new List<Canvas>();
        }

        if (_openedCanvasses.Contains(canvas))
        {
            _openedCanvasses.Remove(canvas);
        }

        _openedCanvasses.Add(canvas);


        for (int i = 0; i < _openedCanvasses.Count; i++)
        {
            int index = i;
            _openedCanvasses[i].sortingOrder = index;
            Debug.Log($"Sorting order of {_openedCanvasses[i].gameObject.name} is {index}");
        }
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey)
    {
        if (!overlay)
            DisableAllCanvasses();

        AddToOpenCanvasses(GetCanvas(canvasKey));
        Canvas canvas = GetCanvas(canvasKey);
        canvas.enabled = true;

        SetBackButton();
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, int coinsNeeded)
    {
        if (coinsNeeded > GameManager.Instance.GetGems())
        {
            GoToScreen(true, CanvasKey.Store);
            return;
        } else
        {
            // Subtract the gems off your current balance
            GameManager.Instance.EditGems(-coinsNeeded);
        }

        if (!overlay)
            DisableAllCanvasses();

        AddToOpenCanvasses(GetCanvas(canvasKey));
        Canvas canvas = GetCanvas(canvasKey);
        canvas.enabled = true;

        SetBackButton();
    }

    public void CloseCanvas(Canvas canvas)
    {
        canvas.enabled = false;
        canvas.sortingOrder = 0;

        if (_openedCanvasses.Contains(canvas)) 
        {
            _openedCanvasses.Remove(canvas);
        }

        SetBackButton();
    }

    public void CloseCanvas()
    {
        _openedCanvasses.Last().enabled = false;

        _openedCanvasses.Remove(_openedCanvasses.Last());

        SetBackButton();
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
            if (!canvas.CompareTag("AlwaysEnabled"))
            {
                canvas.enabled = false;
            }
        }

        _openedCanvasses = new List<Canvas>();
    }
}
