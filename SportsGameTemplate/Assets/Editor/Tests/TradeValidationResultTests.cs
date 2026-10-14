using NUnit.Framework;

public class TradeSimulationReportTests
{
    [Test]
    public void EmptyReportHasZeroRates()
    {
        TradeSimulationReport report = new TradeSimulationReport();

        Assert.AreEqual(0f, report.OfferGenerationRate);
        Assert.AreEqual(0f, report.ValidOfferRate);
        Assert.AreEqual(0f, report.AveragePackageSize);
    }

    [Test]
    public void ReportCalculatesRates()
    {
        TradeSimulationReport report = new TradeSimulationReport
        {
            Attempts = 10,
            OffersGenerated = 5,
            ValidOffers = 4,
            TotalAssets = 15,
            TotalValueRatio = 5f
        };

        Assert.AreEqual(0.5f, report.OfferGenerationRate);
        Assert.AreEqual(0.8f, report.ValidOfferRate);
        Assert.AreEqual(3f, report.AveragePackageSize);
        Assert.AreEqual(1f, report.AverageValueRatio);
    }
}
