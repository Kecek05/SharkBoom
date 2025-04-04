using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameStateManager : BaseGameStateManager
{

    public override async void ChangeGameState(GameState gameState, int delayToChange = 0)
    {
        if (CurrentGameState.Value == GameState.GameEnded) return;

        await Task.Delay(delayToChange);
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

                break;
            case GameState.CalculatingResults:
                if(IsServer)
                {
                    //if(!IsHost)  //REFACTOR
                        //CalculatePearls.CalculatePossibleResults(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0], NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1]);
                    ChangeGameState(GameState.ShowingPlayersInfo);
                }
                break;
            case GameState.ShowingPlayersInfo:
                if (IsServer)
                {
                    ServiceLocator.Get<BasePlayersPublicInfoManager>().RandomizePlayerItems();
                    ChangeGameState(GameState.GameStarted, delayClosePlayersInfo); //Show Players Info delay
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
