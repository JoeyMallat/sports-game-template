using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSkill
{
    [SerializeField] Skill _skill;
    [SerializeField] int _skillRating;

    public PlayerSkill(Skill skill, int rating)
    {
        _skill = skill;
        _skillRating = rating;
    }

    public Skill GetSkill()
    {
        return _skill;
    }

    public int GetRatingForSkill()
    {
        return _skillRating;
    }
}
