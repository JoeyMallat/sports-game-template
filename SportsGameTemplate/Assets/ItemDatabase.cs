using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    List<GameItem> _gameItems;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        InitializeItemDatabase();
    }

    public GameItem GetGameItemByID(int id)
    {
        return _gameItems.Where(x => x.GetItemID() == id).ToList()[0];
    }

    private void InitializeItemDatabase()
    {
        _gameItems = new List<GameItem>();

        TextAsset itemFile = ConfigManager.Instance.GetCurrentConfig().ItemFile;

        string[] rows = itemFile.text.Split('\n');

        int id = 0;
        foreach (string row in rows)
        {
            string[] itemData = row.Split(',');
            int.TryParse(itemData[0], out int imageID);
            ItemType itemType = ParseItemType(itemData[1]);
            BallType ballType = ParseBallType(itemData[2]);
            string itemName = itemData[3];
            int lastIndex = itemData.Length;
            List<SkillBoost> skillBoosts = ParseSkillBoosts(itemData[4..lastIndex]);

            GameItem gameItem = new GameItem(itemType, ballType, id, imageID, itemName, skillBoosts);

            _gameItems.Add(gameItem);
            id++;
        }
    }

    public BallType ParseBallType(string ballTypeString)
    {
        return (BallType)Enum.Parse(typeof(BallType), ballTypeString);
    }

    public ItemType ParseItemType(string itemTypeString)
    {
        return (ItemType)Enum.Parse(typeof(ItemType), itemTypeString);
    }

    public List<SkillBoost> ParseSkillBoosts(string[] skillBoostsStrings)
    {
        List<SkillBoost> skillBoosts = new List<SkillBoost>();

        foreach (string boostString in skillBoostsStrings)
        {
            if (string.IsNullOrEmpty(boostString.Trim())) return skillBoosts;

            string skillString = boostString.Split(' ')[0].Trim();
            string boostAmountString = boostString.Split(' ')[1].Trim();

            Skill skill = (Skill)Enum.Parse(typeof(Skill), skillString);
            int.TryParse(boostAmountString.Replace("+", string.Empty), out int boostAmount);

            SkillBoost skillBoost = new SkillBoost(skill, boostAmount);
            skillBoosts.Add(skillBoost);
        }

        return skillBoosts;
    }

    public GameItem DecideReward(BallType rarity)
    {
        List<GameItem> possibleItems = _gameItems.Where(x => x.GetBallType() == rarity).ToList();
        return possibleItems[UnityEngine.Random.Range(0, possibleItems.Count)];
    }
}
