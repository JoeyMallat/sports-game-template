using System;
using TMPro;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UI;

public class StaffMemberItem : MonoBehaviour
{
    [SerializeField] Image _staffImage;
    [SerializeField] TextMeshProUGUI _staffBoostText;
    [SerializeField] TextMeshProUGUI _hirePriceText;
    [SerializeField] Button _hireButton;

    public static event Action<StaffMember, int> OnStaffHired;

    public void SetStaffDetails(StaffMember staffMember, int populateIndex)
    {
        _staffImage.sprite = staffMember.GetPortrait();
        _staffBoostText.text = staffMember.GetBoostTypeString();
        _hirePriceText.text = CalculatePriceString(staffMember);
        _hireButton.onClick.RemoveAllListeners();

        _hireButton.onClick.AddListener(() => { if (GameManager.Instance.CheckBuyItem(CalculatePrice(staffMember))) { OnStaffHired?.Invoke(staffMember, populateIndex); Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID())); } else { Navigation.Instance.GoToScreen(true, CanvasKey.Store); } });
    }

    private string GetBoostText(BoostType boostType, float v)
    {
        return boostType.ToString() + " +" + v.ToString();
    }

    private string CalculatePriceString(StaffMember staffMember)
    {
        int price = CalculatePrice(staffMember);

        return $"Hire for <color=\"White\">{price} <sprite name=\"Gem\">";
    }

    private int CalculatePrice(StaffMember staffMember)
    {
        float boost = staffMember.GetIncreasePercentage();

        float normalizedBoost = (boost - 1f) * 10;

        int price = Mathf.Clamp(Mathf.RoundToInt(normalizedBoost * 2), 0, 99);

        return price;
    }
}
