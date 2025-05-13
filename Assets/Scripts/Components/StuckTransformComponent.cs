using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class StuckTransformComponent : NetworkBehaviour
{
    [SerializeField] private FollowTransformComponent followTransformComponent;
    private bool stucked = false; //stuck only once
    [SerializeField] private GameObject gfx;
    [SerializeField] private Vector3 knockback;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsOwner) return;

        if(stucked) return;

        PlayerRagdollEnabler playerRagdollEnabler = collision.gameObject.GetComponentInChildren<PlayerRagdollEnabler>();

        if (playerRagdollEnabler)
        {
            Stuck(collision.transform, playerRagdollEnabler);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;

        if (stucked) return;

        PlayerRagdollEnabler playerRagdollEnabler = collision.gameObject.GetComponentInChildren<PlayerRagdollEnabler>();

        if (playerRagdollEnabler)
        {
            Stuck(collision.transform, playerRagdollEnabler);
        }
    }

    private void Stuck(Transform collidedTransform, PlayerRagdollEnabler playerRagdollEnabler)
    {
        if(!stucked)
        {
            stucked = true;

            GameObject objectToStuck = null;
            playerRagdollEnabler.TriggerRagdoll(knockback, collidedTransform.position, (rb) => { objectToStuck = rb.gameObject; });

            if(objectToStuck == null)
            {
                Debug.Log("Object to stuck is null");
                return;
            }

            followTransformComponent.SetTarget(objectToStuck.transform);
            followTransformComponent.SetFollowRotation(false);
            followTransformComponent.EnableComponent();
            Debug.Log($"Stuck to {objectToStuck.name}");
            HideGfxServerRpc();

        }
    }
    [Rpc(SendTo.Server)]
    private void HideGfxServerRpc()
    {
        HideGfxClientRpc();
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void HideGfxClientRpc()
    {
        gfx.SetActive(false);
    }

}
