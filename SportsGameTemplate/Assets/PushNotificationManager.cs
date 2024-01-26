using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.PushNotifications;
using UnityEngine;

public class PushNotificationManager : MonoBehaviour
{
    async void Start()
    {
        await UnityServices.InitializeAsync();
        AnalyticsService.Instance.StartDataCollection();

        try
        {
            PushNotificationsService.Instance.OnNotificationReceived += notificationData =>
            {
                Debug.Log("Received a notification!");
                foreach (KeyValuePair<string, object> item in notificationData)
                {
                    Debug.Log($"Notification data item: {item.Key} - {item.Value}");
                }
            };

            Debug.Log("Attempting to Register for Remote Notifications");
            string pushToken = await PushNotificationsService.Instance.RegisterForPushNotificationsAsync();
            Debug.Log($"Notification Token Received = {pushToken}");
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to retrieve a push notification token. :: {e.Message}");
        }
    }
}
