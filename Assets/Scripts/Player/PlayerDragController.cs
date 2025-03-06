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
        player.OnPlayerCanPlay += Player_OnPlayerCanPlay;
        player.OnPlayerCantPlay += Player_OnPlayerCantPlay;

    }



    private void Player_OnPlayerCanPlay()
    {
        TurnOnDrag();
    }

    private void Player_OnPlayerCantPlay()
    {
        TurnOffDrag();
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
