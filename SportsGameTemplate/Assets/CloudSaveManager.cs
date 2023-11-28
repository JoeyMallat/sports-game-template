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
    }

    private async void LoadAll()
    {
        CloudSaveData saveData = JsonUtility.FromJson<CloudSaveData>(await RetrieveSpecificData<CloudSaveData>("player_data"));

        if (saveData == null) return;

        GameManager.Instance.SetInventory(saveData.Inventory);
        GameManager.Instance.SetGems(saveData.GemAmount);
    }

    private async void SaveAll(CloudSaveData cloudSaveData)
    {
        await ForceSaveSingleData("player_data", cloudSaveData);
    }

    private async Task ForceSaveSingleData(string key, object value)
    {
        try
        {
            Dictionary<string, object> oneElement = new Dictionary<string, object>();
            oneElement.Add(key, value);
            Debug.Log(value.ToString());

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
            Debug.LogError(e);
        }

        return default;
    }
}
