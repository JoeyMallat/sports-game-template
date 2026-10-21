/// <summary>
/// Examples for integrating gameplay systems with the DP ledger. Claim keys must be stable identifiers
/// supplied by the originating system; retries with the same key are intentionally rejected.
/// </summary>
public static class DevelopmentPointsIntegrationExamples
{
    public static bool ClaimAchievement(string achievementId, int reward)
    {
        return DevelopmentPointsManager.Instance.Award(
            reward,
            DevelopmentPointsSource.Achievement,
            "achievement:" + achievementId,
            achievementId);
    }

    public static bool ClaimDailyObjective(string objectiveClaimId, int reward)
    {
        return DevelopmentPointsManager.Instance.Award(
            reward,
            DevelopmentPointsSource.DailyObjective,
            "daily-objective:" + objectiveClaimId,
            objectiveClaimId);
    }

    public static bool ClaimWeeklyObjective(string objectiveClaimId, int reward)
    {
        return DevelopmentPointsManager.Instance.Award(
            reward,
            DevelopmentPointsSource.WeeklyObjective,
            "weekly-objective:" + objectiveClaimId,
            objectiveClaimId);
    }

    public static bool UpgradeTrainingCenter(int targetLevel)
    {
        DevelopmentPointsManager manager = DevelopmentPointsManager.Instance;
        int cost = manager.Settings != null ? manager.Settings.TrainingCenterCost : 500;
        return manager.Spend(cost, DevelopmentPointsSource.FacilityUpgrade,
            $"facility:{manager.CareerId}:training:{targetLevel}", "Training Center upgrade");
    }

    public static bool UpgradeMedicalCenter(int targetLevel)
    {
        DevelopmentPointsManager manager = DevelopmentPointsManager.Instance;
        int cost = manager.Settings != null ? manager.Settings.MedicalCenterCost : 500;
        return manager.Spend(cost, DevelopmentPointsSource.FacilityUpgrade,
            $"facility:{manager.CareerId}:medical:{targetLevel}", "Medical Center upgrade");
    }

    public static bool UpgradeScoutingCenter(int targetLevel)
    {
        DevelopmentPointsManager manager = DevelopmentPointsManager.Instance;
        int cost = manager.Settings != null ? manager.Settings.ScoutingCenterCost : 500;
        return manager.Spend(cost, DevelopmentPointsSource.FacilityUpgrade,
            $"facility:{manager.CareerId}:scouting:{targetLevel}", "Scouting Center upgrade");
    }

    public static bool RenewCoach(string coachId, int renewalSequence)
    {
        DevelopmentPointsManager manager = DevelopmentPointsManager.Instance;
        int cost = manager.Settings != null ? manager.Settings.RenewCoachCost : 200;
        return manager.Spend(cost, DevelopmentPointsSource.CoachingStaff,
            $"coach-renewal:{manager.CareerId}:{coachId}:{renewalSequence}", "Renew coaching staff");
    }

    public static bool PurchaseDraftScoutingReport(string draftClassId, string playerId)
    {
        DevelopmentPointsManager manager = DevelopmentPointsManager.Instance;
        int cost = manager.Settings != null ? manager.Settings.DraftScoutingReportCost : 100;
        return manager.Spend(cost, DevelopmentPointsSource.DraftScoutingReport,
            $"scouting-report:{manager.CareerId}:{draftClassId}:{playerId}", "Draft scouting report");
    }
}
