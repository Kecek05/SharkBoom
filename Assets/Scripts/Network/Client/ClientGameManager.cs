using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class ClientGameManager : IDisposable //Actual Logic to interact with UGS (Relay, Lobby, etc)
{
    private NetworkClient networkClient;
    private MatchplayMatchmaker matchmaker;


    private JoinAllocation joinAllocation;

    private bool isJoining = false;

    private UserData userData;
    public UserData UserData => userData;

    private string joinCode;
    public string JoinCode => joinCode;

    public async Task<bool> InitAsync(AuthTypes authTypes)
    {
        //Authenticate Player
        await UnityServices.InitializeAsync();

        networkClient = new NetworkClient(NetworkManager.Singleton);
        matchmaker = new();

        AuthState authState = authTypes == AuthTypes.Anonymous ? await AuthenticationWrapper.DoAuthAnonymously() : authTypes == AuthTypes.Unity ? await AuthenticationWrapper.DoAuthUnity() : authTypes == AuthTypes.Android ? await AuthenticationWrapper.DoAuthAndroid() : AuthState.NotAuthenticated;


        if(authState == AuthState.Authenticated)
        {
            AuthenticationWrapper.SetPlayerName(await AuthenticationService.Instance.GetPlayerNameAsync());

            Debug.Log(AuthenticationWrapper.PlayerName + authState);

            userData = new UserData
            {
                userName = AuthenticationWrapper.PlayerName,
                userAuthId = AuthenticationService.Instance.PlayerId,
                userPearls = UnityEngine.Random.Range(0, 1001), // random for debug
            };

             Loader.Load(Loader.Scene.MainMenu);

            return true;
        }

        return false;
    }

    public void StartMatchmakingClient(string ip, int port)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort)port);
        ConnectClient();
    }


    public async Task StartRelayClientAsync(string joinCode)
    {
        if (joinCode == null || joinCode == string.Empty) return;

        try
        {
            joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        } catch (Exception ex)
        {
            Debug.LogException(ex);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");

        transport.SetRelayServerData(relayServerData);

        this.joinCode = joinCode;
        Debug.Log("Code Relay:" + this.joinCode);

        ConnectClient();
    }

    private void ConnectClient()
    {
        string payload = JsonUtility.ToJson(userData); //serialize the payload to json
        byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload); //serialize the payload to bytes

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();

        Debug.Log("Started Client!");
    }

    public async Task QuickJoinLobbyAsync()
    {
        if(isJoining) return;

        isJoining = true;

        try
        {
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            if (lobby != null)
            {
                string joinCode = lobby.Data["JoinCode"].Value;
                await StartRelayClientAsync(joinCode);
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }

        isJoining = false;

    }

    /// <summary>
    /// Call in UI to matchmake
    /// </summary>
    public async void MatchmakeAsync(Action<MatchmakerPollingResult> onMatchmakeResponse)
    {
        // Its void because we dont want to wait for the result to continue the execution of the code.
        // Pass and event to call it when the result is ready. Who call this will know the response when the match responds.

        if (matchmaker.IsMatchmaking) return;

        MatchmakerPollingResult matchResult = await GetMatchAsync();

        onMatchmakeResponse?.Invoke(matchResult);

    }

    /// <summary>
    /// Call this to start the matchmake process, return the result.
    /// </summary>
    /// <returns></returns>
    private async Task<MatchmakerPollingResult> GetMatchAsync()
    {
        MatchmakingResult matchmakingResult = await matchmaker.Matchmake(userData);

        if(matchmakingResult.result == MatchmakerPollingResult.Success)
        {
            Debug.Log("Match Found!");
            StartMatchmakingClient(matchmakingResult.ip, matchmakingResult.port);
        }

        return matchmakingResult.result;
    }

    public async Task CancelMatchmakingAsync()
    {
        await matchmaker.CancelMatchmaking();
    }


    /// <summary>
    /// Call this to disconnect from the server and go to Main Menu
    /// </summary>
    public void Disconnect()
    {
        networkClient.Disconnect();
    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }
}


public enum AuthTypes
{
    Anonymous,
    Unity,
    Android,
    IOS,
}
