using System.Collections.Generic;
using UnityEngine;

public class ServerAuthenticationService : IServerAuthenticationService
{

    private Dictionary<string, ulong> authToClientId = new Dictionary<string, ulong>(); 

    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>(); 

    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>(); 

    private Dictionary<PlayableState, ulong> playableStateToClientId = new Dictionary<PlayableState, ulong>(); 

    private List<PlayerData> playerDatas = new List<PlayerData>();

    private Dictionary<ulong, PlayerData> clientIdToPlayerData = new Dictionary<ulong, PlayerData>();

    public List<PlayerData> PlayerDatas => playerDatas;
    public Dictionary<ulong, PlayerData> ClientIdToPlayerData => clientIdToPlayerData;
    public int RegisteredClientCount => clientIdToAuth.Count;

    public Dictionary<string, ulong>.ValueCollection AuthToClientIdValues => authToClientId.Values;

    public void RegisterClient(PlayerData playerData)
    {
        //if dont exist, add
        clientIdToAuth[playerData.clientId] = playerData.userData.userAuthId;
        authToClientId[playerData.userData.userAuthId] = playerData.clientId;
        authIdToUserData[playerData.userData.userAuthId] = playerData.userData;

        playerDatas.Add(playerData);
        clientIdToPlayerData[playerData.clientId] = playerData;
    }

    public void RegisterPlayableClient(PlayerData playerData)
    {
        playableStateToClientId[playerData.playableState] = playerData.clientId;
    }

    public void UnregisterClient(ulong clientId)
    {
        if(clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            authToClientId.Remove(authId);
            clientIdToAuth.Remove(clientId);
            authIdToUserData.Remove(authId);
        }
    }

    public UserData GetUserDataByClientId(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            //Get Auth by client ID
            if (authIdToUserData.TryGetValue(authId, out UserData userData))
            {
                return userData;
            }
        }
        return null;
    }

    public string GetAuthIdByClientId(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            return authId;
        }
        return null;
    }

    public ulong GetClientIdByAuthId(string authId)
    {
        if (authToClientId.TryGetValue(authId, out ulong clientId))
        {
            return clientId;
        }
        return 0;
    }

}
