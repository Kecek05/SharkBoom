using System.Collections.Generic;
using UnityEngine;

public class PlayersPublicInfoManager : BasePlayersPublicInfoManager
{
    private const int JUMP_ITEM_ID = 0; 
    
    public override void Initialize(ItemsListSO itemsListSO)
    {
        this.itemsListSO = itemsListSO;
    }

    public override void AddPlayerToPlayersDictionary(PlayableState playerPlayableState, GameObject playerGameObject)
    {
        if (playerPlayableState == PlayableState.None) return; //Dont add None to the dictionary

        playerStateToGameObject.TryAdd(playerPlayableState, playerGameObject);

        //Debug.Log($"Added player to playersDictionary, PlayableState: {playerPlayableState} - GameObject: {playerGameObject.name}");

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

    public override GameObject GetOtherPlayerByMyPlayableState(PlayableState myPlayableState)
    {

        foreach (PlayableState playableState in playerStateToGameObject.Keys)
        {
            if (playableState != myPlayableState)
            {
                return playerStateToGameObject[playableState];
            }
        }

        Debug.LogWarning("Not found the other player");
        return null;
    }

    public override Dictionary<PlayableState, GameObject> GetAllPlayers()
    {
        return playerStateToGameObject;
    }

    public override void RandomizePlayerItems()
    {
        //int itemsInInventory = UnityEngine.Random.Range(2, itemsListSO.allItemsSOList.Count); //Random qtd of items for now
        //List<ItemSO> itemsInInventoryPool = itemsListSO.allItemsSOList; //all items
        List<ItemSO> itemsAdded = new List<ItemSO>();

        //Add Jump item first
        foreach (PlayerInventory playerInventory in FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None))
        {
            playerInventory.AddPlayerItemsServerRpc(JUMP_ITEM_ID);
        }

        int itemsAddedToInventory = 0;
        int itemsToAddToInventory = itemsListSO.allItemsSOList.Count - 1; //-1 because we already added Jump item

        while(itemsAddedToInventory < itemsToAddToInventory)
        {
            int randomItemIndex = Random.Range(1, itemsListSO.allItemsSOList.Count); //start from 1 to skip Jump item
            
            ItemSO randomItemSO = itemsListSO.allItemsSOList[randomItemIndex];
            
            if (itemsAdded.Contains(randomItemSO))
            {
                continue; //If the item is already added, skip to next iteration
            }

            foreach (PlayerInventory playerInventory in FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None))
            {
                playerInventory.AddPlayerItemsServerRpc(randomItemSO.itemID);
            }

            itemsAdded.Add(randomItemSO);

            itemsAddedToInventory++;
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

    public override Dictionary<PlayableState, PlayerVisualType> GetPlayerVisualTypes()
    {
        return playerVisualByState;
    }

    public override void SetPlayerVisualType(PlayableState playerPlayableState, PlayerVisualType playerVisualType)
    {
        playerVisualByState[playerPlayableState] = playerVisualType;
    }

}


