using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DevelopmentPointsManagerTests
{
    GameObject _gameObject;
    DevelopmentPointsManager _manager;

    [SetUp]
    public void SetUp()
    {
        _gameObject = new GameObject("DevelopmentPointsManagerTests");
        _manager = _gameObject.AddComponent<DevelopmentPointsManager>();
        _manager.RestoreSaveData(new DevelopmentPointsSaveData
        {
            Balance = 0,
            ScopeId = "test-scope",
            Transactions = new List<DevelopmentPointsTransaction>(),
            ProcessedTransactionIds = new List<string>()
        });
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_gameObject);
    }

    [TestCase(true, false, 40)]
    [TestCase(false, false, 10)]
    [TestCase(true, true, 60)]
    [TestCase(false, true, 15)]
    public void MatchRewardsUseRequiredValuesAndPremiumMultiplier(bool victory, bool premium, int expected)
    {
        Assert.AreEqual(expected, _manager.CalculateMatchReward(victory, premium));
    }

    [Test]
    public void AwardAndSpendPublishDetailedChanges()
    {
        DevelopmentPointsChange earned = default;
        DevelopmentPointsChange spent = default;
        DevelopmentPointsManager.OnDevelopmentPointsEarned += CaptureEarned;
        DevelopmentPointsManager.OnDevelopmentPointsSpent += CaptureSpent;

        try
        {
            Assert.IsTrue(_manager.Award(100, DevelopmentPointsSource.Achievement, "achievement:test"));
            Assert.IsTrue(_manager.Spend(35, DevelopmentPointsSource.FacilityUpgrade, "facility:test"));

            Assert.AreEqual(0, earned.PreviousBalance);
            Assert.AreEqual(100, earned.NewBalance);
            Assert.AreEqual(100, earned.AmountChanged);
            Assert.AreEqual("achievement:test", earned.TransactionId);
            Assert.AreEqual(100, spent.PreviousBalance);
            Assert.AreEqual(65, spent.NewBalance);
            Assert.AreEqual(-35, spent.AmountChanged);
            Assert.AreEqual(DevelopmentPointsSource.FacilityUpgrade, spent.Source);
        }
        finally
        {
            DevelopmentPointsManager.OnDevelopmentPointsEarned -= CaptureEarned;
            DevelopmentPointsManager.OnDevelopmentPointsSpent -= CaptureSpent;
        }

        void CaptureEarned(DevelopmentPointsChange change) => earned = change;
        void CaptureSpent(DevelopmentPointsChange change) => spent = change;
    }

    [Test]
    public void DuplicateTransactionIsRejected()
    {
        Assert.IsTrue(_manager.Award(40, DevelopmentPointsSource.MatchWin, "match:test"));
        Assert.IsFalse(_manager.Award(40, DevelopmentPointsSource.MatchWin, "match:test"));
        Assert.AreEqual(40, _manager.CurrentBalance);
    }

    [Test]
    public void InvalidAndUnaffordableSpendsCannotCreateNegativeBalance()
    {
        Assert.IsFalse(_manager.Spend(1, DevelopmentPointsSource.CoachingStaff, "staff:test"));
        Assert.IsFalse(_manager.Spend(-1, DevelopmentPointsSource.CoachingStaff, "staff:negative"));
        Assert.AreEqual(0, _manager.CurrentBalance);
    }

    [Test]
    public void AwardRejectsIntegerOverflow()
    {
        _manager.RestoreSaveData(new DevelopmentPointsSaveData { Balance = int.MaxValue - 5, ScopeId = "overflow" });

        Assert.IsFalse(_manager.Award(10, DevelopmentPointsSource.InAppPurchase, "iap:overflow"));
        Assert.AreEqual(int.MaxValue - 5, _manager.CurrentBalance);
    }

    [Test]
    public void SaveRestorePreservesBalanceHistoryAndIdempotency()
    {
        Assert.IsTrue(_manager.Award(75, DevelopmentPointsSource.WeeklyObjective, "weekly:test"));
        DevelopmentPointsSaveData saveData = _manager.CaptureSaveData();

        _manager.RestoreSaveData(saveData);

        Assert.AreEqual(75, _manager.CurrentBalance);
        Assert.AreEqual(1, _manager.GetTransactionHistory().Count);
        Assert.IsFalse(_manager.Award(75, DevelopmentPointsSource.WeeklyObjective, "weekly:test"));
    }
}
