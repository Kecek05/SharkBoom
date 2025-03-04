using QFSW.QC;
using Unity.Netcode;
using UnityEngine;

public class GameFlowManager : NetworkBehaviour
{

    [SerializeField] private ItemsListSO itemsListSO;


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
}
