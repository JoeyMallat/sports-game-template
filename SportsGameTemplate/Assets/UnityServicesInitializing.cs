using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class UnityServicesInitializing : MonoBehaviour
{
    async Task InitializeAsync()
    {
        // initialize handlers for unity game services
        await UnityServices.InitializeAsync();

        // authentication for managing environment information
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log("Succesfully logged in");
        Debug.Log(AuthenticationService.Instance.AccessToken);
    }

    async Task Awake()
    {
        // initialize Unity's authentication and core services, however check for internet connection
        // in order to fail gracefully without throwing exception if connection does not exist
        if (Utilities.CheckForInternetConnection())
        {
            await InitializeAsync();
        }
    }
}
