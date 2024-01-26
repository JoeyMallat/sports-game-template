using System.Collections;
using TMPro;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UI;

public class RewardsView : MonoBehaviour, ISettable
{
    [SerializeField] float _secondsToWaitForTransitionToRewardsScreen;
    [SerializeField] float _secondsToWaitBetweenTextAnimations;

    [SerializeField] Image _ballItem;
    [SerializeField] Image _rewardImage;
    [SerializeField] TextMeshProUGUI _itemNameText;
    [SerializeField] TextMeshProUGUI _durationText;
    [SerializeField] TextMeshProUGUI _boostsText;

    [SerializeField] Button _spinAgainButton;
    [SerializeField] TextMeshProUGUI _spinAgainText;
    [SerializeField] Button _mainMenuButton;
    [SerializeField] Button _assignItemButton;

    private void Awake()
    {
        BallSystem.OnBallPicked += StartRewardAnimations;
    }

    private void StartRewardAnimations(BallItem pickedBall)
    {
        StartCoroutine(StartDelay(pickedBall));
    }

    private IEnumerator StartDelay(BallItem pickedBall)
    {
        yield return new WaitForSeconds(_secondsToWaitForTransitionToRewardsScreen);
        TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.Rewards, pickedBall));
    }

    public void SetDetails<T>(T item) where T : class
    {
        BallItem rewardTier = item as BallItem;

        _ballItem.sprite = rewardTier.GetBallImage();

        GameItem reward = ItemDatabase.Instance.DecideReward(rewardTier.GetBallType());

        _rewardImage.sprite = reward.GetItemImage();
        _itemNameText.text = reward.GetItemName();
        _durationText.text = $"{reward.GetGamesRemaining()} games";

        _boostsText.text = "";
        foreach (var boost in reward.GetSkillBoosts())
        {
            _boostsText.text += $"+{boost.GetBoost()} {boost.GetSkill().ToString().Replace("_", " ")}\n";
        }

        GameManager.Instance.AddItem(reward);

        SetButtons();
    }

    private void SetButtons()
    {
        _spinAgainButton.onClick.RemoveAllListeners();
        _mainMenuButton.onClick.RemoveAllListeners();
        _assignItemButton.onClick.RemoveAllListeners();

        int spinWheelAgainCost = RemoteConfigService.Instance.appConfig.GetInt("wheelspin_cost", 12);
        _spinAgainText.text = $"Spin again <color=\"white\">{spinWheelAgainCost} <sprite name=\"Gem\">";
        _spinAgainButton.onClick.AddListener(() => FindAnyObjectByType<BallSystem>().SetStartingState());
        _spinAgainButton.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.BallGame, spinWheelAgainCost)));

        _mainMenuButton.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()))));

        _assignItemButton.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(true, CanvasKey.Team, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()))));
    }
}
