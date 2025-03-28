using System.Threading.Tasks;
using Unity.Netcode;

public class TurnManager : BaseTurnManager
{

    public override void HandleOnPlayableStateValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        if(!IsClient) return;

        if (GameManager.Instance.GameStateManager.GameOver) return;

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

    }

    [Rpc(SendTo.Server)]
    public override void PlayerJumpedServerRpc(PlayableState playableState)
    {
        if (GameManager.Instance.GameStateManager.GameOver) return;

        PlayerJumpedClientRpc(playableState);
    }

    [Rpc(SendTo.Server)]
    public override void PlayerPlayedServerRpc(PlayableState playerPlayingState)
    {
        if (GameManager.Instance.GameStateManager.GameOver) return;

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
        SetPlayableStateServerRpc(PlayableState.Player1Playing); // Debug

        //int randomStartPlayer = UnityEngine.Random.Range(0, 2);
        //SetPlayableStateServerRpc(randomStartPlayer == 0 ? PlayableState.Player1Playing : PlayableState.Player2Playing);
    }

    protected override async void DelayChangeTurns(PlayableState playableState)
    {
        if (GameManager.Instance.GameStateManager.GameOver) return;

        await Task.Delay(delayBetweenTurns);
        SetPlayableStateServerRpc(playableState);
    }

    [Rpc(SendTo.ClientsAndHost)]
    protected override void PlayerJumpedClientRpc(PlayableState playableState)
    {
        if (GameManager.Instance.GameStateManager.GameOver) return;

        if (localPlayableState == playableState)
        {
            //if the jump item is the same as the player playing, owner jumped
            TriggerOnMyTurnJumped();
        }
    }

    [Rpc(SendTo.Server)]
    protected override void SetPlayableStateServerRpc(PlayableState newState)
    {
        currentPlayableState.Value = newState;
    }
}
