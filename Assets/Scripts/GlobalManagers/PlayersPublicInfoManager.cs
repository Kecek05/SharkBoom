using QFSW.QC;
using System.Collections.Generic;
using UnityEngine;

public class PlayersPublicInfoManager : MonoBehaviour
{
    public static int PLAYER_1_LAYER = 8;
    public static int PLAYER_2_LAYER = 9;

    private static PlayersPublicInfoManager instance;
    public static PlayersPublicInfoManager Instance => instance;

    private Dictionary<PlayableState, GameObject> playerStateToGameObject = new Dictionary<PlayableState, GameObject>();




    private void Awake()
    {
        instance = this;
    }

    public void AddPlayerToPlayersDictionary(PlayableState playerPlayableState, GameObject playerGameObject)
    {
        if (playerPlayableState == PlayableState.None) return; //Dont add None to the dictionary

        playerStateToGameObject.TryAdd(playerPlayableState, playerGameObject);

    }

    public GameObject GetPlayerObjectByPlayableState(PlayableState playerPlayableState)
    {
        if (playerStateToGameObject.ContainsKey(playerPlayableState))
        {
            return playerStateToGameObject[playerPlayableState];
        }
        else
        {
            Debug.LogError("Player not found in dictionary");
            return null;
        }
    }

    [Command("playersPublicInfoManager-debugGetPlayer")] //DEBUG
    private void DebugGetPlayer(PlayableState playableState)
    {
        Debug.Log("Getting Player: " + playerStateToGameObject[playableState].name);
    }
}

public enum PlayableState
{
    None,
    Player1Playing, //Player 1 Can Play
    Player1Played, //Player 1 Cant Play
    Player2Playing, //Player 2 Can Play
    Player2Played, //Player 2 Cant Play
}
