using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallSystem : MonoBehaviour
{
    [SerializeField] SpritePulse _crosshair;
    [SerializeField] Transform _ballParent;
    [SerializeField] Vector3 _firstBallPosition;
    [SerializeField] float _distanceBetweenBalls;
    [SerializeField] int _ballsToSpawn;
    [SerializeField] float _ballSpeed;

    float _totalOdds;
    [SerializeField] List<BallItem> _ballItems;

    List<BallItem> _spawnedBalls;
    BallItem _closestBall;

    bool _moveToWinningBall;
    [SerializeField] float _timeToMoveToWinningBall;
    float _elapsedTime;
    Vector3 _goalPosition;

    public static event Action<BallItem> OnBallPicked;

    private void Awake()
    {
        BallItem.OnBallStopped += CheckPickedBall;
        BallItem.OnBallHitBounds += MoveBallToBack;
    }

    private void CheckPickedBall(BallItem ball, Vector3 ballPosition)
    {
        if (_closestBall == null) _closestBall = ball;

        if (Mathf.Abs(ballPosition.x) < _distanceBetweenBalls)
        {
            if (Mathf.Abs(ballPosition.x) < Mathf.Abs(_closestBall.GetPosition().x))
            {
                _closestBall = ball;
                Debug.Log(_closestBall);
                _moveToWinningBall = true;
                _goalPosition = -_closestBall.GetPosition();
            }
        }
    }

    private void MoveBallToBack(BallItem ballItem)
    {
        Vector3 lastPosition = _spawnedBalls.Last().transform.position;
        ballItem.MoveInstantlyToNewPosition(lastPosition + new Vector3(_distanceBetweenBalls, 0, 0));
        _spawnedBalls.Remove(ballItem);
        _spawnedBalls.Add(ballItem);
    }

    void Start()
    {
        _spawnedBalls = new List<BallItem>();

        _ballItems.ForEach(x => _totalOdds += x.GetOdds());

        SpawnBalls();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StopBalls();
        }

        if (_closestBall != null && _moveToWinningBall)
        {
            _elapsedTime += Time.deltaTime;

            float interpolatedRatio = _elapsedTime / _timeToMoveToWinningBall;

            transform.position = Vector3.Lerp(transform.position, _goalPosition, interpolatedRatio);

            if (Vector3.Distance(transform.position, _goalPosition) < 0.05f)
            {
                _moveToWinningBall = false;
                transform.position = _goalPosition;

                _elapsedTime = _timeToMoveToWinningBall;

                OnBallPicked?.Invoke(_closestBall);
                _crosshair.TogglePulse();
            }
        }
    }

    private void SpawnBalls()
    {
        for (int i = 0; i < _ballsToSpawn; i++)
        {
            BallItem ballItem = DecideBallToSpawn();
            BallItem newBall = Instantiate(ballItem, new Vector3(_firstBallPosition.x + i * _distanceBetweenBalls, 0, 0), Quaternion.identity, _ballParent);
            newBall.SetProperties(_ballSpeed, -_distanceBetweenBalls * (Mathf.RoundToInt(_ballsToSpawn * 0.1f)));
            newBall.StartSpin();
            _spawnedBalls.Add(newBall);
        }
    }

    private BallItem DecideBallToSpawn()
    {
        float random = UnityEngine.Random.Range(0f, _totalOdds);

        foreach (var ball in _ballItems)
        {
            if (random > ball.GetOdds())
            {
                random -= ball.GetOdds();
                continue;
            } else
            {
                return ball;
            }
        }

        return null;
    }

    public void StopBalls()
    {
        _spawnedBalls.ForEach(x => x.StopSpin());
        _crosshair.TogglePulse();
    }
}
