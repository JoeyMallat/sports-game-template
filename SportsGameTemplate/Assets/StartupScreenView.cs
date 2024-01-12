using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class StartupScreenView : MonoBehaviour, ISettable
{
    [SerializeField] string _saveFilePath;

    [SerializeField] List<SaveGameButton> _loadButtons;
    [SerializeField] Button _storeButton;

    private async void Start()
    {
        await LeagueSystem.Instance.ReadTeamsFromFile(true);
        SetDetails("empty");
    }

    public void SetDetails<T>(T item) where T : class
    {
        // Load saves and get chosen team
        List<(bool, string)> filePaths = GetSaveFilePaths();

        // Set loaded savegames and button to load
        for (int i = 0; i < _loadButtons.Count; i++)
        {
            int index = i;
            _loadButtons[i].SetSaveGameDetails(filePaths[index].Item1, filePaths[index].Item2);
        }

        // Set store link
        _storeButton.onClick.RemoveAllListeners();
        _storeButton.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(true, CanvasKey.Store)));
    }

    private List<(bool, string)> GetSaveFilePaths()
    {
        List<(bool, string)> files = new List<(bool, string)> ();
        for (int i = 0; i < _loadButtons.Count; i++)
        {
            if (File.Exists(Application.persistentDataPath + _saveFilePath + $"_{i}"))
            {
                files.Add((true, Application.persistentDataPath + _saveFilePath + $"_{i}"));
            } else
            {
                files.Add((false, Application.persistentDataPath + _saveFilePath + $"_{i}"));
            }
        }

        return files;
    }

    public void GoToUrl(string url)
    {
        Application.OpenURL(url);
    }
}
