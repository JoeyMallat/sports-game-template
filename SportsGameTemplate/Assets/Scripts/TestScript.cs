using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public PositionConfig PositionConfig;

    private void Start()
    {
        foreach (Position position in PositionConfig.GetPositions())
        {
            Debug.Log(position.GetPositionName());
        }
    }
}
