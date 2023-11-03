using System.Collections;
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

    public GameItem(ItemType type, BallType ballType, int id, int imageID, string name, List<SkillBoost> skillBoosts)
    {
        _itemType = type;
        _ballType = ballType;
        _itemID = id;
        _itemImageID = imageID;
        _itemName = name;
        _skillBoosts = skillBoosts;
    }

    public BallType GetBallType()
    {
        return _ballType;
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
    Glasses,
    Sweatbands,
    Undershirt
}
