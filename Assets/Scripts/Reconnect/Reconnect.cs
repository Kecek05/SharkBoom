using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using UnityEngine;

public static class Reconnect
{
    public static async Task<bool> GetIsInMatch(string userAuthId)
    {
        var arguments = new Dictionary<string, object>
        {
            { CloudCodeRefs.ARGUMENT_PROJECT_ID, CloudCodeRefs.PROJECT_ID },
            { CloudCodeRefs.ARGUMENT_PLAYERID, userAuthId }
        };
        try
        {
            bool isInMatch = await CloudCodeService.Instance.CallEndpointAsync<bool>(CloudCodeRefs.GET_ISINMATCH_ENDPOINT, arguments);
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
            { CloudCodeRefs.ARGUMENT_PROJECT_ID, CloudCodeRefs.PROJECT_ID },
            { CloudCodeRefs.ARGUMENT_ISINMATCH, isInMatch },
            { CloudCodeRefs.ARGUMENT_PLAYERID, userAuthId }
        };

        bool setted = false;

        while (!setted)
        {
            try
            {
                await CloudCodeService.Instance.CallEndpointAsync(CloudCodeRefs.SET_ISINMATCH_ENDPOINT, arguments);
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
            { CloudCodeRefs.ARGUMENT_PROJECT_ID, CloudCodeRefs.PROJECT_ID },
            { CloudCodeRefs.ARGUMENT_IP, ip },
            { CloudCodeRefs.ARGUMENT_PLAYERID, userAuthId },
            { CloudCodeRefs.ARGUMENT_PORT, port }
        };

        bool setted = false;

        while (!setted)
        {
            try
            {
                await CloudCodeService.Instance.CallEndpointAsync(CloudCodeRefs.SET_PLAYER_MATCH_CONNECTION_ENDPOINT, arguments);
                setted = true;
                Debug.Log($"Setted Match Connection: IP: {ip} - PORT: {port}");
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
            { CloudCodeRefs.ARGUMENT_PROJECT_ID, CloudCodeRefs.PROJECT_ID },
            { CloudCodeRefs.ARGUMENT_PLAYERID, userAuthId }
        };
        try
        {
            string ipMatch = await CloudCodeService.Instance.CallEndpointAsync<string>(CloudCodeRefs.GET_PLAYER_IP_SERVER_ENDPOINT, arguments);
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
            { CloudCodeRefs.ARGUMENT_PROJECT_ID, CloudCodeRefs.PROJECT_ID },
            { CloudCodeRefs.ARGUMENT_PLAYERID, userAuthId }
        };
        try
        {
            int portMatch = await CloudCodeService.Instance.CallEndpointAsync<int>(CloudCodeRefs.GET_PLAYER_PORT_SERVER_ENDPOINT, arguments);
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
