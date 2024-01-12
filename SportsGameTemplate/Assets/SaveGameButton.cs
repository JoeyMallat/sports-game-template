using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SaveGameButton : MonoBehaviour
{
    [SerializeField] int _saveGameIndex;
    [SerializeField] TextMeshProUGUI _teamName;
    [SerializeField] Image _teamLogo;
    [SerializeField] Button _overwriteButton;

    public void SetSaveGameDetails(bool saveExists, string path)
    {
        Button button = GetComponent<Button>();
        LocalSaveManager localSaveManager = FindFirstObjectByType<LocalSaveManager>();
        button.onClick.RemoveAllListeners();
        _overwriteButton.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => localSaveManager.SetFilePath(path));

        if (saveExists)
        {
            localSaveManager.SetFilePath(path);
            Team savedTeam = localSaveManager.LoadTeamDetails(path + "_preview");
            Debug.Log($"Saved team on file path {path} is {savedTeam.GetTeamName()}");
            _teamName.text = savedTeam.GetTeamName();
            _teamLogo.sprite = savedTeam.GetTeamLogo();

            button.onClick.AddListener(async () => { await localSaveManager.LoadGame(path); TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(savedTeam.GetTeamID()))); } );
            _overwriteButton.gameObject.SetActive(true);
            _overwriteButton.onClick.AddListener(() => localSaveManager.SetFilePath(path));
            _overwriteButton.onClick.AddListener(async () => await LeagueSystem.Instance.ReadTeamsFromFile(false));
            _overwriteButton.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.Setup)));
        } else
        {
            _overwriteButton.gameObject.SetActive(false);
            Debug.Log($"No team found on savefile {path}");
            button.onClick.AddListener(async () => await LeagueSystem.Instance.ReadTeamsFromFile(false));
            button.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.Setup)));
        }
    }
}
