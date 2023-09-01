using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ScheduleGenerator
{
    public List<Match> GenerateSchedule(List<Team> teams);
}
