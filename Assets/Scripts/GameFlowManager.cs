using NUnit.Framework;
using QFSW.QC;
using Unity.Netcode;
using UnityEngine;

public class GameFlowManager : NetworkBehaviour
{

    [Command("gameFlowManager-randomizePlayersItems")]
    public void RandomizePlayerItems()
    {
        foreach (PlayerInventory playerInventory in FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None))
        {
            playerInventory.RandomItemServerDebug();
        }
    }
}
