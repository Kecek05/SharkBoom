using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class PearlsManager : BasePearlsManager
{

    public override void HandleOnGameStateChanged(GameState newValue)
    {
        if (newValue == GameState.GameEnded)
        {
            CheckForTheWinner(ServiceLocator.Get<BaseGameOverManager>().LosedPlayer.Value);
        }
    }

    protected override async void CheckForTheWinner(PlayableState losedPlayerState)
    {
        //Code to check if both players have the same health, if so, tie, otherwise check who has the most health and declare the winner.

        if (!IsServer) return;

        if (losedPlayerState == PlayableState.None)
        {
            //Tie, both lose

            if (!IsHost)
            {
                //DS
                await CalculatePearls.ChangePearlsLoser(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0]);

                await CalculatePearls.ChangePearlsLoser(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1]);

            }

            SendGameResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].calculatedPearls.PearlsToLose);

            SendGameResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].calculatedPearls.PearlsToLose);

            //TriggerCanCloseServerRpc();

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

            SendGameResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].calculatedPearls.PearlsToLose);

            SendGameResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].calculatedPearls.PearlsToWin);

        }
        else
        {
            //Player 1 Winner

            if (!IsHost)
            {
                //DS
                await CalculatePearls.ChangePearlsWinner(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0]);

                await CalculatePearls.ChangePearlsLoser(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1]);
            }

            SendGameResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].calculatedPearls.PearlsToWin);

            SendGameResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].calculatedPearls.PearlsToLose);

        }

        //TriggerCanCloseServerRpc();
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
