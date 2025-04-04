using System;
using Unity.Netcode;
using UnityEngine;

public static class OwnershipHandler
{
    /// <summary>
    /// Called when the client gain ownership of the player object for the first time.
    /// </summary>
    public static event Action OnClientGainOwnership;

    /// <summary>
    /// return if the player is reconnecting to the game. Server Only!
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
    /// Handle the ownership of the player object when reconnected. Server Only!
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
    /// Handle the ownership of the player object when a new client joins. Server Only!
    /// </summary>
    public static void HandleClientJoinPlayerOwnership(PlayerData playerData)
    {
        HandleOwnership(playerData.userData, playerData.clientId);

        GameObject playerGameObject = ServiceLocator.Get<BasePlayersPublicInfoManager>().GetPlayerObjectByPlayableState(playerData.playableState);

        playerData.gameObject = playerGameObject;

        OnClientGainOwnership?.Invoke();
    }
}
