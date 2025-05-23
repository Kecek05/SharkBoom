using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public static class OwnershipHandler
{
    /// <summary>
    /// Called when the client gain ownership of the player object. Pass the new owner clientId
    /// </summary>
    public static event Action<ulong> OnClientGainOwnership;

    /// <summary>
    /// return if the player is reconnecting to the game. Server Only!
    /// </summary>
    public static bool IsReconnecting(string authId)
    {
        if(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.GetPlayerDataByAuthId(authId) != null)
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
    /// <param name="clientId"> New client Id of the player</param>
    public static void HandleOwnership(string authId, ulong clientId)
    {
        if (NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.AuthIdToPlayerData.TryGetValue(authId, out PlayerData playerData))
        {
            //Update clientId
            playerData = NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.AuthIdToPlayerData[authId];

            playerData.clientId = clientId;

            //Change Onwership

            BasePlayersPublicInfoManager playersPublicInfoManager = ServiceLocator.Get<BasePlayersPublicInfoManager>();

            if (playersPublicInfoManager != null)
            {
                GameObject playerObj = playersPublicInfoManager.GetPlayerObjectByPlayableState(playerData.playableState);

                if (playerObj != null)
                {
                    NetworkObject netObj = playerObj.GetComponent<NetworkObject>();
                    if (netObj != null)
                    {
                        netObj.ChangeOwnership(clientId);
                        OnClientGainOwnership?.Invoke(clientId);
                    }
                    else
                    {
                        Debug.LogError("NetworkObject not found on player object.");
                    }
                }
                else
                {
                    Debug.LogError($"playerObj is null for playableState: {playerData.playableState}");
                }
            }
            else
            {
                Debug.LogError("playersPublicInfoManager is null (not found in ServiceLocator).");
            }
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
    public static async void HandleClientJoinPlayerOwnership(PlayerData playerData)
    {
        Debug.Log($"HandleClientJoinPlayerOwnership, Called");
        while(!NetworkServerProvider.Instance.CurrentNetworkServer.CanChangeOwnership)
        {
            await Task.Delay(100);
        }
        Debug.Log($"HandleClientJoinPlayerOwnership, Can change ownership, player count is: {NetworkServerProvider.Instance.CurrentNetworkServer.PlayerSpawner.PlayerCount}, changing ownership");

        HandleOwnership(playerData.userData.userAuthId, playerData.clientId);

        GameObject playerGameObject = ServiceLocator.Get<BasePlayersPublicInfoManager>().GetPlayerObjectByPlayableState(playerData.playableState);

        playerData.gameObject = playerGameObject;

    }
}
