using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameStateManager : BaseGameStateManager
{
    private BaseGameOverManager gameOverManager;
    private void Start()
    {
        gameOverManager = ServiceLocator.Get<BaseGameOverManager>();
    }

    public override async void ChangeGameState(GameState gameState, int delayToChange = 0)
    {
        if (gameOverManager.GameOver) return;

        await Task.Delay(delayToChange);
        SetGameStateServerRpc(gameState);

    }

    public override void ConnectionLostHostAndClient()
    {
        TriggerOnLostConnectionInHost();
    }

    public override void HandleOnGameOver()
    {
        if (!IsServer) return;

        ChangeGameState(GameState.GameEnded);
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
                if (IsServer && !IsHost) //DS
                {
                    //REFACTOR
                    CalculatePearlsManager.CalculatePossibleResults(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[0], NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas[1]);
                    ChangeGameState(GameState.ShowingPlayersInfo);
                }
                else if (IsHost)
                {
                    ChangeGameState(GameState.ShowingPlayersInfo);
                }
                break;
            case GameState.ShowingPlayersInfo:
                if (IsServer)
                {
                    GameManager.Instance.RandomizePlayerItems();
                    ChangeGameState(GameState.GameStarted, delayClosePlayersInfo); //Show Players Info delay
                }
                break;
            case GameState.GameStarted:
                break;
            case GameState.GameEnded:
                //If is DS, change players save
                if (IsServer)
                    CheckForTheWinner();
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

    [Rpc(SendTo.Server)]
    protected override void SetGameStateServerRpc(GameState newState)
    {
        gameState.Value = newState;
    }
}
