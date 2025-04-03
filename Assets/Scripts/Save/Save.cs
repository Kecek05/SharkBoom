using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using QFSW.QC;
using System;
using Unity.Services.CloudCode;

public static class Save
{

    public static event Action OnPlayerPearlsChanged;

    [Command("save-addSavePlayerPearls")]
    public static async Task AddSavePlayerPearls(string userAuthId, int PlayerPearlsToAdd)
    {
        //Save to cloud

        var arguments = new Dictionary<string, object>
        {
            { CloudCodeRefs.ARGUMENT_PROJECT_ID, CloudCodeRefs.PROJECT_ID },
            { CloudCodeRefs.ADD_PEARLS_ARGUMENT_PEARLS, PlayerPearlsToAdd },
            { CloudCodeRefs.ARGUMENT_PLAYERID, userAuthId }
        };

        bool saved = false;

        while(!saved)
        {
            try
            {
                await CloudCodeService.Instance.CallEndpointAsync(CloudCodeRefs.ADD_PEARLS_ENDPOINT, arguments);
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
            { CloudCodeRefs.ARGUMENT_PROJECT_ID, CloudCodeRefs.PROJECT_ID },
            { CloudCodeRefs.ARGUMENT_PLAYERID, userAuthId }
        };

        try
        {
            int loadPearls = await CloudCodeService.Instance.CallEndpointAsync<int>(CloudCodeRefs.GET_PEARLS_ENDPOINT, arguments);

            Debug.Log($"Player Pearls: {loadPearls}");
            return loadPearls;

        } catch (CloudCodeException e)
        {
            Debug.LogError($"Error loading pearls: {e.Message}, Closing Game");
            Application.Quit();
            return 0;
        }
    }

    public static async Task<string> LoadPlayerName(string userAuthId)
    {
        var arguments = new Dictionary<string, object>
        {
            { CloudCodeRefs.ARGUMENT_PROJECT_ID, CloudCodeRefs.PROJECT_ID },
            { CloudCodeRefs.ARGUMENT_PLAYERID, userAuthId }
        };
        try
        {
            string playerName = await CloudCodeService.Instance.CallEndpointAsync<string>(CloudCodeRefs.GET_PLAYER_NAME_ENDPOINT, arguments);
            Debug.Log($"Player Name: {playerName}");
            return playerName;
        }
        catch (CloudCodeException e)
        {
            Debug.LogError($"Error loading player name: {e.Message}, Closing Game");
            Application.Quit();
            return "";
        }
    }

    [Command("save-setPlayerName")]
    public static async Task SavePlayerName(string userAuthId, string playerName)
    {
        var arguments = new Dictionary<string, object>
        {
            { CloudCodeRefs.ARGUMENT_PROJECT_ID, CloudCodeRefs.PROJECT_ID },
            { CloudCodeRefs.ARGUMENT_PLAYERID, userAuthId },
            { CloudCodeRefs.SET_PLAYER_NAME_ARGUMENT_NAME, playerName }
        };

        bool saved = false;

        while (!saved)
        {
            try
            {
                await CloudCodeService.Instance.CallEndpointAsync(CloudCodeRefs.SET_PLAYER_NAME_ENDPOINT, arguments);
                saved = true;
                Debug.Log($"Saved Player Name");
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Error saving name: {e.Message}, trying again");
                await Task.Delay(100);
            }
        }
    }

}
