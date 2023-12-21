using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveGameButton : MonoBehaviour
{
    [SerializeField] int _saveGameIndex;
    [SerializeField] TextMeshProUGUI _teamName;
    [SerializeField] Image _teamLogo;

    public void SetSaveGameDetails(bool saveExists, string path)
    {
        Button button = GetComponent<Button>();
        LocalSaveManager localSaveManager = FindFirstObjectByType<LocalSaveManager>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => localSaveManager.SetFilePath(path));

        if (saveExists)
        {
            localSaveManager.SetFilePath(path);
            Team savedTeam = localSaveManager.LoadTeamDetails(path);
            Debug.Log($"Saved team on file path {path} is {savedTeam.GetTeamName()}");
            _teamName.text = savedTeam.GetTeamName();
            _teamLogo.sprite = savedTeam.GetTeamLogo();

            button.onClick.AddListener(() => localSaveManager.LoadGame(path));
            button.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, localSaveManager.LoadTeamDetails(path))));
        } else
        {
            Debug.Log($"No team found on savefile {path}");
            button.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.Setup)));
        }
    }
}
