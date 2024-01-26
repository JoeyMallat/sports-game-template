using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameItem
{
    [SerializeField] ItemType _itemType;
    [SerializeField] BallType _ballType;
    [SerializeField] int _itemID;
    [SerializeField] int _itemImageID;
    [SerializeField] string _itemName;
    [SerializeField] List<SkillBoost> _skillBoosts;
    [SerializeField] int _gamesRemaining;

    public GameItem(ItemType type, BallType ballType, int id, int imageID, string name, List<SkillBoost> skillBoosts)
    {
        _itemType = type;
        _ballType = ballType;
        _itemID = id;
        _itemImageID = imageID;
        _itemName = name;
        _skillBoosts = skillBoosts;
        _gamesRemaining = 50;
    }

    public string GetSkillBoostsString()
    {
        string text = "";
        foreach (SkillBoost boost in _skillBoosts)
        {
            text += $"<color=\"white\">+{boost.GetBoost()} {boost.GetSkill().ToString().Replace("_", " ")}</color>\n";
        }

        return text;
    }

    public int GetItemID()
    {
        return _itemID;
    }

    public int GetGamesRemaining()
    {
        return _gamesRemaining;
    }

    public BallType GetBallType()
    {
        return _ballType;
    }

    public ItemType GetItemType()
    {
        return _itemType;
    }

    public string GetItemName()
    {
        return _itemName;
    }

    public Sprite GetItemImage()
    {
        return Resources.Load<Sprite>($"Items/{_itemImageID}");
    }

    public List<SkillBoost> GetSkillBoosts()
    {
        return _skillBoosts;
    }
}

public enum ItemType
{
    Shoes,
    ArmSleeves,
    Sweatbands,
    Undershirt
}
