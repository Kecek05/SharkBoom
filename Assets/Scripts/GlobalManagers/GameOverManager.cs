using Unity.Netcode;

public class GameOverManager : BaseGameOverManager
{
    
    private BaseTurnManager turnManager;

    private void Start()
    {
        turnManager = ServiceLocator.Get<BaseTurnManager>();
    }

    public override void GameOverClient()
    {
        if (losedPlayer.Value == turnManager.LocalPlayableState)
        {
            //Change pearls, then lose


            //await CalculatePearlsManager.TriggerChangePearls();
            TriggerOnLose();
        }
        else if (losedPlayer.Value == PlayableState.None)
        {
            //Tie, lose


            //await CalculatePearlsManager.TriggerChangePearls();
            TriggerOnLose();
        }
        else
        {
            //Change pearls, then win

            //await CalculatePearlsManager.TriggerChangePearls();
            TriggerOnWin();
        }
    }

    public override void HandleOnGameStateChanged(GameState newValue)
    {
        if (newValue == GameState.GameEnded)
        {
            CheckForTheWinner();
        }
    }

    public override void LoseGame(PlayableState playerLosedPlayableState)
    {
        losedPlayer.Value = playerLosedPlayableState;
    }

    protected override async void CheckForTheWinner()
    {
        //Code to check if both players have the same health, if so, tie, otherwise check who has the most health and declare the winner.

        if(!IsServer) return;

        if (LosedPlayer.Value == PlayableState.None)
        {
            //Tie, both lose

            if (!IsHost)
            {
                //DS
                await CalculatePearlsManager.ChangePearlsLoser(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0]);

                await CalculatePearlsManager.ChangePearlsLoser(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1]);

            }

            SendGameResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].calculatedPearls.PearlsToLose);

            SendGameResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].calculatedPearls.PearlsToLose);

            //TriggerCanCloseServerRpc();

            return;
        }

        if (LosedPlayer.Value == NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].playableState)
        {
            //Player 2 Winner

            if (!IsHost)
            {
                //DS
                await CalculatePearlsManager.ChangePearlsWinner(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1]);

                await CalculatePearlsManager.ChangePearlsLoser(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0]);


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
                await CalculatePearlsManager.ChangePearlsWinner(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0]);

                await CalculatePearlsManager.ChangePearlsLoser(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1]);

            }
            SendGameResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].calculatedPearls.PearlsToWin);

            SendGameResultsToClient(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].calculatedPearls.PearlsToLose);

        }

        //TriggerCanCloseServerRpc();
    }

    public override void HandleOnLosedPlayerValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        TriggerOnGameOver();

        if (IsClient)
        {
            GameOverClient();
        }

        gameOver = true;
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
        //OnCanShowPearls?.Invoke(valueToShow);

    }
}
