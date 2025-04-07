using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : BaseTurnManager
{

    private BaseGameStateManager gameStateManager;

    private void Start()
    {
        gameStateManager = ServiceLocator.Get<BaseGameStateManager>();
    }

    public override void HandleOnGameStateChanged(GameState newValue)
    {
        if (newValue == GameState.GameStarted)
        {
            StartGame();
        }
    }

    public override void HandleOnPlayableStateValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        if(!IsClient) return;

        if (gameStateManager.CurrentGameState.Value == GameState.GameEnded) return;

        if (newValue == localPlayableState)
        {
            //Local Player can play
            TriggerOnMyTurnStarted();
        }
        else if (newValue == localPlayedState)
        {
            //Local Player cant play
            TriggerOnMyTurnEnded();
        }
    }


    public override void HandleOnTimesUp()
    {
        PlayerPlayed(currentPlayableState.Value); //change turn
    }

    public override void InitializeLocalStates(PlayableState playingState)
    {
        switch (playingState)
        {
            case PlayableState.Player1Playing:
                localPlayableState = PlayableState.Player1Playing;
                localPlayedState = PlayableState.Player1Played;
                break;
            case PlayableState.Player2Playing:
                localPlayableState = PlayableState.Player2Playing;
                localPlayedState = PlayableState.Player2Played;
                break;
        }

        TriggerOnLocalPlayableStateChanged();
        Debug.Log($"InitializeLocalStates, Local Playing State {playingState}");
    }


    public override void PlayerJumped(PlayableState playableState)
    {
        PlayerJumpedServerRpc(playableState);
    }


    [Rpc(SendTo.Server)]
    private void PlayerJumpedServerRpc(PlayableState playableState)
    {
        if (gameStateManager.CurrentGameState.Value == GameState.GameEnded) return;

        PlayerJumpedClient(playableState);
    }


    public override void PlayerPlayed(PlayableState playerPlayingState)
    {
        PlayerPlayedServerRpc(playerPlayingState);
    }

    [Rpc(SendTo.Server)]
    private void PlayerPlayedServerRpc(PlayableState playerPlayingState)
    {
        if (gameStateManager.CurrentGameState.Value == GameState.GameEnded) return;

        if (playerPlayingState == PlayableState.Player1Playing)
        {
            //recived item callback from player 1
            currentPlayableState.Value = PlayableState.Player1Played;

            DelayChangeTurns(PlayableState.Player2Playing);
        }
        else if (playerPlayingState == PlayableState.Player2Playing)
        {
            //recived item callback from player 2
            currentPlayableState.Value = PlayableState.Player2Played;

            DelayChangeTurns(PlayableState.Player1Playing);
        }
    }

    public override void StartGame()
    {
        if(!IsServer) return;

        SetPlayableStateServer(PlayableState.Player1Playing); // Debug

        //int randomStartPlayer = UnityEngine.Random.Range(0, 2);
        //SetPlayableStateServerRpc(randomStartPlayer == 0 ? PlayableState.Player1Playing : PlayableState.Player2Playing);
    }

    protected override async void DelayChangeTurns(PlayableState playableState)
    {
        if (gameStateManager.CurrentGameState.Value == GameState.GameEnded) return;

        await Task.Delay(delayBetweenTurns);
        SetPlayableStateServer(playableState);
    }


    protected override void PlayerJumpedClient(PlayableState playableState)
    {
        PlayerJumpedClientRpc(playableState);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerJumpedClientRpc(PlayableState playableState)
    {
        if (gameStateManager.CurrentGameState.Value == GameState.GameEnded) return;

        if (localPlayableState == playableState)
        {
            //if the jump item is the same as the player playing, owner jumped
            TriggerOnMyTurnJumped();
        }
    }


    protected override void SetPlayableStateServer(PlayableState newState)
    {
        SetPlayableStateServerRpc(newState);
    }

    [Rpc(SendTo.Server)]
    private void SetPlayableStateServerRpc(PlayableState newState)
    {
        currentPlayableState.Value = newState;
    }

}
