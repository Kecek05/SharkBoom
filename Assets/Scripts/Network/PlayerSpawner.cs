using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : IPlayerSpawner
{
    /// <summary>
    /// Called when a player is spawned. Passes how many players are spawned. Server Call This.
    /// </summary>
    public static event Action<int> OnPlayerSpawned;

    private readonly NetworkObject playerPrefab;

    private int playerSpawned = 0;

    public int PlayerCount => playerSpawned;

    public PlayerSpawner(NetworkObject _playerPrefab)
    {
        playerPrefab = _playerPrefab;
    }


    /// <summary>
    /// Call this to spawn a player.
    /// </summary>
    /// <param name="clientId"> Id of the player</param>
    /// <param name="playerNumber"> player playing state</param>
    public void SpawnPlayer(ulong clientId)
    {
        playerSpawned++;


        Transform randomSpawnPointSelected = ServiceLocator.Get<BasePlayersPublicInfoManager>().GetRandomSpawnPoint();

        NetworkObject playerInstance = GameObject.Instantiate(playerPrefab, randomSpawnPointSelected.position, Quaternion.identity);

        playerInstance.SpawnAsPlayerObject(clientId);
        playerInstance.DontDestroyWithOwner = true;

        playerInstance.GetComponent<PlayerThrower>().InitializePlayerRpc(GetPlayableStateByCount(), randomSpawnPointSelected.rotation, NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.ClientIdToAuth[clientId]);

        PlayerData playerData = NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.ClientIdToPlayerData[clientId];

        playerData.gameObject = playerInstance.gameObject;
        playerData.playableState = GetPlayableStateByCount();

        OnPlayerSpawned?.Invoke(playerSpawned);

        Debug.Log($"Spawning Player, Client Id: {clientId} PlayableState: {GetPlayableStateByCount()} Selected Random SpawnPoint: {randomSpawnPointSelected.name}");
    }

    public PlayableState GetPlayableStateByCount()
    {
        return PlayerCount == 1 ? PlayableState.Player1Playing : PlayableState.Player2Playing;
    }

}
