using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillBoost
{
    [SerializeField] Skill _skill;
    [SerializeField] int _boost;

    public SkillBoost(Skill skill, int boost)
    {
        _skill = skill;
        _boost = boost;
    }

    public Skill GetSkill()
    {
        return _skill;
    }

    public int GetBoost()
    {
        return _boost;
    }
}
