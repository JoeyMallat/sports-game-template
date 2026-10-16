using NUnit.Framework;

public class TradePackageValueTests
{
    [Test]
    public void PlayerDecayIsOrderIndependent()
    {
        int forwardValue = TradeValueService.CalculatePackageValue(new[] { 1000, 800, 400 }, 0, 0.85f);
        int reverseValue = TradeValueService.CalculatePackageValue(new[] { 400, 800, 1000 }, 0, 0.85f);

        Assert.AreEqual(1969, forwardValue);
        Assert.AreEqual(forwardValue, reverseValue);
    }

    [Test]
    public void AdditionalPlayersDecayExponentially()
    {
        int packageValue = TradeValueService.CalculatePackageValue(new[] { 1000, 800, 400 }, 0, 0.85f);

        Assert.AreEqual(1000 + 680 + 289, packageValue);
    }

    [Test]
    public void NonPlayerValueRemainsAtFullValue()
    {
        int packageValue = TradeValueService.CalculatePackageValue(new[] { 1000, 800 }, 500, 0.85f);

        Assert.AreEqual(500 + 1000 + 680, packageValue);
    }
}
