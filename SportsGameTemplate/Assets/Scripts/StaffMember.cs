using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class StaffMember
{
    [SerializeField] [HideInInspector] bool _isSet;
    [SerializeField] int _portraitID;
    [SerializeField] float _increasePercentage;
    [SerializeField] BoostType _boostType;

    public StaffMember(float percentage, List<BoostType> possibleBoosts)
    {
        _isSet = false;
        _portraitID = Random.Range(0, 10);
        _increasePercentage = percentage;
        _boostType = possibleBoosts[Random.Range(0, possibleBoosts.Count)];
    }

    public string GetBoostTypeString()
    {
        return StaffSystem.Instance.GetBoostString(_boostType).Replace("{boost}", GetTruePercentage().ToString());
    }

    public Sprite GetPortrait()
    {
        return Resources.Load<Sprite>($"Faces/Staff/{_portraitID}");
    }

    public int GetPortraitID()
    {
        return _portraitID;
    }

    public float GetIncreasePercentage()
    {
        return _increasePercentage;
    }

    public int GetTruePercentage()
    {
        return Mathf.RoundToInt((_increasePercentage - 1) * 100);
    }

    public BoostType GetBoostType()
    {
        return _boostType;
    }

    public bool IsSet()
    {
        return _isSet;
    }

    public void Unset()
    {
        _isSet = false;
    }

    public void Set()
    {
        _isSet = true;
    }

    public void SetAndAssign(StaffMember staffMember)
    {
        _isSet = true;
        _portraitID = staffMember.GetPortraitID();
        _increasePercentage = staffMember.GetIncreasePercentage();
        _boostType = staffMember.GetBoostType();
    }
}
