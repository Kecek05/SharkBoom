using System.Collections.Generic;
using UnityEngine;

public class ServerAuthenticationService : IServerAuthenticationService
{

    private Dictionary<string, ulong> authToClientId = new Dictionary<string, ulong>(); // save authentication IDs to their client IDs
    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>(); // save client IDs to their authentication IDs
    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>(); // save authentication IDs to user data

    public int RegisteredClientCount => clientIdToAuth.Count;

    public Dictionary<string, ulong>.ValueCollection AuthToClientIdValues => authToClientId.Values;

    public void RegisterClient(ulong clientId, UserData userData)
    {
        //if dont exist, add to dictionary
        clientIdToAuth[clientId] = userData.userAuthId;
        authToClientId[userData.userAuthId] = clientId;
        authIdToUserData[userData.userAuthId] = userData;
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
