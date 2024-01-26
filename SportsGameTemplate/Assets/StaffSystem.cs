using System;
using System.Collections.Generic;
using UnityEngine;

public class StaffSystem : MonoBehaviour
{
    public static StaffSystem Instance;

    [SerializeField] StaffMember _coach;
    [SerializeField] StaffMember _scout;
    [SerializeField] StaffMember _mascot;
    [SerializeField] StaffMember _negotiator;

    [SerializeField] StaffMemberHired _coachHired;
    [SerializeField] StaffMemberHired _scoutHired;
    [SerializeField] StaffMemberHired _mascotHired;
    [SerializeField] StaffMemberHired _negotiatorHired;

    [SerializeField][TextArea(2, 3)] string _upgradeChanceString;
    [SerializeField][TextArea(2, 3)] string _scoutingPercentageString;
    [SerializeField][TextArea(2, 3)] string _gameBoostString;
    [SerializeField][TextArea(2, 3)] string _betterTradesString;
    [SerializeField][TextArea(2, 3)] string _betterShootingString;
    [SerializeField][TextArea(2, 3)] string _lowerSalaryString;

    public List<StaffMember> members = new List<StaffMember>();

    public List<StaffMemberItem> items;

    private void Awake()
    {
        StaffMemberItem.OnStaffHired += OnStaffMemberHire;
        GameManager.OnNewSeasonStarted += ResetStaff;

        ResetStaff();

        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(this);
        }
    }

    public string GetBoostString(BoostType boostType)
    {
        switch (boostType)
        {
            case BoostType.UpgradeChance:
                return _upgradeChanceString;
            case BoostType.ScoutingPercentage:
                return _scoutingPercentageString;
            case BoostType.GameBoost:
                return _gameBoostString;
            case BoostType.BetterTrades:
                return _betterTradesString;
            case BoostType.BetterShooting:
                return _betterShootingString;
            case BoostType.LowerSalary:
                return _lowerSalaryString;
        }

        return "";
    }

    public void GenerateStaffList(int role)
    {
        // role index
        // 0: coach
        // 1: scout
        // 2: mascot
        // 3: negotiator

        switch (role)
        {
            case 0:
                GenerateStaff(10, new List<BoostType>() { BoostType.UpgradeChance, BoostType.BetterShooting });

                for (int i = 0; i < members.Count; i++)
                {
                    items[i].gameObject.SetActive(true);
                    int index = i;
                    items[i].SetStaffDetails(members[index], 0);
                }
                break;
            case 1:
                GenerateStaff(10, new List<BoostType>() { BoostType.ScoutingPercentage });
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].gameObject.SetActive(true);
                    int index = i;

                    if (i < members.Count)
                    {
                        items[i].SetStaffDetails(members[index], 1);
                    } else
                    {
                        items[i].gameObject.SetActive(false);
                    }
                }
                break;
            case 2:
                GenerateStaff(10, new List<BoostType>() { BoostType.GameBoost, BoostType.BetterShooting });
                for (int i = 0; i < members.Count; i++)
                {
                    items[i].gameObject.SetActive(true);
                    int index = i;
                    items[i].SetStaffDetails(members[index], 2);
                }
                break;
            case 3:
                GenerateStaff(10, new List<BoostType>() { BoostType.BetterTrades, BoostType.LowerSalary });
                for (int i = 0; i < members.Count; i++)
                {
                    items[i].gameObject.SetActive(true);
                    int index = i;
                    items[i].SetStaffDetails(members[index], 3);
                }
                break;
            default:
                break;
        }

        Navigation.Instance.GoToScreen(true, CanvasKey.Staff);
    }

    private void ResetStaff()
    {
        _coach.Unset();
        _scout.Unset();
        _mascot.Unset();
        _negotiator.Unset();

        RefreshStaffMembers();
    }

    public AllStaff GetAllStaff()
    {
        return new AllStaff(_coach, _scout, _mascot, _negotiator);
    }

    public void SetStaffFromLoad(StaffMember coach, StaffMember scout, StaffMember mascot, StaffMember negotiator)
    {
        if (coach.IsSet())
            _coach.SetAndAssign(coach);
        if (scout.IsSet())
            _scout.SetAndAssign(scout);
        if (mascot.IsSet())
            _mascot.SetAndAssign(mascot);
        if (negotiator.IsSet())
            _negotiator.SetAndAssign(negotiator);

        RefreshStaffMembers();
    }

    private void OnStaffMemberHire(StaffMember member, int roleIndex)
    {
        switch (roleIndex)
        {
            case 0:
                _coach = member;
                _coach.Set();
                break;
            case 1:
                _scout = member;
                _scout.Set();
                break;
            case 2:
                _mascot = member;
                _mascot.Set();
                break;
            case 3:
                _negotiator = member;
                _negotiator.Set();
                break;
            default:
                break;
        }

        RefreshStaffMembers();
    }

    private void RefreshStaffMembers()
    {
        _coachHired.SetDetails(_coach);
        _scoutHired.SetDetails(_scout);
        _mascotHired.SetDetails(_mascot);
        _negotiatorHired.SetDetails(_negotiator);
    }

    private void GenerateStaff(int amount, List<BoostType> boosts)
    {
        members = new List<StaffMember>();

        if (boosts.Count == 1 && boosts.Contains(BoostType.ScoutingPercentage))
        {
            for (int i = 1; i < 6; i++)
            {
                members.Add(new StaffMember(1 + (i * 0.2f), boosts));
            }

            return;
        }
        

        for (int i = 0; i < amount; i++)
        {
            int index = i + 1;

            if (i < 2)
            {
                index = 2;
                members.Add(new StaffMember(1 + (index * 1.2f / 120f), boosts));
                continue;
            }

            members.Add(new StaffMember(1 + (index * 4f / 120f), boosts));
        }
    }

    public float GetUpgradeChanceBoost()
    {
        if (_coach.IsSet())
        {
            if (_coach.GetBoostType() == BoostType.UpgradeChance)
            {
                return _coach.GetIncreasePercentage();
            }
        }

        return 0;
    }

    public float GetShootingBoost()
    {
        float shootingBoost = 1f;

        if (_coach.IsSet() && _coach.GetBoostType() == BoostType.BetterShooting) shootingBoost += _coach.GetIncreasePercentage() - 1;
        if (_mascot.IsSet() && _mascot.GetBoostType() == BoostType.BetterShooting) shootingBoost += _mascot.GetIncreasePercentage() - 1;

        return shootingBoost;
    }

    public float GetGameBoost()
    {
        float gameBoost = 1f;

        if (_coach.IsSet() && _coach.GetBoostType() == BoostType.GameBoost) gameBoost += _coach.GetIncreasePercentage() - 1;
        if (_mascot.IsSet() && _mascot.GetBoostType() == BoostType.GameBoost) gameBoost += _mascot.GetIncreasePercentage() - 1;

        return gameBoost;
    }

    public float GetScoutingPercentage()
    {
        if (_scout.IsSet() && _scout.GetBoostType() == BoostType.ScoutingPercentage) return _scout.GetIncreasePercentage();

        return 0;
    }

    public float GetBetterTradePercentage()
    {
        if (_negotiator.IsSet() && _negotiator.GetBoostType() == BoostType.BetterTrades) return 2 - _negotiator.GetIncreasePercentage();

        return 1;
    }

    public float GetLowerSalaryPercentage()
    {
        if (_negotiator.IsSet() && _negotiator.GetBoostType() == BoostType.LowerSalary) return 2f - _negotiator.GetIncreasePercentage();

        return 1;
    }
}

[System.Serializable]
public class AllStaff
{
    public StaffMember Coach;
    public StaffMember Scout;
    public StaffMember Mascot;
    public StaffMember Negotiator;

    public AllStaff(StaffMember coach, StaffMember scout, StaffMember mascot, StaffMember negotiator)
    {
        Coach = coach;
        Scout = scout;
        Mascot = mascot;
        Negotiator = negotiator;
    }
}
