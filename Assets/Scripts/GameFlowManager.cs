using QFSW.QC;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameFlowManager : NetworkBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [SerializeField] private ItemsListSO itemsListSO;
    [SerializeField] private List<Transform> spawnPointsPos;

    private void Awake()
    {
        Instance = this;
    }

    [Command("gameFlowManager-randomizePlayersItems")]
    public void RandomizePlayerItems()
    {
        int itemsInInventory = Random.Range(2, itemsListSO.allItemsSOList.Count); //Random qtd of items for now

        for(int i = 0; i < itemsInInventory; i++)
        {
            int randomItemSOIndex = Random.Range(0, itemsListSO.allItemsSOList.Count);

            foreach (PlayerInventory playerInventory in FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None))
            {
                playerInventory.SetPlayerItems(randomItemSOIndex);
                Debug.Log($"Player: {playerInventory.gameObject.name}");
            }
        }
    }

    public Vector3 GetRandomSpawnPoint()
    {
        Transform selectedSpawnPoint = spawnPointsPos[Random.Range(0, spawnPointsPos.Count)];
        spawnPointsPos.Remove(selectedSpawnPoint);
        return selectedSpawnPoint.position;
    }
}
