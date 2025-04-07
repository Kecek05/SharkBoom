using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameStateManager : BaseGameStateManager
{

    public override async void ChangeGameState(GameState gameState, int delayToChangeMS = 0)
    {
        if (CurrentGameState.Value == GameState.GameEnded) return;

        await Task.Delay(delayToChangeMS);
        SetGameStateServerRpc(gameState);

    }

    public override void ConnectionLostHostAndClient()
    {
        TriggerOnLostConnectionInHost();
    }

    public override void HandleOnGameStateValueChanged(GameState newValue)
    {
        switch (newValue)
        {
            case GameState.WaitingForPlayers:
                break;
            case GameState.SpawningPlayers:
                //All players connected
                Debug.Log("Start Spawning Players");

                if(IsServer)
                {
                    //ServiceLocator.Get<BasePlayersPublicInfoManager>().RandomizePlayerItems();

                    ChangeGameState(GameState.CalculatingResults);
                }

                break;
            case GameState.CalculatingResults:
                if(IsServer)
                {
                    //if(!IsHost)  //REFACTOR
                    //CalculatePearls.CalculatePossibleResults(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0], NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1]);
                    ServiceLocator.Get<BasePlayersPublicInfoManager>().RandomizePlayerItems();
                }
                break;
            case GameState.ShowingPlayersInfo:
                if (IsServer)
                {
                    //ServiceLocator.Get<BasePlayersPublicInfoManager>().RandomizePlayerItems();
                    ChangeGameState(GameState.GameStarted, DELAY_CLOSE_PLAYERSINFO); //Show Players Info delay
                }
                break;
            case GameState.GameStarted:
                break;
            case GameState.GameEnded:
                //If is DS, change players save
                //Show UI for clients
                break;
        }
        Debug.Log($"Game State Changed to: {newValue}");
    }

    public override void HandleOnGameTimerEnd()
    {
        if(!IsServer) return;

        Debug.Log("Game Timer Ended");
        ChangeGameState(GameState.GameEnded);
    }

    public override void HandleOnPlayerSpawned(int playerCount)
    {
        if(!IsServer) return;

        if (playerCount == 2)
        {
            ChangeGameState(GameState.CalculatingResults); // All players Spawned, calculating Results

            if(!IsHost)
            {
                //Is DS, wait for a bit and then change state
                ChangeGameState(GameState.ShowingPlayersInfo, DELAY_DS_STARTGAME);
            }
        }
    }

    public override void HandleOnPlayerGainOwnership(ulong clientId)
    {
        if(!IsServer) return;

        if(!IsHost) return; //DS should not change state

        clientsGainedOwnership++;

        if(clientsGainedOwnership >= 2)
        {
            ChangeGameState(GameState.ShowingPlayersInfo); // All clients with ownership, relay can start game
        }
    }

    public override void HandeOnPlayerDie()
    {
        //Player died, Game Over.

        ChangeGameState(GameState.GameEnded);
    }

    [Rpc(SendTo.Server)]
    private void SetGameStateServerRpc(GameState newState)
    {
        gameState.Value = newState;
    }

}
