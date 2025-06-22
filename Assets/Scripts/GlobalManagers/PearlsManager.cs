using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PearlsManager : BasePearlsManager
{
    private int gameOversSynced = 0;
    private bool alreadyTriggeredFinishedCalculationOnServer = false;
    private const int TIME_WAIT_LOCAL_PLAYERS_DIED = 8; //Wait in seconds
    
    public override void OnNetworkSpawn()
    {
        GameOverUI.OnRecievedAllGameOverUIInfo += GameOverUIOnOnRecievedAllGameOverUIInfo;
    }

    public override void OnNetworkDespawn()
    {
        GameOverUI.OnRecievedAllGameOverUIInfo -= GameOverUIOnOnRecievedAllGameOverUIInfo;
    }

    protected override void GameOverUIOnOnRecievedAllGameOverUIInfo()
    {
        if(!IsServer) return;
        gameOversSynced++;
        // Debug.Log($"GAME OVER - GameOverUIOnOnRecievedAllGameOverUIInfo Count: {gameOversSynced}");
        if (gameOversSynced >= 2)
        {
            if(alreadyTriggeredFinishedCalculationOnServer) return;
            
            alreadyTriggeredFinishedCalculationOnServer = true;
            TriggerOnFinishedCalculationsOnServer();
        }
    }

    public override void HandleOnLosedPlayerChanged(PlayableState newValue)
    {
        if (!IsServer) return;

        ChangePearls(newValue);
    }

    protected override async void ChangePearls(PlayableState losedPlayerState)
    {
        if (!IsServer) return;

        if (losedPlayerState == PlayableState.Tie)
        {
            //Tie, both lose

            if (!IsHost)
            {
                //DS
                await CalculatePearls.ChangePearlsLoser(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0]);

                await CalculatePearls.ChangePearlsLoser(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1]);

                SendPearlsResultsToClient
                    (
                    NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId,
                    CalculatePearls.AuthIdToCalculatedPearls[NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId].PearlsToLose
                    );

                SendPearlsResultsToClient
                    (
                    NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId,
                    CalculatePearls.AuthIdToCalculatedPearls[NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId].PearlsToLose
                    );


            } else
            {
                SendPearlsResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId, 0);

                SendPearlsResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId, 0);
            }

            StartCoroutine(WaitTriggerOnFinishedCalculationsOnServer());

            Debug.Log("Tie, both lose");
            return;
        }

        if (losedPlayerState == NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].playableState)
        {
            //Player 2 Winner

            if (!IsHost)
            {
                //DS
                await CalculatePearls.ChangePearlsWinner(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1]);

                await CalculatePearls.ChangePearlsLoser(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0]);


                SendPearlsResultsToClient
                    (
                    NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId,
                    CalculatePearls.AuthIdToCalculatedPearls[NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId].PearlsToLose
                    );

                SendPearlsResultsToClient
                    (
                    NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId,
                    CalculatePearls.AuthIdToCalculatedPearls[NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId].PearlsToWin
                    );

            } else
            {
                SendPearlsResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId, 0);

                SendPearlsResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId, 0);
            }

            Debug.Log($"Player {NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userName} Winner");
        }
        else if (losedPlayerState == NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].playableState)
        {
            //Player 1 Winner

            if (!IsHost)
            {
                //DS
                await CalculatePearls.ChangePearlsWinner(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0]);

                await CalculatePearls.ChangePearlsLoser(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1]);

                SendPearlsResultsToClient
                    (
                    NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId,
                    CalculatePearls.AuthIdToCalculatedPearls[NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId].PearlsToWin
                    );

                SendPearlsResultsToClient
                    (
                    NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId,
                    CalculatePearls.AuthIdToCalculatedPearls[NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId].PearlsToLose
                    );
            } else
            {
                SendPearlsResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId, 0);

                SendPearlsResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId, 0);
            }



            Debug.Log($"Player {NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userName} Winner");
        }

        StartCoroutine(WaitTriggerOnFinishedCalculationsOnServer());
    }

    protected override IEnumerator WaitTriggerOnFinishedCalculationsOnServer()
    {
        yield return new WaitForSeconds(TIME_WAIT_LOCAL_PLAYERS_DIED);
        //Need to wait time or see if all already recieveed the RPC with UI infos

        if (alreadyTriggeredFinishedCalculationOnServer) yield break;
        TriggerOnFinishedCalculationsOnServer(); //This will shut down the server

    }

    protected override void SendPearlsResultsToClient(string authId, int valueToShow)
    {
        SendPearlsResultsToClientRpc(authId, valueToShow);
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void SendPearlsResultsToClientRpc(string authId, int valueToShow)
    {
        if (ClientSingleton.Instance == null) return;

        if (ClientSingleton.Instance.GameManager.UserData.userAuthId != authId) return;

        //Owner
        TriggerOnPearlsChanged(valueToShow);

    }
}
