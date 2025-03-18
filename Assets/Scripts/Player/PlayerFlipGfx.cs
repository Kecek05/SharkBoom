using Sortify;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerFlipGfx : DragListener
{
    [BetterHeader("References")]
    [SerializeField] private Transform playerGfxTransform;
    [SerializeField] private NetworkTransform[] networkTransforms;

    [Tooltip("Value to be add to not rotate the object to close to the 90 degrees")]
    [SerializeField] private float angleOffset = 0.5f;
    private Vector3 startEulerAngles;

    protected override void DoOnSpawn()
    {
        //startEulerAngles = playerGfxTransform.eulerAngles;
    }

    protected override void DoOnDragChange()
    {


        if (player.PlayerDragController.GetOpositeFingerPos().x > playerGfxTransform.position.x + angleOffset)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, -90f , transform.eulerAngles.z);
        else if (player.PlayerDragController.GetOpositeFingerPos().x < playerGfxTransform.position.x - angleOffset)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 90f, transform.eulerAngles.z);
    }


    private void ChangeToServerAuthoritative()
    {
        foreach (NetworkTransform networkTransform in networkTransforms)
        {
            networkTransform.AuthorityMode = NetworkTransform.AuthorityModes.Server;


        }
    }

    private void ChangeToClientAuthoritative()
    {
        foreach(NetworkTransform networkTransform in networkTransforms)
        {
            networkTransform.AuthorityMode = NetworkTransform.AuthorityModes.Owner;
        }
    }

    protected override void DoOnDragRelease()
    {
        //playerGfxTransform.eulerAngles = startEulerAngles;


    }

}
