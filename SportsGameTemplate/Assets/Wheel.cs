using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    [SerializeField] bool _isSpinning;
    [SerializeField] bool _isStopping;

    [SerializeField] float speed = 0.05f;

    void FixedUpdate()
    {
        if (_isSpinning)
        {
            LeanTween.rotateAroundLocal(gameObject, Vector3.forward, 3f, speed);
        }

        if (_isStopping)
        {
            _isSpinning = !_isStopping;

            speed += (0.05f * Time.deltaTime);
            LeanTween.rotateAroundLocal(gameObject, Vector3.forward, 3f, speed);

            if (speed > 0.5f)
            {
                _isStopping = false;
                _isSpinning = false;
            }
        }
    }
}
