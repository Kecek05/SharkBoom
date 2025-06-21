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

    public static async Task<bool> CheckIsOnline()
    {
        if (!Application.internetReachability.Equals(NetworkReachability.NotReachable) &&
            await IsPortReachableAsync(REFERENCE_HOST, REFERENCE_PORT, TIME_OUT))
        {
            return true;
        }
        else
        {
            Debug.LogError("Client appears offline – no Internet connectivity detected.");
            return false;
        }
    }
    
    public async Task<ConnectResult> CheckServerAsync(string serverIP, ushort serverPort)
    {
        if(await CheckIsOnline())
        {
            // 2️⃣ Prepare the transport with the target IP/port
            var networkManager = NetworkManager.Singleton;
            var unityTransport = networkManager.GetComponent<UnityTransport>();
            unityTransport.SetConnectionData(serverIP, serverPort);

            // 3️⃣ Wire callbacks into a TaskCompletionSource so we can await synchronously
            var taskCompletionSource = new TaskCompletionSource<ConnectResult>();
            void OnSuccess(ulong _) => taskCompletionSource.TrySetResult(ConnectResult.Success);
            void OnFail(ulong _) => taskCompletionSource.TrySetResult(ParseDisconnectReason(networkManager.DisconnectReason));

            networkManager.OnClientConnectedCallback += OnSuccess;
            networkManager.OnClientDisconnectCallback += OnFail;
            networkManager.StartClient();

            // 4️⃣ Race connection attempt vs. timeout
            var winner = await Task.WhenAny(taskCompletionSource.Task, Task.Delay(TIME_OUT));
            var result = winner == taskCompletionSource.Task ? taskCompletionSource.Task.Result : ConnectResult.Timeout;

            // 5️⃣ Clean up and map to friendly message
            networkManager.Shutdown();
            networkManager.OnClientConnectedCallback -= OnSuccess;
            networkManager.OnClientDisconnectCallback -= OnFail;
            // return FriendlyMessage(result);
        }

        return ConnectResult.Unknown;
    }
    
    public enum ConnectResult { Success, Timeout, NetworkError, IncompatibleVersion, Unknown }

    private static ConnectResult ParseDisconnectReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) return ConnectResult.Unknown;
        
        if (reason.Contains("ConnectTimeout", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("MaxConnectionAttempts", StringComparison.OrdinalIgnoreCase))
            return ConnectResult.Timeout;
        if (reason.Contains("NetworkFailure", StringComparison.OrdinalIgnoreCase))
            return ConnectResult.NetworkError;
        if (reason.Contains("HashMismatch", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("Incompatible", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("ProtocolError", StringComparison.OrdinalIgnoreCase))
            return ConnectResult.IncompatibleVersion;
        return ConnectResult.Unknown;
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
