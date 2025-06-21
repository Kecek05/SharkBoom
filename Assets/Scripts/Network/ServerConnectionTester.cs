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

    private static async Task<bool> CheckIsOnline()
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
    /// Call this to check if till need to reconnect with DS
    /// </summary>
    /// <param name="serverIP"></param>
    /// <param name="queryPort"></param>
    /// <returns> if true, client is offline or the server is online, false, server is offline dont reconnect anymore</returns>
    public static async Task<bool> CheckServerAsync(string serverIP, int queryPort)
    {
        if(await CheckIsOnline())
        {
            if (await IsServerOnline(serverIP, queryPort))
            {
                Debug.Log($"Server is Reachable");
                return true;
            }
            else
            {
                Debug.Log($"Server not Reachable");
                return false;
            }
        }
        else
        {
            return true;
        }
    }
    
    public static async Task<bool> IsServerOnline(string ip, int queryPort, int timeoutMs = 1000)
    {
        using var udp = new UdpClient();
        udp.Client.ReceiveTimeout = timeoutMs;
        var end = new IPEndPoint(IPAddress.Parse(ip), queryPort);

        // Send ChallengeRequest packet (type = 0 + 4 dummy bytes)
        byte[] chal = new byte[5];
        chal[0] = 0;
        await udp.SendAsync(chal, chal.Length, end);

        var resp = await udp.ReceiveAsync();
        if (resp.Buffer.Length < 5) return false;

        // parse ChallengeToken (big‑endian)
        uint token = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(resp.Buffer, 1));

        // Send QueryRequest packet (type = 1, with token + version + chunk mask)
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write((byte)1);
        bw.Write(IPAddress.HostToNetworkOrder((int)token));
        bw.Write(IPAddress.HostToNetworkOrder((short)1)); // protocol version
        bw.Write((byte)1); // Request ServerInfo chunk
        await udp.SendAsync(ms.ToArray(), (int)ms.Length, end);

        var qr = await udp.ReceiveAsync();
        return qr.Buffer.Length >= 1 && qr.Buffer[0] == 1;
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
