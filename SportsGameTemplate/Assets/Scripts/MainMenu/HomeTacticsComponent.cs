using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeTacticsComponent : MonoBehaviour, ISettable
{
    private static readonly HashSet<Skill> AttackingSkills = new HashSet<Skill>
    {
        Skill.Short_Range_Shooting,
        Skill.Mid_Range_Shooting,
        Skill.Three_Point_Shooting,
        Skill.Dunking,
        Skill.Layups,
        Skill.Passing,
        Skill.Dribbling,
        Skill.Playmaking
    };

    private static readonly HashSet<Skill> DefensiveSkills = new HashSet<Skill>
    {
        Skill.Speed,
        Skill.Stealing,
        Skill.Rebounding,
        Skill.Blocking
    };

    [SerializeField] Button _button;
    [SerializeField] TextMeshProUGUI _mainTacticText;
    [SerializeField] TextMeshProUGUI _defensiveRatingText; //Format: 99 Def
    [SerializeField] TextMeshProUGUI _attackingRatingText; //Format: 99 Off

    public void SetDetails<T>(T item, bool showMatchButton = false) where T : class
    {
        Team team = item as Team;

        if (team == null)
        {
            return;
        }

        TacticsSettings tacticsSettings = FindFirstObjectByType<TacticsSettings>(FindObjectsInactive.Include);
        int attackingRating = CalculateRating(team.GetPlayersFromTeam(), AttackingSkills);
        int defensiveRating = CalculateRating(team.GetPlayersFromTeam(), DefensiveSkills);
        string mainTactic = tacticsSettings != null ? tacticsSettings.GetMainTacticName() : "Balanced";

        _mainTacticText.text = CapitalizeFirstLetter(mainTactic);
        _attackingRatingText.text = $"{attackingRating} Off";
        _defensiveRatingText.text = $"{defensiveRating} Def";

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Tactics, team, showMatchButton));
        }
    }

    private string CapitalizeFirstLetter(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return char.ToUpperInvariant(value[0]) + value.Substring(1);
    }

    private int CalculateRating(List<Player> players, HashSet<Skill> includedSkills)
    {
        int totalRating = 0;
        int skillCount = 0;

        foreach (Player player in players)
        {
            foreach (PlayerSkill playerSkill in player.GetSkills())
            {
                if (!includedSkills.Contains(playerSkill.GetSkill()))
                {
                    continue;
                }

                totalRating += player.GetRatingForSkillWithItem(playerSkill);
                skillCount++;
            }
        }

        return skillCount > 0 ? Mathf.RoundToInt(totalRating / (float)skillCount) : 0;
    }
}
