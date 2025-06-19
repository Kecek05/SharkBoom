using System;
using Unity.Netcode;

public abstract class BasePlayerSyncPosition : NetworkBehaviour
{
    /// <summary>
    /// Called when the position is synced with the server.
    /// </summary>
    public static event Action OnPositionSyncedWithServer;
    
    protected BasePlayersPublicInfoManager basePlayersPublicInfoManager;

    protected void InvokeOnPositionSyncedWithServer() => OnPositionSyncedWithServer?.Invoke();

    public abstract void Initialize(BasePlayersPublicInfoManager basePlayersPublicInfoManager);
    
    // public abstract void SyncPlayersPositionServerRpc();
}
