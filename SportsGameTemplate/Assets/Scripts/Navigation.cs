using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation : SerializedMonoBehaviour
{
    public static Navigation Instance;

    public Dictionary<CanvasKey, Canvas> CanvasDatabase;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void GoToScreen(Canvas canvas, Player player)
    {
        DisableAllCanvasses();

        canvas.enabled = true;

        ISettable settable = canvas.gameObject.GetComponent<ISettable>();
        
        if (settable != null)
        {
            settable.SetDetails(player);
        }
    }

    public void GoToScreen(Canvas canvas, Team team)
    {
        DisableAllCanvasses();

        canvas.enabled = true;

        ISettable settable = canvas.gameObject.GetComponent<ISettable>();
        
        if (settable != null)
        {
            settable.SetDetails(team);
        }
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

    private void DisableAllCanvasses()
    {
        Canvas[] canvasses = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Canvas canvas in canvasses)
        {
            canvas.enabled = false;
        }
    }
}
