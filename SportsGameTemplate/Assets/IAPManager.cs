using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : IDetailedStoreListener
{
    private IStoreController controller;
    private IExtensionProvider extensions;

    string _subscriptionID = "com.basketballgm.allstar";
    string _mvpID = "com.basketballgm.mvp";
    string _10gemsID = "com.basketballgm.gems10";
    string _100gemsID = "com.basketballgm.gems100";
    string _250gemsID = "com.basketballgm.gems250";
    string _500gemsID = "com.basketballgm.gems500";
    string _1000gemsID = "com.basketballgm.gems1000";
    string _10kgemsID = "com.basketballgm.gems10k";
    string _50kgemsID = "com.basketballgm.gems50k";
    string _100kgemsID = "com.basketballgm.gems100k";

    public static event Action<string> OnProductPurchased;

    public IAPManager()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder = AddProducts(builder);

        UnityPurchasing.Initialize(this, builder);
    }

    public IStoreController GetController()
    {
        return controller;
    }

    private ConfigurationBuilder AddProducts(ConfigurationBuilder builder)
    {
        builder.AddProduct(_subscriptionID, ProductType.Subscription);
        builder.AddProduct(_mvpID, ProductType.NonConsumable);
        builder.AddProduct(_10gemsID, ProductType.Consumable);
        builder.AddProduct(_100gemsID, ProductType.Consumable);
        builder.AddProduct(_250gemsID, ProductType.Consumable);
        builder.AddProduct(_500gemsID, ProductType.Consumable);
        builder.AddProduct(_1000gemsID, ProductType.Consumable);
        builder.AddProduct(_10kgemsID, ProductType.Consumable);
        builder.AddProduct(_50kgemsID, ProductType.Consumable);
        builder.AddProduct(_100kgemsID, ProductType.Consumable);

        return builder;
    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
        this.extensions = extensions;
    }

    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
    }

    /// <summary>
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        Debug.Log($"Purchased product with ID: {e.purchasedProduct.definition.id}");

        OnProductPurchased?.Invoke(e.purchasedProduct.definition.id);

        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        throw new System.NotImplementedException();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        throw new System.NotImplementedException();
    }
}
