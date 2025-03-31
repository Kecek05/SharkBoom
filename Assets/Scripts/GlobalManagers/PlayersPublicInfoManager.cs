using UnityEngine;

public class PlayersPublicInfoManager : BasePlayersPublicInfoManager
{
    public override void AddPlayerToPlayersDictionary(PlayableState playerPlayableState, GameObject playerGameObject)
    {
        if (playerPlayableState == PlayableState.None) return; //Dont add None to the dictionary

        playerStateToGameObject.TryAdd(playerPlayableState, playerGameObject);

    }

    public override GameObject GetPlayerObjectByPlayableState(PlayableState playerPlayableState)
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
}


