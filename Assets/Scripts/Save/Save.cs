using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using QFSW.QC;
using System;
using Unity.Services.CloudCode;

public static class Save
{
    //ONLY SERVER

    private const string ADD_PEARLS_ENDPOINT = "AddSavePlayerPearls";
    private const string GET_PEARLS_ENDPOINT = "GetPlayerPearls";

    private const string ADD_PEARLS_ARGUMENT_PLAYERID = "playerId";
    private const string ADD_PEARLS_ARGUMENT_PEARLS = "pearls";
    private const string ARGUMENT_PROJECT_ID = "gameProjectId";
    private const string PROJECT_ID = "01563be5-25e2-47ed-b519-012967e3d8e3";

    public static event Action OnPlayerPearlsChanged;

    [Command("save-addSavePlayerPearls")]
    public static async Task AddSavePlayerPearls(string userAuthId, int PlayerPearlsToAdd)
    {
        //Save to cloud

        var arguments = new Dictionary<string, object>
        {
            { ARGUMENT_PROJECT_ID, PROJECT_ID },
            { ADD_PEARLS_ARGUMENT_PEARLS, PlayerPearlsToAdd },
            { ADD_PEARLS_ARGUMENT_PLAYERID, userAuthId }
        };

        bool saved = false;

        while(!saved)
        {
            try
            {
                await CloudCodeService.Instance.CallEndpointAsync(ADD_PEARLS_ENDPOINT, arguments);
                saved = true;
                Debug.Log($"AddSavePlayerPearls");
                OnPlayerPearlsChanged?.Invoke();
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Error saving pearls: {e.Message}, trying again");
                await Task.Delay(100);
            }
        }
    }

    public static async Task<int> LoadPlayerPearls(string userAuthId)
    {
        var arguments = new Dictionary<string, object>
        {
            { ARGUMENT_PROJECT_ID, PROJECT_ID },
            { ADD_PEARLS_ARGUMENT_PLAYERID, userAuthId }
        };

        try
        {
            int loadPearls = await CloudCodeService.Instance.CallEndpointAsync<int>(GET_PEARLS_ENDPOINT, arguments);

            Debug.Log($"Player Pearls: {loadPearls}");
            return loadPearls;

        } catch (CloudCodeException e)
        {
            Debug.LogError($"Error loading pearls: {e.Message}, Closing Game");
            Application.Quit();
            return 0;
        }
    }
}
