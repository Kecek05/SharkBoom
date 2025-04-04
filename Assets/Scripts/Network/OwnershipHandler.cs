using Unity.Netcode;
using UnityEngine;

public class OwnershipHandler
{

    /// <summary>
    /// return if the player is reconnecting to the game.
    /// </summary>
    public static bool IsReconnecting(ulong clientId)
    {
        if(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.GetAuthIdByClientId(clientId) != null)
        {
            //Player already registered
            return true;
        } else
        {
            //New Player
            return false;
        }
    }


    /// <summary>
    /// Handle the ownership of the player object when reconnected.
    /// </summary>
    public static void HandleOwnership(UserData userData, ulong clientId)
    {
        if (NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.AuthIdToPlayerData.TryGetValue(userData.userAuthId, out PlayerData playerData))
        {
            //Update clientId
            playerData = NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.AuthIdToPlayerData[userData.userAuthId];

            playerData.clientId = clientId;

            //Change Onwership
            ServiceLocator.Get<BasePlayersPublicInfoManager>().GetPlayerObjectByPlayableState(playerData.playableState).GetComponent<NetworkObject>().ChangeOwnership(clientId);

            Debug.Log($"Changing the Ownership of the object: {ServiceLocator.Get<BasePlayersPublicInfoManager>().GetPlayerObjectByPlayableState(playerData.playableState).name} to ClientId: {clientId}");
        }
        else
        {
            //Client is not registered, but already spawned
            Debug.LogWarning("CheckReconnect, Client is not registered, but already spawned, this is a error!");
        }
    }

    /// <summary>
    /// Handle the ownership of the player object when a new client joins.
    /// </summary>
    public static void HandleClientJoinPlayerOwnership(UserData userData)
    {
        
    }
}
