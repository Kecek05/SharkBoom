using System;
using Unity.Netcode;

public abstract class BasePearlsManager : NetworkBehaviour
{
    public event Action<int> OnPearlsChanged;


    protected void TriggerOnPearlsChanged(int value) => OnPearlsChanged?.Invoke(value);

    protected abstract void CheckForTheWinner(PlayableState losedPlayerState);

    public abstract void HandleOnGameStateChanged(GameState newValue);

    protected abstract void SendGameResultsToClient(string authId, int valueToShow);
}
