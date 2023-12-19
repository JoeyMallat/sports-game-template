using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Sirenix.Serialization;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;
using UnityEngine;

public class CloudSaveManager : MonoBehaviour
{
    void Start()
    {
        GameManager.OnPremiumStatusUpdated += SaveAll;
        GameManager.OnGemsUpdated += SaveAll;
        GameManager.OnInventoryUpdated += SaveAll;
        AuthenticationService.Instance.SignedIn += LoadAll;
        //AuthenticationService.Instance.SignedIn += SetFirstFreeSpinTimer;
        MM_OfficeView.OnFreeSpin += SetFreeSpinTimer;
    }

    public async Task<TimeObject> LoadTime()
    {
        try
        {
            string data = await RetrieveSpecificData<string>("free_spin_timer");
            TimeObject loadedTime = JsonUtility.FromJson<TimeObject>(data);
            DateTime.TryParse(loadedTime.FreeSpinTimeString, out DateTime freeSpinTime);
            Debug.Log(freeSpinTime);
            TimeObject timeObject = new TimeObject(freeSpinTime);
            return timeObject;
        } catch
        {
            Debug.LogWarning("Free Spin Timer not yet retrieved. Trying again.");
            return null;
        }
    }

    private async void LoadAll()
    {
        CloudSaveData saveData = JsonUtility.FromJson<CloudSaveData>(await RetrieveSpecificData<CloudSaveData>("player_data"));

        if (saveData == null)
        {
            SaveAll(new CloudSaveData(GameManager.Instance.GetSalaryCapIncrease(), GameManager.Instance.GetPremiumStatus(), GameManager.Instance.GetGems(), GameManager.Instance.GetItems()));
            return;
        }
        else
        {
            GameManager.Instance.SetSalaryCapIncrease(saveData.SalaryCapIncrease);
            GameManager.Instance.SetPremiumStatus(saveData.PremiumStatus);
            GameManager.Instance.SetInventory(saveData.Inventory);
            GameManager.Instance.SetGems(saveData.GemAmount);
        }
    }

    private async void SaveAll(CloudSaveData cloudSaveData)
    {
        await ForceSaveSingleData("player_data", cloudSaveData);
    }

    private async void SetFreeSpinTimer(DateTime timeToSet)
    {
        TimeObject timeObject = new TimeObject(timeToSet);
        Debug.Log($"New time set: {timeToSet}");
        await ForceSaveSingleData("free_spin_timer", timeObject);
    }

    private async void SetFirstFreeSpinTimer()
    {
        TimeObject timeObject = new TimeObject(DateTime.MinValue);
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

                if (key == "free_spin_timer")
                {
                    SetFirstFreeSpinTimer();
                    return await RetrieveSpecificData<string>("free_spin_timer");
                }
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
    public string FreeSpinTimeString;
    public DateTime FreeSpinTime;

    public TimeObject(DateTime time)
    {
        FreeSpinTime = time;
        FreeSpinTimeString = time.ToString();
    }
}
