using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine; 

public class PlayerRagdollEnabler : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform ragdollRoot;

    private Rigidbody[] ragdollRbs;
    private Collider[] ragdollColliders;

    //[SerializeField] private bool debugRagdollEnabler;
    //[SerializeField] private bool debugRagdollDisabler;

    public override void OnNetworkSpawn()
    {
        ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        ragdollColliders = ragdollRoot.GetComponentsInChildren<Collider>();
    }

    //// JUST FOR DEBUG ON RAGDOLL SCENE
    //private void Awake()
    //{
    //    ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
    //    ragdollColliders = ragdollRoot.GetComponentsInChildren<Collider>();
    //}

    //private void Update()
    //{
    //    if (debugRagdollEnabler)
    //    {
    //        debugRagdollEnabler = false;
    //        DisableRagdoll();
    //    }

    //    if (debugRagdollDisabler)
    //    {
    //        debugRagdollDisabler = false;
    //        EnableRagdoll();
    //    }
    //}

    public void IniatilizeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction += HandleOnItemCallbackAction;
    }

    private void HandleOnItemCallbackAction()
    {
        if (IsOwner)
        {
            RequestRagdollDisableServerRpc();
        }
    }

    public void TriggerRagdoll(float knockbackStrength, Vector3 hitPoint)
    {
        RequestRagdollServerRpc();

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

        Debug.Log($"Force: {force}");
        AddForceToOtherServerRpc(closestIndex, force, hitPoint);
    }

    [Rpc(SendTo.Server)]
    private void AddForceToOtherServerRpc(int hitRigidbodyIndex, Vector3 force, Vector3 hitPoint)
    {
        AddForceToOtherClientRpc(hitRigidbodyIndex, force, hitPoint);
    }

    [Rpc(SendTo.Owner)]
    private void AddForceToOtherClientRpc(int hitRigidbodyIndex, Vector3 force, Vector3 hitPoint)
    {
        Rigidbody hitRigidbody = ragdollRbs[hitRigidbodyIndex]; // get the rb we hit
        hitRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
    }


    [Rpc(SendTo.Server)]
    private void RequestRagdollServerRpc()
    {
        EnableRagdollClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EnableRagdollClientRpc()
    {
        EnableRagdoll();
    }

    private void EnableRagdoll()
    {
        Debug.Log("Hit trigger - Here we be enable to cache original pos, compare if the time is the same");
        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = false;
        }

        foreach (Collider ragdollCollider in ragdollColliders)
        {
            ragdollCollider.enabled = true;
        }

        animator.enabled = false;
    }


    [Rpc(SendTo.Server)]
    private void RequestRagdollDisableServerRpc()
    {
        DisableRagdollClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DisableRagdollClientRpc()
    {
        DisableRagdoll();
    }


    private void DisableRagdoll()
    {
        animator.enabled = true;

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = true;
        }

        foreach (Collider ragdollCollider in ragdollColliders)
        {
            ragdollCollider.enabled = false;
        }

    }

    public void UnInitializeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction += HandleOnItemCallbackAction;
    }

}
