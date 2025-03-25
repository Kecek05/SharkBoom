using QFSW.QC;
using Sortify;
using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class DebuggingTools : NetworkBehaviour
{

    public static DebuggingTools Instance;

    [BetterHeader("References")]
    public TextMeshProUGUI debugGameStateText;
    public TextMeshProUGUI debugPlayableStateText;
    public TextMeshProUGUI debugPingText;

    //Ping
    private float lastPingTime;
    private float currentPing = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if(IsClient)
        {
            StartCoroutine(PingCoroutine());
        }
    }

    private IEnumerator PingCoroutine()
    {
        while (true)
        {
            lastPingTime = Time.time;
            RequestPingServerRpc();
            yield return new WaitForSeconds(1f); // Send ping every second
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPingServerRpc(ServerRpcParams rpcParams = default)
    {
        RespondPingClientRpc(rpcParams.Receive.SenderClientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RespondPingClientRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            currentPing = (Time.time - lastPingTime) * 1000f; // Convert to milliseconds
            debugPingText.text = $"Ping: {currentPing:F1} ms";
        }
    }

    private void Update()
    {
        debugGameStateText.text = GameFlowManager.Instance.GameStateManager.CurrentGameState.Value.ToString();
        debugPlayableStateText.text = GameFlowManager.Instance.TurnManager.CurrentPlayableState.Value.ToString();
    }



    [Command("shutDownDebug")]
    private void ShutdownDebug() //DEBUG
    {
        if (NetworkManager.Singleton.IsConnectedClient)
            NetworkManager.Singleton.Shutdown();
    }

    [Command("quitGame")]
    private void QuitGameDEBUG()
    {
        //Return to main menu
        if (NetworkManager.Singleton != null && HostSingleton.Instance != null && NetworkManager.Singleton.IsHost) //Server cant click buttons
        {
            HostSingleton.Instance.GameManager.ShutdownAsync();
        }

        if (ClientSingleton.Instance != null)
            ClientSingleton.Instance.GameManager.Disconnect();
    }
}
