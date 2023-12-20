using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionAnimation : MonoBehaviour
{
    public static TransitionAnimation Instance;

    [SerializeField] Image _backgroundBlocker;

    [SerializeField] GameObject _leftSide;
    [SerializeField] GameObject _rightSide;
    [SerializeField] GameObject _teamLogo;
    [SerializeField] Image _teamLogoImage;

    [SerializeField] AnimationCurve _sidesAnimationCurve;

    [SerializeField] float _startingDelay;
    [SerializeField] float _sidesMoveSpeed;
    [SerializeField] float _logoTurnSpeed;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void OnEnable()
    {
        //StartTransition(() => { });
        _backgroundBlocker.enabled = false;
    }

    public void SetTeamLogo()
    {
        _teamLogoImage.sprite = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetTeamLogo();
    }

    private void Update()
    {
        _teamLogoImage.transform.Rotate(new Vector3(0, 0, _logoTurnSpeed));
    }

    public void StartTransition(Action actionOnTransition)
    {
        SetTeamLogo();
        SetStartingState();
        LeanTween.moveLocal(_leftSide, new Vector3(-270, 0, 0), _sidesMoveSpeed).setEase(_sidesAnimationCurve).setDelay(_startingDelay);
        LeanTween.moveLocal(_rightSide, new Vector3(270, 0, 0), _sidesMoveSpeed).setEase(_sidesAnimationCurve).setDelay(_startingDelay);
        LeanTween.moveLocal(_teamLogo, new Vector3(0, 0, 0), _sidesMoveSpeed).setEase(_sidesAnimationCurve).setDelay(_startingDelay).setOnComplete(actionOnTransition); ;
        LeanTween.moveLocal(_leftSide, new Vector3(-895, 0, 0), _sidesMoveSpeed).setEase(_sidesAnimationCurve).setDelay(_startingDelay + _sidesMoveSpeed + _logoTurnSpeed);
        LeanTween.moveLocal(_rightSide, new Vector3(895, 0, 0), _sidesMoveSpeed).setEase(_sidesAnimationCurve).setDelay(_startingDelay + _sidesMoveSpeed + _logoTurnSpeed);
        LeanTween.moveLocal(_teamLogo, new Vector3(625, 0, 0), _sidesMoveSpeed).setEase(_sidesAnimationCurve).setDelay(_startingDelay + _sidesMoveSpeed + _logoTurnSpeed).setOnComplete(() => _backgroundBlocker.enabled = false);
    }

    public IEnumerator StartTransitionWithWaitForCompletion(Action actionOnTransition, IEnumerator waitForCompletion)
    {
        SetTeamLogo();
        SetStartingState();
        LeanTween.moveLocal(_leftSide, new Vector3(-270, 0, 0), _sidesMoveSpeed).setEase(_sidesAnimationCurve).setDelay(_startingDelay);
        LeanTween.moveLocal(_rightSide, new Vector3(270, 0, 0), _sidesMoveSpeed).setEase(_sidesAnimationCurve).setDelay(_startingDelay);
        LeanTween.moveLocal(_teamLogo, new Vector3(0, 0, 0), _sidesMoveSpeed).setEase(_sidesAnimationCurve).setDelay(_startingDelay);
        yield return new WaitForSeconds(_startingDelay + _sidesMoveSpeed);
        yield return StartCoroutine(waitForCompletion);
        actionOnTransition?.Invoke();
        LeanTween.moveLocal(_leftSide, new Vector3(-950, 0, 0), _sidesMoveSpeed).setEase(_sidesAnimationCurve).setDelay(_startingDelay + _sidesMoveSpeed + _logoTurnSpeed);
        LeanTween.moveLocal(_rightSide, new Vector3(950, 0, 0), _sidesMoveSpeed).setEase(_sidesAnimationCurve).setDelay(_startingDelay + _sidesMoveSpeed + _logoTurnSpeed);
        LeanTween.moveLocal(_teamLogo, new Vector3(700, 0, 0), _sidesMoveSpeed).setEase(_sidesAnimationCurve).setDelay(_startingDelay + _sidesMoveSpeed + _logoTurnSpeed).setOnComplete(() => _backgroundBlocker.enabled = false);
    }

    private void SetStartingState()
    {
        Debug.Log("called");
        _backgroundBlocker.enabled = true;
        LeanTween.moveLocal(_leftSide, new Vector3(-950, 0, 0), 0.001f);
        LeanTween.moveLocal(_rightSide, new Vector3(950, 0, 0), 0.001f);
        LeanTween.moveLocal(_teamLogo, new Vector3(-700, 0, 0), 0.001f);
    }
}
