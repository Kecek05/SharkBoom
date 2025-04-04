using Unity.Netcode;
using UnityEngine;

public class PearlsManager : BasePearlsManager
{

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

            }


            SendGameResultsToClient
                (
                NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId, 
                CalculatePearls.AuthIdToCalculatedPearls[NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId].PearlsToLose
                );

            SendGameResultsToClient
                (
                NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId,
                CalculatePearls.AuthIdToCalculatedPearls[NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId].PearlsToLose
                );

            TriggerOnFinishedCalculationsOnServer();

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
            }

            SendGameResultsToClient
                (
                NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId,
                CalculatePearls.AuthIdToCalculatedPearls[NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId].PearlsToLose
                );

            SendGameResultsToClient
                (
                NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId,
                CalculatePearls.AuthIdToCalculatedPearls[NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId].PearlsToWin
                );

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
            }

            SendGameResultsToClient
                (
                NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId,
                CalculatePearls.AuthIdToCalculatedPearls[NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId].PearlsToWin
                );

            SendGameResultsToClient
                (
                NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId,
                CalculatePearls.AuthIdToCalculatedPearls[NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId].PearlsToLose
                );

            Debug.Log($"Player {NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userName} Winner");
        }

        TriggerOnFinishedCalculationsOnServer();
    }

    protected override void SendGameResultsToClient(string authId, int valueToShow)
    {
        SendGameResultsToClientRpc(authId, valueToShow);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SendGameResultsToClientRpc(string authId, int valueToShow)
    {
        if (ClientSingleton.Instance == null) return;

        if (ClientSingleton.Instance.GameManager.UserData.userAuthId != authId) return;

        //Owner
        TriggerOnPearlsChanged(valueToShow);

    }
}
