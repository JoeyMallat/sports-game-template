using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class Navigation : SerializedMonoBehaviour
{
    sealed class NavigationEntry
    {
        public Canvas Canvas;
        public CanvasKey Key;
        public Action Configure;
        public bool ShowMatchButton;
    }

    public static Navigation Instance;

    [SerializeField] Button _backButton;
    [SerializeField] NextMatchButton _nextMatchButton;

    [SerializeField] List<Canvas> _openedCanvasses;

    readonly List<NavigationEntry> _history = new List<NavigationEntry>();

    public Dictionary<CanvasKey, Canvas> CanvasDatabase;

    public static event Action OnNewScreenOpened;
    public static event Action<CanvasKey> OnScreenOpened;
    public CanvasKey CurrentScreen { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        if (_nextMatchButton == null)
        {
            _nextMatchButton = FindFirstObjectByType<NextMatchButton>(FindObjectsInactive.Include);
        }

        _openedCanvasses = new List<Canvas>();

        Application.targetFrameRate = 120;

        GameManager.OnAdvance += RefreshMainMenu;
        GameManager.OnGameStarted += RefreshMainMenu;

        GoToScreen(false, CanvasKey.Startup);
    }

    public void RefreshMainMenu(SeasonStage seasonStage, int week)
    {
        Canvas canvas = GetCanvas(CanvasKey.MainMenu);
        canvas.GetComponent<ISettable>().SetDetails(LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()), true);
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, Player player, bool showMatchButton = false)
    {
        OpenScreen(overlay, canvasKey, showMatchButton,
            settable => settable.SetDetails(player, showMatchButton));
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, Match match, bool showMatchButton = false)
    {
        OpenScreen(overlay, canvasKey, showMatchButton,
            settable => settable.SetDetails(match, showMatchButton));
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, Team team, bool showMatchButton = false)
    {
        OpenScreen(overlay, canvasKey, showMatchButton,
            settable => settable.SetDetails(team, showMatchButton));
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, List<Team> teams, bool showMatchButton = false)
    {
        OpenScreen(overlay, canvasKey, showMatchButton,
            settable => settable.SetDetails(teams, showMatchButton));
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, BallItem pickedBall, bool showMatchButton = false)
    {
        OpenScreen(overlay, canvasKey, showMatchButton,
            settable => settable.SetDetails(pickedBall, showMatchButton));
    }

    void OpenScreen(bool overlay, CanvasKey canvasKey, bool showMatchButton, Action<ISettable> configure)
    {
        Canvas canvas = GetCanvas(canvasKey);
        if (canvas == null)
            return;

        if (!overlay)
            DisableAllCanvasses();

        RemoveFromHistory(canvas);
        _openedCanvasses.Add(canvas);
        NavigationEntry entry = new NavigationEntry
        {
            Canvas = canvas,
            Key = canvasKey,
            ShowMatchButton = showMatchButton
        };
        entry.Configure = () =>
        {
            ISettable settable = canvas.gameObject.GetComponent<ISettable>();
            if (settable != null && configure != null)
                configure(settable);
        };
        _history.Add(entry);

        for (int i = 0; i < _openedCanvasses.Count; i++)
            _openedCanvasses[i].sortingOrder = i;

        canvas.enabled = true;
        entry.Configure();
        CurrentScreen = canvasKey;
        OnNewScreenOpened?.Invoke();
        OnScreenOpened?.Invoke(canvasKey);
        SetBackButton();
    }

    void RemoveFromHistory(Canvas canvas)
    {
        for (int i = _history.Count - 1; i >= 0; i--)
        {
            if (_history[i].Canvas == canvas)
                _history.RemoveAt(i);
        }

        _openedCanvasses.Remove(canvas);
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, bool showMatchButton = false)
    {
        OpenScreen(overlay, canvasKey, showMatchButton, null);
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, int coinsNeeded, bool showMatchButton = false)
    {
        if (!DevelopmentPointsManager.Instance.CanAfford(coinsNeeded))
        {
            GoToScreen(true, CanvasKey.Store);
            return;
        }
        else
        {
            DevelopmentPointsManager.Instance.Spend(coinsNeeded, DevelopmentPointsSource.GameplayPurchase,
                DevelopmentPointsManager.CreateTransactionId("screen:" + canvasKey), canvasKey.ToString());
        }

        OpenScreen(overlay, canvasKey, showMatchButton, null);
    }

    public void CloseCanvas(Canvas canvas)
    {
        int historyIndex = _history.FindLastIndex(entry => entry.Canvas == canvas);
        bool wasCurrent = historyIndex == _history.Count - 1;

        canvas.enabled = false;
        canvas.sortingOrder = 0;
        _openedCanvasses.Remove(canvas);
        if (historyIndex >= 0)
            _history.RemoveAt(historyIndex);

        if (wasCurrent)
            RestorePreviousScreen();
        else
            SetBackButton();
    }

    public void CloseCanvas()
    {
        if (_history.Count > 0)
            CloseCanvas(_history[_history.Count - 1].Canvas);
    }

    void RestorePreviousScreen()
    {
        if (_history.Count == 0)
        {
            SetBackButton();
            return;
        }

        NavigationEntry previous = _history[_history.Count - 1];
        previous.Canvas.enabled = true;
        previous.Configure();
        CurrentScreen = previous.Key;
        OnNewScreenOpened?.Invoke();
        OnScreenOpened?.Invoke(previous.Key);
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
        _backButton.onClick.RemoveAllListeners();

        bool teamSelected = GameManager.Instance != null && GameManager.Instance.GetTeamPickedStatus();
        NavigationEntry current = _history.Count > 0 ? _history[_history.Count - 1] : null;
        bool liveMatchIsCurrent = current != null && current.Key == CanvasKey.LiveMatch;
        bool hasPreviousScreen = _history.Count > 1;
        bool canReturnToMainMenu = teamSelected && current != null && current.Key != CanvasKey.MainMenu;
        bool canGoBack = !liveMatchIsCurrent && (hasPreviousScreen || canReturnToMainMenu);

        _backButton.gameObject.SetActive(canGoBack);
        _nextMatchButton?.SetVisible(current != null && current.ShowMatchButton);

        if (hasPreviousScreen && !liveMatchIsCurrent)
        {
            _backButton.onClick.AddListener(CloseCanvas);
        }
        else if (canReturnToMainMenu && !liveMatchIsCurrent)
        {
            _backButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    private void ReturnToMainMenu()
    {
        Team team = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID());
        GoToScreen(false, CanvasKey.MainMenu, team, true);
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
        _history.Clear();
    }
}
