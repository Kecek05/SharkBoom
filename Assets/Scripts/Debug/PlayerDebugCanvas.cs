using Sortify;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerDebugCanvas : NetworkBehaviour
{

    [BetterHeader("References")]
    public Player player;
    public TextMeshProUGUI selectedItemIndexText;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }

        player.PlayerInventory.SelectedItemInventoryIndex.OnValueChanged += SelectedItemIndex_OnValueChanged;
    }

    private void SelectedItemIndex_OnValueChanged(int previousValue, int newValue)
    {
        selectedItemIndexText.text = player.PlayerInventory.SelectedItemInventoryIndex.Value.ToString();
    }

}
