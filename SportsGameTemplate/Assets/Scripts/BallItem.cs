using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BallItem : MonoBehaviour
{
    [SerializeField] BallType _ballType;
    [SerializeField] float _oddsOnAppearing;

    float _leftBoundX;

    float _ballSpeed;
    float _originalSpeed;

    bool _spinning;
    bool _stopping;

    public static event Action<BallItem, Vector3> OnBallStopped;
    public static event Action<BallItem> OnBallHitBounds;

    public BallType GetBallType()
    {
        return _ballType;
    }

    public float GetOdds()
    {
        return _oddsOnAppearing;
    }

    public float GetXPosition()
    {
        return transform.position.x;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void SetProperties(float speed, float leftBoundX)
    {
        SetSpeed(speed);
        _leftBoundX = leftBoundX;
    }

    private void SetSpeed(float speed)
    {
        _originalSpeed = speed;
        _ballSpeed = speed;
    }

    public void StartSpin()
    {
        _spinning = true;
    }

    public void StopSpin()
    {
        _stopping = true;
    }

    private void Update()
    {
        MoveBall();
        StopBall();
        CheckForOutOfBounds();
    }

    private void MoveBall()
    {
        if (_spinning && _ballSpeed > 0.05f)
        {
            transform.position = new Vector3(transform.position.x - _ballSpeed * Time.deltaTime, 0, 0);
        }
    }

    private void StopBall()
    {
        if (_stopping)
        {
            _ballSpeed -= Mathf.Lerp(1f, .5f, _ballSpeed / _originalSpeed) * Time.deltaTime;

            if (_ballSpeed <= 0.051f)
            {
                _ballSpeed = 0;
                _spinning = false;
                _stopping = false;
                OnBallStopped?.Invoke(this, transform.position);
            }
        }
    }

    private void CheckForOutOfBounds()
    {
        if (transform.position.x <= _leftBoundX)
        {
            OnBallHitBounds?.Invoke(this);
        }
    }

    public void MoveInstantlyToNewPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
}

public enum BallType
{
    Ruby,
    Gold,
    Silver
}
