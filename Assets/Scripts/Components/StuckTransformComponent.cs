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

        Stuck(collision.transform);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;

        Stuck(collision.transform);
    }

    private void Stuck(Transform stuckParent)
    {
        if(!stucked)
        {
            stucked = true;

            //FIX THAT
            Rigidbody[] rigidBodies = stuckParent.GetComponentsInChildren<Rigidbody>();

            if(rigidBodies == null || rigidBodies.Length == 0)
            {
                Debug.Log("No rigidbodies found in the parent");
                return;
            }

            Rigidbody stuckTo = rigidBodies.OrderBy(Rigidbody => Vector3.Distance(Rigidbody.position, transform.position)).First(); // we order all rbs by distance to hitpoint and take the first one

            followTransformComponent.SetTarget(stuckTo.transform);
            followTransformComponent.SetFollowRotation(false);
            followTransformComponent.EnableComponent();
            Debug.Log($"Stuck to {stuckTo.name}");
        }
    }

}
