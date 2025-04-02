using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using UnityEngine;

public static class Reconnect
{
    //Is In Match
    private const string GET_ISINMATCH_ENDPOINT = "GetIsInMatch";
    private const string SET_ISINMATCH_ENDPOINT = "SetIsInMatch";
    private const string ARGUMENT_ISINMATCH = "isInMatch";

    //Player Match Connection
    private const string SET_PLAYER_MATCH_CONNECTION_ENDPOINT = "SetPlayerMatchConnection";
    private const string ARGUMENT_IP = "ipServer";
    private const string ARGUMENT_PORT = "portServer";

    //Get Match Connection
    private const string GET_PLAYER_PORT_SERVER_ENDPOINT = "GetPlayerPortServer";
    private const string GET_PLAYER_IP_SERVER_ENDPOINT = "GetPlayerIpServer";

    //Generic
    private const string ARGUMENT_PLAYERID = "playerId";
    private const string ARGUMENT_PROJECT_ID = "gameProjectId";
    private const string PROJECT_ID = "01563be5-25e2-47ed-b519-012967e3d8e3";


    public static async Task<bool> GetIsInMatch(string userAuthId)
    {
        var arguments = new Dictionary<string, object>
        {
            { ARGUMENT_PROJECT_ID, PROJECT_ID },
            { ARGUMENT_PLAYERID, userAuthId }
        };
        try
        {
            bool isInMatch = await CloudCodeService.Instance.CallEndpointAsync<bool>(GET_ISINMATCH_ENDPOINT, arguments);
            Debug.Log($"Is In Match: {isInMatch}");
            return isInMatch;
        }
        catch (CloudCodeException e)
        {
            Debug.LogError($"Error getting IsInMatch: {e.Message}, Closing Game");
            Application.Quit();
            return false;
        }
    }

    //public static async Task AddSavePlayerPearls(string userAuthId, int PlayerPearlsToAdd)
    //{
    //    //Save to cloud

    //    var arguments = new Dictionary<string, object>
    //    {
    //        { ARGUMENT_PROJECT_ID, PROJECT_ID },
    //        { ADD_PEARLS_ARGUMENT_PEARLS, PlayerPearlsToAdd },
    //        { ADD_PEARLS_ARGUMENT_PLAYERID, userAuthId }
    //    };

    //    bool saved = false;

    //    while (!saved)
    //    {
    //        try
    //        {
    //            await CloudCodeService.Instance.CallEndpointAsync(ADD_PEARLS_ENDPOINT, arguments);
    //            saved = true;
    //            Debug.Log($"AddSavePlayerPearls");
    //            OnPlayerPearlsChanged?.Invoke();
    //        }
    //        catch (CloudCodeException e)
    //        {
    //            Debug.LogError($"Error saving pearls: {e.Message}, trying again");
    //            await Task.Delay(100);
    //        }
    //    }
    //}



    //public static async Task<int> LoadPlayerPearls(string userAuthId)
    //{
    //    var arguments = new Dictionary<string, object>
    //    {
    //        { ARGUMENT_PROJECT_ID, PROJECT_ID },
    //        { ADD_PEARLS_ARGUMENT_PLAYERID, userAuthId }
    //    };

    //    try
    //    {
    //        int loadPearls = await CloudCodeService.Instance.CallEndpointAsync<int>(GET_PEARLS_ENDPOINT, arguments);

    //        Debug.Log($"Player Pearls: {loadPearls}");
    //        return loadPearls;

    //    }
    //    catch (CloudCodeException e)
    //    {
    //        Debug.LogError($"Error loading pearls: {e.Message}, Closing Game");
    //        Application.Quit();
    //        return 0;
    //    }
    //}
}
