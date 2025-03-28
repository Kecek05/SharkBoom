using System.Collections.Generic;
using UnityEngine;

public abstract class BasePlayersPublicInfoManager : MonoBehaviour
{
    //REFACTOR THIS BASED ON PLAYERDATA
    public static int PLAYER_1_LAYER = 8;
    public static int PLAYER_2_LAYER = 9;

    protected Dictionary<PlayableState, GameObject> playerStateToGameObject = new Dictionary<PlayableState, GameObject>();

    public abstract void AddPlayerToPlayersDictionary(PlayableState playerPlayableState, GameObject playerGameObject);

    public abstract GameObject GetPlayerObjectByPlayableState(PlayableState playerPlayableState);
}



public enum PlayableState
{
    None,
    Player1Playing, //Player 1 Can Play
    Player1Played, //Player 1 Cant Play
    Player2Playing, //Player 2 Can Play
    Player2Played, //Player 2 Cant Play
}
