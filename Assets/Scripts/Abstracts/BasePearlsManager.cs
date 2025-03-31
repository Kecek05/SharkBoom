using System;
using Unity.Netcode;

public abstract class BasePearlsManager : NetworkBehaviour
{
    /// <summary>
    /// Called when the Client recieves the pearls value.
    /// </summary>
    public event Action<int> OnPearlsChanged;

    /// <summary>
    /// Called when already finished changing pearls on Cloud Code.
    /// </summary>
    public static event Action OnFinishedCalculationsOnServer;

    protected void TriggerOnPearlsChanged(int value) => OnPearlsChanged?.Invoke(value);

    protected void TriggerOnFinishedCalculationsOnServer() => OnFinishedCalculationsOnServer?.Invoke();

    protected abstract void ChangePearls(PlayableState losedPlayerState);

    public abstract void HandleOnLosedPlayerChanged(PlayableState newValue);

    protected abstract void SendGameResultsToClient(string authId, int valueToShow);
}
