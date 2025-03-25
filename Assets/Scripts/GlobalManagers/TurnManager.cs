using Sortify;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{

    public event Action OnMyTurnStarted; //local player can play
    public event Action OnMyTurnEnded;
    public event Action OnMyTurnJumped;
    public event Action OnLocalPlayableStateChanged;

    [BetterHeader("Settings")]
    [Tooltip("in ms")][SerializeField] private int delayBetweenTurns = 3000;

    private PlayableState localPlayableState = new();
    private PlayableState localPlayedState = new();

    private NetworkVariable<PlayableState> currentPlayableState = new(PlayableState.None);

    //Publics
    public PlayableState LocalPlayableState => localPlayableState;
    public NetworkVariable<PlayableState> CurrentPlayableState => currentPlayableState;


    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            currentPlayableState.OnValueChanged += CurrentPlayableState_OnValueChanged;
        }
    }

    /// <summary>
    /// Set the localPlayable and localPlayed states to the GameFlowManager, Only Owner.
    /// </summary>
    /// <param name="playingState"> Playing State</param>
    public void InitializeLocalStates(PlayableState playingState)
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

        OnLocalPlayableStateChanged?.Invoke();
    }

    public void StartGame()
    {
        SetPlayableStateServerRpc(PlayableState.Player1Playing); // Debug

        //int randomStartPlayer = UnityEngine.Random.Range(0, 2);
        //SetPlayableStateServerRpc(randomStartPlayer == 0 ? PlayableState.Player1Playing : PlayableState.Player2Playing);
    }

    /// <summary>
    /// Call this when player played an item. It will change the turn.
    /// </summary>
    /// <param name="playerPlayingState"> Playing State</param>
    [Rpc(SendTo.Server)]
    public void PlayerPlayedServerRpc(PlayableState playerPlayingState)
    {
        if (GameFlowManager.Instance.GameStateManager.GameOver) return;

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

    /// <summary>
    /// Called when the player jumped.
    /// </summary>
    /// <param name="playableState"> Playing State</param>
    [Rpc(SendTo.Server)]
    public void PlayerJumpedServerRpc(PlayableState playableState)
    {
        if (GameFlowManager.Instance.GameStateManager.GameOver) return;

        PlayerJumpedClientRpc(playableState);
    }

    /// <summary>
    /// Tell the owner that the player jumped.
    /// </summary>
    /// <param name="playableState"> Owner Playing State</param>
    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerJumpedClientRpc(PlayableState playableState)
    {
        if (GameFlowManager.Instance.GameStateManager.GameOver) return;

        if (localPlayableState == playableState)
        {
            //if the jump item is the same as the player playing, owner jumped
            OnMyTurnJumped?.Invoke();
        }
    }

    /// <summary>
    /// Call this to change the player's turn after a delay.
    /// </summary>
    /// <param name="playableState"> Playing State</param>
    private async void DelayChangeTurns(PlayableState playableState)
    {
        if (GameFlowManager.Instance.GameStateManager.GameOver) return;

        await Task.Delay(delayBetweenTurns);
        SetPlayableStateServerRpc(playableState);
    }

    [Rpc(SendTo.Server)]
    private void SetPlayableStateServerRpc(PlayableState newState)
    {
        currentPlayableState.Value = newState;
    }


    private void CurrentPlayableState_OnValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        if (GameFlowManager.Instance.GameStateManager.GameOver) return;

        if (newValue == localPlayableState)
        {
            //Local Player can play
            OnMyTurnStarted?.Invoke();
        }
        else if (newValue == localPlayedState)
        {
            //Local Player cant play
            OnMyTurnEnded?.Invoke();
        }

    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            currentPlayableState.OnValueChanged -= CurrentPlayableState_OnValueChanged;
        }
    }
}
