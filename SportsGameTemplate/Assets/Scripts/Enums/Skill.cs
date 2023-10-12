using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Skill
{
    // Basketball Skills
    [InspectorName("Misc/Speed")] Speed,
    [InspectorName("Basketball/Short-range Shooting")] Short_Range_Shooting,
    [InspectorName("Basketball/Mid-range Shooting")] Mid_Range_Shooting,
    [InspectorName("Basketball/Three-point Shooting")] Three_Point_Shooting,
    [InspectorName("Basketball/Dunking")] Dunking,
    [InspectorName("Basketball/Layups")] Layups,
    [InspectorName("Misc/Stealing")] Stealing,
    [InspectorName("Misc/Passing")] Passing,
    [InspectorName("Basketball/Dribbling")] Dribbling,
    [InspectorName("Misc/Leadership")] Leadership,
    [InspectorName("Misc/Playmaking")] Playmaking,
    [InspectorName("Basketball/Rebounding")] Rebounding,
    [InspectorName("Basketball/Blocking")] Blocking,

    // Baseball Skills
    [InspectorName("Baseball/Power")] Power,
    [InspectorName("Baseball/Contact")] Contact,
    [InspectorName("Baseball/Bunting")] Bunting,
    [InspectorName("Baseball/Fielding")] Fielding,
    [InspectorName("Baseball/Accuracy")] Accuracy
}
