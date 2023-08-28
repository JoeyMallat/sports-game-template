using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class RandomIDGenerator
{
    public static string GenerateRandomID()
    {
        return Guid.NewGuid().ToString();
    }
}
