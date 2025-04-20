using QFSW.QC;
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : IPlayerSpawner
{
    /// <summary>
    /// Called when a player is spawned. Passes how many players are spawned. Server Call This.
    /// </summary>
    public static event Action<int> OnPlayerSpawned;

    private readonly NetworkObject[] playerPrefabs;

    private int playerSpawned = 0;
    private int firstPlayerIndex;

    public int PlayerCount => playerSpawned;
    public NetworkObject[] PlayerPrefabs => playerPrefabs;

    public PlayerSpawner(NetworkObject[] _playerPrefabs)
    {
        playerPrefabs = _playerPrefabs;
        firstPlayerIndex = UnityEngine.Random.Range(0, 2);
    }


    /// <summary>
    /// Call this to spawn a player.
    /// </summary>
    public void SpawnPlayer()
    {
        if(playerSpawned >= 2)
        {
            Debug.Log("SpawnPlayer, Already Spawned both players");
            return;
        }

        int prefabIndex = (playerSpawned == 0) ? firstPlayerIndex : 1 - firstPlayerIndex;
        playerSpawned++;

        Transform randomSpawnPointSelected = ServiceLocator.Get<BasePlayersPublicInfoManager>().GetRandomSpawnPoint();

        
        
        NetworkObject playerInstance = GameObject.Instantiate(playerPrefabs[prefabIndex], randomSpawnPointSelected.position, Quaternion.identity);


        playerInstance.Spawn(true);
        playerInstance.DontDestroyWithOwner = true;

        playerInstance.GetComponent<PlayerThrower>().InitializePlayerRpc(GetPlayableStateByCount(), randomSpawnPointSelected.rotation);

        //PlayerData playerData = NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.ClientIdToPlayerData[clientId];

        //playerData.gameObject = playerInstance.gameObject;
        //playerData.playableState = GetPlayableStateByCount();

        Debug.Log($"Spawning Player, PlayableState: {GetPlayableStateByCount()} Selected Random SpawnPoint: {randomSpawnPointSelected.name} - Player Spawned: {PlayerCount}");

        OnPlayerSpawned?.Invoke(playerSpawned);
    }

    public PlayableState GetPlayableStateByCount()
    {
        Debug.Log($"GetPlayableStateByCount - {PlayerCount}");

        if (PlayerCount == 1)
        {
            return PlayableState.Player1Playing;
        }
        else if (PlayerCount == 2)
        {
            return PlayableState.Player2Playing;
        }
        else
        {
            Debug.LogError($"GetPlayableStateByCount - PlayerCount is not 1 or 2 - it is {PlayerCount}");
            return PlayableState.None;
        }
    }

}
