using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using UnityEngine;

public static class Reconnect
{
    private const int MATCH_DURATION = 420; // Match duration in seconds (7 minutes)
    
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
            Loader.Load(Loader.Scene.NoNetwork);
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
                Debug.LogError($"Error setting is in match: {e.Message}, trying again");
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
                Debug.LogError($"Error setting is in match: {e.Message}, trying again");
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
            Loader.Load(Loader.Scene.NoNetwork);
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
            Loader.Load(Loader.Scene.NoNetwork);
            return 0;
        }
    }

    public static async Task SetMatchEndTime(string userAuthId)
    {
        //Save to cloud
        double endMatchTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + MATCH_DURATION;
        
        var arguments = new Dictionary<string, object>
        {
            { CloudCodeRefs.ARGUMENT_PROJECT_ID, CloudCodeRefs.PROJECT_ID },
            { CloudCodeRefs.SET_PLAYER_END_MATCH_TIME_ARGUMENT_MATCH_TIME, endMatchTime },
            { CloudCodeRefs.ARGUMENT_PLAYERID, userAuthId },
        };

        bool setted = false;

        while (!setted)
        {
            try
            {
                await CloudCodeService.Instance.CallEndpointAsync(CloudCodeRefs.SET_PLAYER_END_MATCH_TIME_ENDPOINT, arguments);
                setted = true;
                Debug.Log($"Setted Match End time to: {endMatchTime}");
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Error setting end match time: {e.Message}, trying again");
                await Task.Delay(100);
            }
        }
    }

    public static async Task<bool> CanRejoinInMatch(string userAuthId)
    {
        var arguments = new Dictionary<string, object>
        {
            { CloudCodeRefs.ARGUMENT_PROJECT_ID, CloudCodeRefs.PROJECT_ID },
            { CloudCodeRefs.ARGUMENT_PLAYERID, userAuthId }
        };
        try
        {
            double endMatchTime = await CloudCodeService.Instance.CallEndpointAsync<double>(CloudCodeRefs.GET_PLAYER_END_MATCH_TIME_ENDPOINT, arguments);
            Debug.Log($"endMatchTime: {endMatchTime}");
            
            double now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (now > endMatchTime)
            {
                Debug.Log("Match already over, cant rejoin");
                return false;
            }
            else
            {
                Debug.Log("Match in progress, rejoin!");
                return true;
            }
        }
        catch (CloudCodeException e)
        {
            Debug.LogError($"Error getting endMatchTime: {e.Message}, assuming match expired");
            return false;
        }
    }

    /// <summary>
    /// Comprehensive check to determine if a player can safely reconnect to a match
    /// This includes time validation and server health checks
    /// </summary>
    /// <param name="userAuthId">Player's authentication ID</param>
    /// <returns>True if player can reconnect, false otherwise</returns>
    public static async Task<bool> CanSafelyReconnect(string userAuthId)
    {
        try
        {
            // First check if player is marked as in match
            if (!await GetIsInMatch(userAuthId))
            {
                Debug.Log("Player not marked as in match");
                return false;
            }

            // Check if match hasn't expired
            if (!await CanRejoinInMatch(userAuthId))
            {
                Debug.Log("Match has expired, clearing reconnect flag");
                await SetIsInMatch(userAuthId, false);
                return false;
            }

            // Get server connection details
            string ipMatch = await GetIpMatch(userAuthId);
            int portMatch = await GetPortMatch(userAuthId);

            // Check if server is healthy
            if (!await ServerConnectionTester.CheckGameServerHealth(ipMatch, portMatch))
            {
                Debug.Log("Game server is not responsive, clearing reconnect flag");
                await SetIsInMatch(userAuthId, false);
                return false;
            }

            Debug.Log("All checks passed, player can safely reconnect");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during reconnect validation: {e.Message}");
            // In case of any error, clear the reconnect flag to prevent infinite loops
            try
            {
                await SetIsInMatch(userAuthId, false);
            }
            catch (Exception clearError)
            {
                Debug.LogError($"Failed to clear reconnect flag: {clearError.Message}");
            }
            return false;
        }
    }

}
