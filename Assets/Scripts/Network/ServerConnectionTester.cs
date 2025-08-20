using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public class ServerConnectionTester
{
    /// <summary>
    /// Const of Google DNS to check if the player has internet. (IP)
    /// </summary>
    private const string REFERENCE_HOST = "8.8.8.8";

    /// <summary>
    /// Const of Google DNS to check if the player has internet. (Port)
    /// </summary>
    private const ushort REFERENCE_PORT = 53;

    private const int TIME_OUT = 6000; // Max wait per network call (ms)


    /// <summary>
    /// Call this to check if the player is online
    /// </summary>
    /// <param name="serverIP"></param>
    /// <param name="serverPort"></param>
    /// <returns></returns>

    public static async Task<bool> CheckIsOnline()
    {
        if (!Application.internetReachability.Equals(NetworkReachability.NotReachable) &&
            await IsPortReachableAsync(REFERENCE_HOST, REFERENCE_PORT, TIME_OUT))
        {
            Debug.Log("Client appears Online!");
            return true;
        }
        else
        {
            Debug.LogError("Client appears offline – no Internet connectivity detected.");
            return false;
        }
    }

    /// <summary>
    /// Check if a specific game server is reachable and responsive
    /// </summary>
    /// <param name="serverIP">The IP address of the game server</param>
    /// <param name="serverPort">The port of the game server</param>
    /// <returns>True if server is reachable, false otherwise</returns>
    public static async Task<bool> CheckGameServerHealth(string serverIP, int serverPort)
    {
        if (string.IsNullOrEmpty(serverIP) || serverIP == "NoIp" || serverPort <= 0)
        {
            Debug.LogWarning($"Invalid server connection details: IP={serverIP}, Port={serverPort}");
            return false;
        }

        try
        {
            Debug.Log($"Checking game server health: {serverIP}:{serverPort}");
            bool isReachable = await IsPortReachableAsync(serverIP, serverPort, TIME_OUT);
            
            if (isReachable)
            {
                Debug.Log($"Game server {serverIP}:{serverPort} is responsive");
            }
            else
            {
                Debug.LogWarning($"Game server {serverIP}:{serverPort} is not responsive");
            }
            
            return isReachable;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error checking game server health {serverIP}:{serverPort}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Lightweight TCP probe – true if <paramref name="host"/>:<paramref name="port"/> accepts a socket within <paramref name="timeout"/> ms.
    /// </summary>
    private static async Task<bool> IsPortReachableAsync(string host, int port, int timeout)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            var finished = await Task.WhenAny(connectTask, Task.Delay(timeout));
            return finished == connectTask && client.Connected;
        }
        catch
        {
            return false;
        }
    }
    
}
