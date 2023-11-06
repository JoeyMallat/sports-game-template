using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillBar : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _skillTitleText;
    [SerializeField] Image _skillBarFill;
    [SerializeField] Image _skillBarSecondaryFill;

    public void SetSkillBar(PlayerSkill skill)
    {
        _skillBarSecondaryFill.enabled = false;
        _skillTitleText.text = $"{skill.GetSkill().ToString().Replace("_", " ")}   <b><color=\"white\">{skill.GetRatingForSkill()}</color></b>";
        _skillBarFill.fillAmount = skill.GetRatingForSkill() / 99f;
    }

    public void SetSkillBar(string skill, int rating)
    {
        _skillBarSecondaryFill.enabled = false;
        _skillTitleText.text = $"{skill.Replace("_", " ")}   <b><color=\"white\">{rating}</color></b>";
        _skillBarFill.fillAmount = rating / 99f;
    }

    public void SetSkillBar(string skill, int rating, int boost)
    {
        _skillBarSecondaryFill.enabled = true;
        _skillTitleText.text = $"{skill.Replace("_", " ")}   <b><color=\"white\">{Mathf.Clamp(rating + boost, 0, 99)} (+{boost})</color></b>";
        _skillBarFill.fillAmount = rating / 99f;
        _skillBarSecondaryFill.fillAmount = (rating + boost) / 99f;
    }


    public void SetSkillBarWithRange(string skill, int minRating, int maxRating)
    {
        _skillBarSecondaryFill.enabled = true;
        _skillTitleText.text = $"{skill.Replace("_", " ")}   <b><color=\"white\">{minRating} - {maxRating}</color></b>";
        _skillBarFill.fillAmount = minRating / 99f;
        _skillBarSecondaryFill.fillAmount = maxRating / 99f;
    }
}
