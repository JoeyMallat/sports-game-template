using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Position File", menuName = "Config/Position File")]
public class Position : ScriptableObject
{
    [SerializeField] string _positionName;
    [Range(0, 10)][SerializeField] int _amountInStartingTeam;
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

        return total;
    }
}
