using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

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

    private const int TIME_OUT = 3000; // Max wait per network call (ms)


    /// <summary>
    /// Call this to check if the player is online
    /// </summary>
    /// <param name="serverIP"></param>
    /// <param name="serverPort"></param>
    /// <returns></returns>

    private static async Task<bool> CheckIsOnline()
    {
        if (!Application.internetReachability.Equals(NetworkReachability.NotReachable) &&
            await IsPortReachableAsync(REFERENCE_HOST, REFERENCE_PORT, TIME_OUT))
        {
            // Debug.Log("Client appears Online!");
            return true;
        }
        else
        {
            // Debug.LogError("Client appears offline – no Internet connectivity detected.");
            return false;
        }
    }
    
    /// <summary>
    /// Call this to check if till need to reconnect with DS
    /// </summary>
    /// <param name="serverIP"></param>
    /// <param name="serverPort"></param>
    /// <returns> if true, client is offline or the server is online, false, server is offline dont reconnect anymore</returns>
    public static async Task<bool> CheckServerAsync(string serverIP, ushort serverPort)
    {
        if(await CheckIsOnline())
        {
            if (await IsPortReachableAsync(serverIP, serverPort, TIME_OUT))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
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
