using Unity.Netcode;

public class PearlsManager : BasePearlsManager
{

    public override void HandleOnGameStateChanged(GameState newValue)
    {
        if (newValue == GameState.GameEnded)
        {
            //ChangePearls(GetTheWinner());
        }
    }

    protected override async void ChangePearls(PlayableState losedPlayerState)
    {
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

            TriggerOnFinishedCalculationsOnServer();

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
