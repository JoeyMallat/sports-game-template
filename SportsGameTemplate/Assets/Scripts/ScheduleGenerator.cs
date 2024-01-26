using System.Collections.Generic;

public interface ScheduleGenerator
{
    public List<Match> GenerateSchedule(List<Team> teams);
}
