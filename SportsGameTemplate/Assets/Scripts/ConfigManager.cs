using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance;

    [SerializeField] Config _configFile;

    public Config GetCurrentConfig()
    {
        return _configFile;
    }

    private void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(this); }

        Debug.Log($"Current loaded config: {_configFile.SportsType}");
    }
}
