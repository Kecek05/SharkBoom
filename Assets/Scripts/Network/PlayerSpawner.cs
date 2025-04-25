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

    private readonly NetworkObject playerPrefab;
    private readonly bool switchOrder;

    private int playerSpawned = 0;

    public int PlayerCount => playerSpawned;

    private BasePlayersPublicInfoManager playersPublicInfo;
    public PlayerSpawner(NetworkObject _playerPrefab)
    {
        playerPrefab = _playerPrefab;
        switchOrder = UnityEngine.Random.value > 0.5f; // Randomly switching the value false or true for bool basead on 50% chance
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

        playerSpawned++;

        playersPublicInfo = ServiceLocator.Get<BasePlayersPublicInfoManager>();
        Transform randomSpawnPointSelected = playersPublicInfo.GetRandomSpawnPoint();

        NetworkObject playerInstance = GameObject.Instantiate(playerPrefab, randomSpawnPointSelected.position, Quaternion.identity);

        

        playerInstance.Spawn(true);
        playerInstance.DontDestroyWithOwner = true;

        playerInstance.GetComponent<PlayerThrower>().InitializePlayerRpc(GetPlayableStateByCount(), randomSpawnPointSelected.rotation);

        PlayerRandomizeVisual visual = playerInstance.GetComponentInChildren<PlayerRandomizeVisual>();


        if (visual == null)
        {
            Debug.LogError("PlayerRandomizeVisual is null");
        }
        else
        {
            PlayerVisualType visualType;

            if (PlayerCount == 1)
            {
                visualType = switchOrder ? PlayerVisualType.Orca : PlayerVisualType.Shark;
                playersPublicInfo.SetPlayerVisualType(GetPlayableStateByCount(), visualType);
                visual.SetVisualNetworked(visualType);
            }
            else if (PlayerCount == 2)
            {
                visualType = switchOrder ? PlayerVisualType.Shark : PlayerVisualType.Orca;
                playersPublicInfo.SetPlayerVisualType(GetPlayableStateByCount(), visualType);
                visual.SetVisualNetworked(visualType);
            }
        }

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
