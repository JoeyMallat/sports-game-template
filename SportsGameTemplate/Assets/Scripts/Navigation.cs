using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Navigation : SerializedMonoBehaviour
{
    public static Navigation Instance;

    [SerializeField] Button _backButton;

    Canvas _currentCanvas;

    public Dictionary<CanvasKey, Canvas> CanvasDatabase;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void GoToScreen(bool overlay, bool showBack, CanvasKey canvasKey, Player player)
    {
        _currentCanvas = GetCanvas(canvasKey);
        Canvas canvas = GetCanvas(canvasKey);

        if (!overlay)
            DisableAllCanvasses();

        canvas.enabled = true;

        ISettable settable = canvas.gameObject.GetComponent<ISettable>();
        
        if (settable != null)
        {
            settable.SetDetails(player);
        }

        if (showBack)
        {
            SetBackButton(overlay, canvasKey);
            _backButton.gameObject.SetActive(true);
        }
        else
        {
            _backButton.gameObject.SetActive(false);
        }
    }

    public void GoToScreen(bool overlay, bool showBack, CanvasKey canvasKey, Team team)
    {
        _currentCanvas = GetCanvas(canvasKey);
        Canvas canvas = GetCanvas(canvasKey);

        if (!overlay)
            DisableAllCanvasses();

        canvas.enabled = true;

        ISettable settable = canvas.gameObject.GetComponent<ISettable>();
        
        if (settable != null)
        {
            settable.SetDetails(team);
        }

        if (showBack){
            SetBackButton(overlay, canvasKey);
            _backButton.gameObject.SetActive(true);
        }
        else
        {
            _backButton.gameObject.SetActive(false);
        }
    }

    public void GoToScreen(bool overlay, CanvasKey canvasKey, Team team)
    {
        _currentCanvas = GetCanvas(canvasKey);
        Canvas canvas = GetCanvas(canvasKey);

        if (!overlay)
            DisableAllCanvasses();

        canvas.enabled = true;

        ISettable settable = canvas.gameObject.GetComponent<ISettable>();
        
        if (settable != null)
        {
            settable.SetDetails(team);
        }
    }

    public void GoToScreen(bool overlay, bool showBack, CanvasKey canvasKey, List<Team> teams)
    {
        _currentCanvas = GetCanvas(canvasKey);
        Canvas canvas = GetCanvas(canvasKey);

        if (!overlay)
            DisableAllCanvasses();

        canvas.enabled = true;

        ISettable settable = canvas.gameObject.GetComponent<ISettable>();

        if (settable != null)
        {
            settable.SetDetails(teams);
        }

        if (showBack){
            SetBackButton(overlay, canvasKey);
            _backButton.gameObject.SetActive(true);
        }
        else
        {
            _backButton.gameObject.SetActive(false);
        }
    }

    public void GoToScreen(bool overlay, bool showBack, CanvasKey canvasKey)
    {
        _currentCanvas = GetCanvas(canvasKey);
        Canvas canvas = GetCanvas(canvasKey);

        if (!overlay)
            DisableAllCanvasses();

        canvas.enabled = true;

        if (showBack)
        {
            SetBackButton(overlay, canvasKey);
            _backButton.gameObject.SetActive(true);
        }
        else
        {
            _backButton.gameObject.SetActive(false);
        }
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

    private void SetBackButton(bool wasOverlay, CanvasKey canvasKey)
    {
        _backButton.onClick.RemoveAllListeners();

        if (wasOverlay)
        {
            _backButton.onClick.AddListener(() => CloseCanvas(_currentCanvas));
        }

        if (canvasKey != CanvasKey.MainMenu)
        {
            _backButton.onClick.AddListener(() => _backButton.gameObject.SetActive(false));
        } else
        {
            _backButton.onClick.AddListener(() => _backButton.gameObject.SetActive(true));
        }
    }

    private void DisableAllCanvasses()
    {
        Canvas[] canvasses = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Canvas canvas in canvasses)
        {
            canvas.enabled = false;
        }

        // Always enable back button canvas
        _backButton.GetComponentInParent<Canvas>().enabled = true;
    }
}
