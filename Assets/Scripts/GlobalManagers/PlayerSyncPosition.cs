using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSyncPosition : BasePlayerSyncPosition
{
    public override void Initialize(BasePlayersPublicInfoManager basePlayersPublicInfoManager)
    {
        this.basePlayersPublicInfoManager = basePlayersPublicInfoManager;
        
        BaseItemThrowable.OnItemCallbackAction += BaseItemThrowableOnOnItemCallbackAction;
    }

    private void BaseItemThrowableOnOnItemCallbackAction()
    {
        SyncPlayersPositionServerRpc();
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void SyncPlayersPositionServerRpc()
    {
        Debug.Log($"Syncing Players Position");
        Dictionary<PlayableState, GameObject> players = basePlayersPublicInfoManager.GetAllPlayers();

        foreach (GameObject player in players.Values)
        {
            PlayerGetUp playerGetUp = player.GetComponent<PlayerGetUp>();
            playerGetUp.SyncPosWithServerRpc();
        }
        
        InvokeOnPositionSyncedWithServer();
    }
}
