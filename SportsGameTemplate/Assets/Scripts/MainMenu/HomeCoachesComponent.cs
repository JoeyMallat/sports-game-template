using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeCoachesComponent : MonoBehaviour, ISettable
{
    [SerializeField] Button _button;
    [SerializeField] TextMeshProUGUI _amountOfCoachesHiredText; //Format: 3/3
    [SerializeField] Image _coachingBadge; //Not yet implemented

    public void SetDetails<T>(T item) where T : class
    {
        AllStaff staff = item as AllStaff;

        if (staff == null)
        {
            return;
        }

        int hiredStaff = 0;
        if (staff.Coach.IsSet()) hiredStaff++;
        if (staff.Scout.IsSet()) hiredStaff++;
        if (staff.Mascot.IsSet()) hiredStaff++;
        if (staff.Negotiator.IsSet()) hiredStaff++;

        _amountOfCoachesHiredText.text = $"{hiredStaff}/4";

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => StaffSystem.Instance.GenerateStaffList(0));
        }
    }
}
