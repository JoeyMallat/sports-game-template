using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;
using UnityEngine;

public class CloudSaveManager : MonoBehaviour
{
    void Start()
    {
        GameManager.OnGemsUpdated += SaveAll;
        GameManager.OnInventoryUpdated += SaveAll;
        AuthenticationService.Instance.SignedIn += LoadAll;
        MM_OfficeView.OnFreeSpin += SetFreeSpinTimer;
    }

    public async Task<TimeObject> LoadTime()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            string data = await RetrieveSpecificData<string>("free_spin_timer");
            TimeObject timeObject = JsonUtility.FromJson<TimeObject>(data);
            return timeObject;
        } else
        {
            return null;
        }
    }

    private async void LoadAll()
    {
        CloudSaveData saveData = JsonUtility.FromJson<CloudSaveData>(await RetrieveSpecificData<CloudSaveData>("player_data"));

        if (saveData == null)
        {
            SaveAll(new CloudSaveData(GameManager.Instance.GetGems(), GameManager.Instance.GetItems()));
            return;
        }
        else
        {
            GameManager.Instance.SetInventory(saveData.Inventory);
            GameManager.Instance.SetGems(saveData.GemAmount);
        }
    }

    private async void SaveAll(CloudSaveData cloudSaveData)
    {
        await ForceSaveSingleData("player_data", cloudSaveData);
    }

    private async void SetFreeSpinTimer()
    {
        TimeObject timeObject = new TimeObject(DateTime.Now);
        await ForceSaveSingleData("free_spin_timer", timeObject);
    }

    private async Task ForceSaveSingleData(string key, object value)
    {
        try
        {
            Dictionary<string, object> oneElement = new Dictionary<string, object>();
            oneElement.Add(key, value);

            // Saving the data without write lock validation by passing the data as an object instead of a SaveItem
            Dictionary<string, string> result =
                await CloudSaveService.Instance.Data.Player.SaveAsync(oneElement);

            Debug.Log(
                $"Successfully saved {key}:{value} with updated write lock {result[key]}"
            );
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
    }

    private async Task<string> RetrieveSpecificData<T>(string key)
    {
        try
        {
            var results = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { key }
            );

            if (results.TryGetValue(key, out var item))
            {
                return item.Value.GetAsString();
            }
            else
            {
                Debug.Log($"There is no such key as {key}!");
            }
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogWarning(e);
        }

        return default;
    }
}

[System.Serializable]
public class TimeObject
{
    public int Month;
    public int Day;
    public int Hour;
    public int Minute;

    public TimeObject(DateTime time)
    {
        Month = time.Month;
        Day = time.Day;
        Hour = time.Hour;
        Minute = time.Minute;
    }

    public void SetTimeDetails(int month, int day, int hour, int minute)
    {
        Month = month;
        Day = day;
        Hour = hour;
        Minute = minute;
    }

    public (int, int) TimeDifference(DateTime time)
    {
        int monthDifference = time.Month - Month;
        int dayDifference = time.Day - Day;
        int hourDifference = time.Hour - Hour;
        int minuteDifference = time.Minute - Minute;
        
        if (monthDifference >= 1)
        {
            // If > month, return a big number
            return (99, 99);
        }

        return ((dayDifference * 24 + hourDifference), minuteDifference);
    }
}
