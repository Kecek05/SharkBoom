using Sortify;
using System;
using Unity.Netcode;
using UnityEngine; 

public class PlayerRagdollEnabler : NetworkBehaviour
{
    public event Action OnRagdollDisabled;

    [BetterHeader("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private Collider[] playerColliders;
    [SerializeField] private Rigidbody parentRigidbody;
    [SerializeField] private Transform hips;
    private Rigidbody[] ragdollRbs;

    private Collider[] ragdollColliders;
    private Vector3 defaultHipsRotation = new Vector3(-90f, 180f, 0f);

    [BetterHeader("DEBUG")]
    [SerializeField] private bool debugRagdollEnabler;
    [SerializeField] private bool debugRagdollDisabler;

    public override void OnNetworkSpawn()
    { 
        ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        ragdollColliders = ragdollRoot.GetComponentsInChildren<Collider>();
    }

    // JUST FOR DEBUG ON RAGDOLL SCENE
    //private void Awake()
    //{
    //    ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
    //    ragdollColliders = ragdollRoot.GetComponentsInChildren<Collider>();
    //}

    private void Update()
    {
        if (debugRagdollDisabler)
        {
            debugRagdollDisabler = false;
            DisableRagdoll();
        }

        if (debugRagdollEnabler)
        {
            debugRagdollEnabler = false;
            EnableRagdoll();
        }
    }


    public void HandleOnPlayerGetUp()
    {
        if (!IsOwner) return;

        RequestRagdollDisableServerRpc();
    }

    public void TriggerRagdoll(float knockbackStrength, Vector3 hitPoint)
    {
        Rigidbody hitRigidbody = null;
        float closestDistance = float.MaxValue;
        int index = 0;
        int closestIndex = -1;
        Vector3 force = Vector3.zero;
        Vector3 direction = Vector3.zero;

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            float currentDistance = Vector3.Distance(ragdollRb.position, hitPoint);
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;

                hitRigidbody = ragdollRb;
                closestIndex = index;

                direction = (hitRigidbody.position - hitPoint).normalized;
                force = direction * knockbackStrength;
            }
            index++;
        }

        if (closestIndex == -1)
        {
            Debug.LogError("No ragdoll rb found");
            return;
        }
        TriggerRagdollServerRpc(closestIndex, force, hitPoint);
    }

    [Rpc(SendTo.Server)]
    private void TriggerRagdollServerRpc(int hitRigidbodyIndex, Vector3 force, Vector3 hitPoint)
    {
        TriggerRagdollClientRpc(hitRigidbodyIndex, force, hitPoint);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerRagdollClientRpc(int hitRigidbodyIndex, Vector3 force, Vector3 hitPoint)
    {
        EnableRagdoll();
        Rigidbody hitRigidbody = ragdollRbs[hitRigidbodyIndex]; // get the rb we hit

        force = new Vector3(force.x, force.y, 0f); //Ensure to not knockback in Z
        hitRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
    }

    private void EnableRagdoll()
    {

        foreach (Collider ragdollCollider in ragdollColliders)
        {
            ragdollCollider.enabled = true;
        }

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = false;
        }

        foreach (Collider playerCollider in playerColliders)
        {
            playerCollider.enabled = false;
        }

        parentRigidbody.isKinematic = true;
        animator.enabled = false;
    }


    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void RequestRagdollDisableServerRpc()
    {
        DisableRagdollClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void DisableRagdollClientRpc()
    {
        DisableRagdoll();
    }

    private void DisableRagdoll()
    {
        animator.enabled = true;

        foreach (Collider ragdollCollider in ragdollColliders)
        {
            ragdollCollider.enabled = false;
        }

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = true;
            Debug.Log("Disabled Ragdoll");
        }

        foreach (Collider playerCollider in playerColliders)
        {
            playerCollider.enabled = true;
        }

        parentRigidbody.isKinematic = false;

        hips.localRotation = Quaternion.Euler(defaultHipsRotation);

        OnRagdollDisabled?.Invoke();
    }
}
