using NUnit.Framework;

public class TradeValueResultTests
{
    [Test]
    public void HigherTalentProducesHigherValue()
    {
        TradeValueResult lowerTalent = new TradeValueResult(1000f);
        TradeValueResult higherTalent = new TradeValueResult(2000f);

        Assert.Greater(higherTalent.TotalValue, lowerTalent.TotalValue);
    }

    [Test]
    public void MultipliersCompoundDeterministically()
    {
        TradeValueResult result = new TradeValueResult(1000f, 1.2f, 1.1f, 0.9f, 1.15f, 1.05f);

        Assert.AreEqual(1435, result.TotalValue);
    }

    [Test]
    public void TradeValueNeverFallsBelowOne()
    {
        TradeValueResult result = new TradeValueResult(0f, 0f, 0f, 0f, 0f, 0f);

        Assert.AreEqual(1, result.TotalValue);
    }
}
