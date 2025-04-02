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

    public static async Task SetIsInMatch(string userAuthId, bool isInMatch)
    {
        //Save to cloud

        var arguments = new Dictionary<string, object>
        {
            { ARGUMENT_PROJECT_ID, PROJECT_ID },
            { ARGUMENT_ISINMATCH, isInMatch },
            { ARGUMENT_PLAYERID, userAuthId }
        };

        bool setted = false;

        while (!setted)
        {
            try
            {
                await CloudCodeService.Instance.CallEndpointAsync(SET_ISINMATCH_ENDPOINT, arguments);
                setted = true;
                Debug.Log($"Setted is in game");
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Error saving pearls: {e.Message}, trying again");
                await Task.Delay(100);
            }
        }
    }

    public static async Task SetPlayerMatchConnection(string userAuthId, string ip, int port)
    {
        //Save to cloud

        var arguments = new Dictionary<string, object>
        {
            { ARGUMENT_PROJECT_ID, PROJECT_ID },
            { ARGUMENT_IP, ip },
            { ARGUMENT_PLAYERID, userAuthId },
            { ARGUMENT_PORT, port }
        };

        bool setted = false;

        while (!setted)
        {
            try
            {
                await CloudCodeService.Instance.CallEndpointAsync(SET_PLAYER_MATCH_CONNECTION_ENDPOINT, arguments);
                setted = true;
                Debug.Log($"Setted is in game");
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Error saving pearls: {e.Message}, trying again");
                await Task.Delay(100);
            }
        }
    }

    public static async Task<string> GetIpMatch(string userAuthId)
    {
        var arguments = new Dictionary<string, object>
        {
            { ARGUMENT_PROJECT_ID, PROJECT_ID },
            { ARGUMENT_PLAYERID, userAuthId }
        };
        try
        {
            string ipMatch = await CloudCodeService.Instance.CallEndpointAsync<string>(GET_PLAYER_IP_SERVER_ENDPOINT, arguments);
            Debug.Log($"Ip Match: {ipMatch}");
            return ipMatch;
        }
        catch (CloudCodeException e)
        {
            Debug.LogError($"Error getting Ip Match: {e.Message}, Closing Game");
            Application.Quit();
            return "NoIp";
        }
    }

    public static async Task<int> GetPortMatch(string userAuthId)
    {
        var arguments = new Dictionary<string, object>
        {
            { ARGUMENT_PROJECT_ID, PROJECT_ID },
            { ARGUMENT_PLAYERID, userAuthId }
        };
        try
        {
            int portMatch = await CloudCodeService.Instance.CallEndpointAsync<int>(GET_PLAYER_PORT_SERVER_ENDPOINT, arguments);
            Debug.Log($"Port Match: {portMatch}");
            return portMatch;
        }
        catch (CloudCodeException e)
        {
            Debug.LogError($"Error getting IsInMatch: {e.Message}, Closing Game");
            Application.Quit();
            return 0;
        }
    }

}
