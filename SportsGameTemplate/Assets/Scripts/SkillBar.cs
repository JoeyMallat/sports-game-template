using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillBar : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _skillTitleText;
    [SerializeField] Image _skillBarFill;

    public void SetSkillBar(PlayerSkill skill)
    {
        _skillTitleText.text = $"{skill.GetSkill()}   <b><color=\"white\">{skill.GetRatingForSkill()}</color></b>";
        _skillBarFill.fillAmount = skill.GetRatingForSkill() / 99f;
    }

    public void SetSkillBar(string skill, int rating)
    {
        _skillTitleText.text = $"{skill}   <b><color=\"white\">{rating}</color></b>";
        _skillBarFill.fillAmount = rating / 99f;
    }
}
