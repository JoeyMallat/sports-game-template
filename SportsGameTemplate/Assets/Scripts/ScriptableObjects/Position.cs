using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Position File", menuName = "Config/Position File")]
public class Position : ScriptableObject
{
    [SerializeField] string _positionName;
    [Range(0, 10)][SerializeField] int _amountInStartingTeam;
    [ReadOnly][SerializeField] float _totalWeightOfSkills;
    [InfoBox("Position weights total is too high!", "@GetTotalOfWeights() > 1f", InfoMessageType = InfoMessageType.Warning)][SerializeField] List<PositionStat> _positionStats;

    public string GetPositionName()
    {
        return _positionName;
    }
    public float GetTotalOfWeights()
    {
        float total = 0;

        foreach (var item in _positionStats)
        {
            total += item.GetSkillWeight();
        }

        _totalWeightOfSkills = total;
        return total;
    }

    public int CalculateAverageRatingForPosition(List<PlayerSkill> playerSkills)
    {
        float rating = 0;
        foreach (PositionStat position in _positionStats)
        {
            foreach (PlayerSkill playerSkill in playerSkills)
            {
                if (position.GetSkill() == playerSkill.GetSkill())
                {
                    rating += position.GetSkillWeight() * playerSkill.GetRatingForSkill();
                }
            }
        }
        return Mathf.RoundToInt(rating);
    }

    public List<PositionStat> GetPositionStats() { return _positionStats; }

    public int GetAmountInStartingTeam()
    {
        return _amountInStartingTeam;
    }
}
