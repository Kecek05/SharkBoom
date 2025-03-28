using Unity.Netcode;
using UnityEngine;

public class GameOverManager : BaseGameOverManager
{

    public override void OnNetworkSpawn()
    {
        throw new System.NotImplementedException();
    }

    public override void GameOverClient()
    {
        if (losedPlayer.Value == GameManager.Instance.TurnManager.LocalPlayableState)
        {
            //Change pearls, then lose


            //await CalculatePearlsManager.TriggerChangePearls();
            OnLose?.Invoke();
        }
        else if (losedPlayer.Value == PlayableState.None)
        {
            //Tie, lose


            //await CalculatePearlsManager.TriggerChangePearls();
            OnLose?.Invoke();
        }
        else
        {
            //Change pearls, then win

            //await CalculatePearlsManager.TriggerChangePearls();
            OnWin?.Invoke();
        }
    }

    public override void LoseGame(PlayableState playerLosedPlayableState)
    {
        losedPlayer.Value = playerLosedPlayableState;
    }





    protected override void CheckForTheWinner()
    {
        //Code to check if both players have the same health, if so, tie, otherwise check who has the most health and declare the winner.

        if (LosedPlayer.Value == PlayableState.None)
        {
            //Tie, both lose

            if (!IsHost)
            {
                //DS
                await CalculatePearlsManager.ChangePearlsLoser(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0]);

                await CalculatePearlsManager.ChangePearlsLoser(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1]);

            }

            SendGameResultsToClientRpc(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].calculatedPearls.PearlsToLose);

            SendGameResultsToClientRpc(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].calculatedPearls.PearlsToLose);

            TriggerCanCloseServerRpc();

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
            SendGameResultsToClientRpc(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].calculatedPearls.PearlsToLose);

            SendGameResultsToClientRpc(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].calculatedPearls.PearlsToWin);

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
            SendGameResultsToClientRpc(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0].calculatedPearls.PearlsToWin);

            SendGameResultsToClientRpc(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].userData.userAuthId, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1].calculatedPearls.PearlsToLose);

        }

        TriggerCanCloseServerRpc();
    }

    protected override void HandleOnLosedPlayerValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        TriggerOnGameOver();

        if (IsServer)
            ChangeGameState(GameState.GameEnded);

        if (IsClient)
        {
            GameOverClient();
        }

        gameOver = true;
    }

    [Rpc(SendTo.ClientsAndHost)]
    protected override void SendGameResultsToClientRpc(string authId, int valueToShow)
    {
        if (ClientSingleton.Instance == null) return;

        if (ClientSingleton.Instance.GameManager.UserData.userAuthId != authId) return;

        //Owner
        OnCanShowPearls?.Invoke(valueToShow);

    }

    public override void OnNetworkDespawn()
    {
        throw new System.NotImplementedException();
    }
}
