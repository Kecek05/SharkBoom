using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class StuckTransformComponent : NetworkBehaviour
{
    [SerializeField] private FollowTransformComponent followTransformComponent;
    private bool stucked = false; //stuck only once

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
            playerRagdollEnabler.TriggerRagdoll(new Vector3(150,20,0), collidedTransform.position, (rb) => { objectToStuck = rb.gameObject; });

            if(objectToStuck == null)
            {
                Debug.Log("Object to stuck is null");
                return;
            }

            CalculateStuckOffset(objectToStuck.transform.position);
            followTransformComponent.SetTarget(objectToStuck.transform);
            followTransformComponent.SetFollowRotation(false);
            followTransformComponent.EnableComponent();
            Debug.Log($"Stuck to {objectToStuck.name}");
        }
    }

    private void CalculateStuckOffset(Vector3 touchedPos)
    {
        Vector3 offset = transform.position - touchedPos;
        followTransformComponent.SetPositionOffset(offset);
    }

}
