using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : IPlayerSpawner
{
    /// <summary>
    /// Called when a player is spawned. Passes the player that was spawned. Server Call This
    /// </summary>
    public static event Action<PlayerThrower> OnPlayerSpawned;

    private readonly NetworkObject playerPrefab;

    public PlayerSpawner(NetworkObject _playerPrefab)
    {
        playerPrefab = _playerPrefab;
    }


    /// <summary>
    /// Call this to spawn a player.
    /// </summary>
    /// <param name="clientId"> Id of the player</param>
    /// <param name="playerNumber"> player playing state</param>
    public void SpawnPlayer(ulong clientId, PlayableState playerState)
    {
        Transform randomSpawnPointSelected = GameFlowManager.Instance.GetRandomSpawnPoint();

        NetworkObject playerInstance = GameObject.Instantiate(playerPrefab, randomSpawnPointSelected.position, Quaternion.identity);

        playerInstance.SpawnAsPlayerObject(clientId);

        PlayerThrower playerThrower = playerInstance.GetComponent<PlayerThrower>();

        playerThrower.InitializePlayerRpc(playerState, randomSpawnPointSelected.rotation);

        OnPlayerSpawned?.Invoke(playerThrower);

        Debug.Log($"Spawning Player, Client Id: {clientId} PlayableState: {playerState} Selected Random SpawnPoint: {randomSpawnPointSelected.name}");
    }
}
