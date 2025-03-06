using Sortify;
using Unity.Netcode;
using UnityEngine;

public class PlayerDragController : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Player player;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        player.PlayerInventory.OnItemSOSelected += SetLaunch;
    }


    public void SetLaunch(ItemSO itemSO)
    {
        player.PlayerDragAndShoot.SetDragAndShoot(itemSO.rb);

    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        player.PlayerInventory.OnItemSOSelected -= SetLaunch;
    }
}
