using System;

public static class RandomIDGenerator
{
    public static string GenerateRandomID()
    {
        return Guid.NewGuid().ToString();
    }
}
