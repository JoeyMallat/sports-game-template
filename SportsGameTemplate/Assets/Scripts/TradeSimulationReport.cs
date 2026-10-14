public class TradeSimulationReport
{
    public int Attempts { get; set; }
    public int OffersGenerated { get; set; }
    public int ValidOffers { get; set; }
    public int PlayerAssets { get; set; }
    public int DraftPickAssets { get; set; }
    public int TotalAssets { get; set; }
    public float TotalValueRatio { get; set; }

    public float OfferGenerationRate => Attempts == 0 ? 0f : OffersGenerated / (float)Attempts;
    public float ValidOfferRate => OffersGenerated == 0 ? 0f : ValidOffers / (float)OffersGenerated;
    public float AveragePackageSize => OffersGenerated == 0 ? 0f : TotalAssets / (float)OffersGenerated;
    public float AverageValueRatio => OffersGenerated == 0 ? 0f : TotalValueRatio / OffersGenerated;

    public override string ToString()
    {
        return $"Attempts {Attempts}, offers {OffersGenerated}, valid {ValidOfferRate:P0}, average assets {AveragePackageSize:0.00}, average value ratio {AverageValueRatio:0.00}, players {PlayerAssets}, picks {DraftPickAssets}";
    }
}
