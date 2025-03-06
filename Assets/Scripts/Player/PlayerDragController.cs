using Sortify;
using Unity.Netcode;
using UnityEngine;

public class PlayerDragController : DragAndShoot
{
    [BetterHeader("References")]
    [SerializeField] private Player player;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        player.PlayerDragController.OnDragRelease += PlayerDragController_OnDragRelease;
    }

    private void PlayerDragController_OnDragRelease()
    {
        ResetDrag();
    }


    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        player.PlayerDragController.OnDragRelease -= PlayerDragController_OnDragRelease;
    }
}
