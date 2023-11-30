using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public static ColorManager Instance;
    public Color DisabledButtonColor;
    public Color DisabledButtonTextColor;
    public Color EnabledButtonColor;
    public Color EnabledButtonTextColor;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(this);
        }
    }
}
