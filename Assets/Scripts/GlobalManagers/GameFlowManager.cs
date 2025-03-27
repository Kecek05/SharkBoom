using QFSW.QC;
using Sortify;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Unity.Netcode;
using UnityEngine;

public class GameFlowManager : NetworkBehaviour
{
    private static GameFlowManager instance;
    public static GameFlowManager Instance => instance;

    [BetterHeader("References")]
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private ItemsListSO itemsListSO;
    [SerializeField] private List<Transform> spawnPointsPos;

    //Publics
    public TurnManager TurnManager => turnManager;
    public GameStateManager GameStateManager => gameStateManager;


    private void Awake()
    {
        instance = this;

        //CalculatePearlsManager.Reset();
    }

    /// <summary>
    /// Call this to randomize and give items to players
    /// </summary>
    public void RandomizePlayerItems()
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
    public Transform GetRandomSpawnPoint()
    {
        Transform selectedSpawnPoint = spawnPointsPos[UnityEngine.Random.Range(0, spawnPointsPos.Count)];
        spawnPointsPos.Remove(selectedSpawnPoint);
        return selectedSpawnPoint;
    }




}




