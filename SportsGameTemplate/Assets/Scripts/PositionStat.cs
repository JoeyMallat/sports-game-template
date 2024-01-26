using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class PositionStat
{
    [SerializeField] Skill _skillName;
    [PropertyRange(0f, 1f)][SerializeField] float _skillWeightForPosition;

    public float GetSkillWeight() { return _skillWeightForPosition; }
    public Skill GetSkill() { return _skillName; }
}
