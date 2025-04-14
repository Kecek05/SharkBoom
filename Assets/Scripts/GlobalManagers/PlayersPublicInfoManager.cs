using UnityEngine;

public class PlayersPublicInfoManager : BasePlayersPublicInfoManager
{
    public override void Initialize(ItemsListSO itemsListSO)
    {
        this.itemsListSO = itemsListSO;
    }

    public override void AddPlayerToPlayersDictionary(PlayableState playerPlayableState, GameObject playerGameObject)
    {
        if (playerPlayableState == PlayableState.None) return; //Dont add None to the dictionary

        playerStateToGameObject.TryAdd(playerPlayableState, playerGameObject);

        Debug.Log($"Added player to playersDictionary, PlayableState: {playerPlayableState} - GameObject: {playerGameObject.name}");

    }

    public override GameObject GetPlayerObjectByPlayableState(PlayableState playerPlayableState)
    {
        if (playerStateToGameObject.ContainsKey(playerPlayableState))
        {
            return playerStateToGameObject[playerPlayableState];
        }
        else
        {
            Debug.LogWarning("Player not found in dictionary");
            return null;
        }
    }

    public override void RandomizePlayerItems()
    {
        //int itemsInInventory = UnityEngine.Random.Range(2, itemsListSO.allItemsSOList.Count); //Random qtd of items for now
        int itemsInInventory = itemsListSO.allItemsSOList.Count; //all items

        //Add Jump item first
        foreach (PlayerInventory playerInventory in FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None))
        {
            playerInventory.SetPlayerItems(0);
        }

        for (int i = 0; i < itemsInInventory; i++)
        {
            int randomItemSOIndex = UnityEngine.Random.Range(1, itemsListSO.allItemsSOList.Count); //Start from index 1,index 0 is jump

            foreach (PlayerInventory playerInventory in FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None))
            {
                playerInventory.SetPlayerItems(randomItemSOIndex);
            }
        }
    }

    /// <summary>
    /// Get a random Spawnpoint from the list and remove it.
    /// </summary>
    /// <returns></returns>
    public override Transform GetRandomSpawnPoint()
    {
        Transform selectedSpawnPoint = spawnPointsPos[Random.Range(0, spawnPointsPos.Count)];
        spawnPointsPos.Remove(selectedSpawnPoint);
        return selectedSpawnPoint;
    }

    public override void AddRandomSpawnPoint(Transform transformToAdd)
    {
        spawnPointsPos.Add(transformToAdd);
    }
}


