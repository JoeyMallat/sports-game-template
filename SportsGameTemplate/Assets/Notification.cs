using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
    public static Notification Instance { get; private set; }

    [SerializeField] TextMeshProUGUI _notificationText;
    [SerializeField] Image _notificationColor;

    [SerializeField] Color _successColor;
    [SerializeField] Color _warningColor;
    [SerializeField] Color _reminderColor;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void ShowNotification(string notificationText, NotificationType notificationType, int seconds)
    {
        StartCoroutine(SetAndShowNotification(notificationText, notificationType, seconds));
    }

    private IEnumerator SetAndShowNotification(string notificationText, NotificationType notificationType, int seconds)
    {
        _notificationText.text = notificationText;

        switch (notificationType)
        {
            case NotificationType.Success:
                _notificationColor.color = _successColor;
                break;
            case NotificationType.Warning:
                _notificationColor.color = _warningColor;
                break;
            case NotificationType.Reminder:
                _notificationColor.color = _reminderColor;
                break;
            default:
                break;
        }

        LeanTween.value(gameObject, (value) =>
        {
            _notificationColor.color = value;
            _notificationText.color = new Color(1, 1, 1, value.a);
        }, new Color(_notificationColor.color.r, _notificationColor.color.g, _notificationColor.color.b, 0),
            new Color(_notificationColor.color.r, _notificationColor.color.g, _notificationColor.color.b, 1f),
            0.2f);

        yield return new WaitForSeconds(seconds);

        LeanTween.value(gameObject, (value) =>
        {
            _notificationColor.color = value;
            _notificationText.color = new Color(1, 1, 1, value.a);
        },
        new Color(_notificationColor.color.r, _notificationColor.color.g, _notificationColor.color.b, 1f),
        new Color(_notificationColor.color.r, _notificationColor.color.g, _notificationColor.color.b, 0),
            0.2f);
    }
}

public enum NotificationType
{
    Success,
    Warning,
    Reminder
}
