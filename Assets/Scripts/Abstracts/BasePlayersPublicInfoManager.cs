using System.Collections.Generic;
using UnityEngine;

public abstract class BasePlayersPublicInfoManager : MonoBehaviour
{
    //REFACTOR THIS BASED ON PLAYERDATA
    public static int PLAYER_1_LAYER = 8;
    public static int PLAYER_2_LAYER = 9;

    protected ItemsListSO itemsListSO;
    protected List<Transform> spawnPointsPos = new();

    public abstract void Initialize(ItemsListSO itemsListSO);

    protected Dictionary<PlayableState, GameObject> playerStateToGameObject = new Dictionary<PlayableState, GameObject>();

    public abstract void AddPlayerToPlayersDictionary(PlayableState playerPlayableState, GameObject playerGameObject);

    public abstract GameObject GetPlayerObjectByPlayableState(PlayableState playerPlayableState);

    /// <summary>
    /// Call this to randomize and give items to players
    /// </summary>
    public abstract void RandomizePlayerItems();


    /// <summary>
    /// Get a random Spawnpoint from the list and remove it.
    /// </summary>
    /// <returns></returns>
    public abstract Transform GetRandomSpawnPoint();

    /// <summary>
    /// Add a spawnpoint to the List
    /// </summary>
    /// <param name="transformToAdd"></param>
    public abstract void AddRandomSpawnPoint(Transform transformToAdd);
}



public enum PlayableState
{
    None,
    Tie,
    Player1Playing, //Player 1 Can Play
    Player1Played, //Player 1 Cant Play
    Player2Playing, //Player 2 Can Play
    Player2Played, //Player 2 Cant Play
}
