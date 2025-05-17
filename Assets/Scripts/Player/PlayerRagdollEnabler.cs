using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerRagdollEnabler : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerGetUp playerGetUp;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private Transform rootTransform;
    [SerializeField] private Transform hipsTransform;

    [SerializeField] private Rigidbody[] ragdollRbs;
    [SerializeField] private Collider[] ragdollColliders;

    private float verticalOffset = 0f;

    private Quaternion originalHipRotation;
    private Quaternion originalRootRotation;
    private Quaternion ragdollRootRotation;

    private bool isFallen = false;

    [SerializeField] private bool debugRagdollEnabler;
    [SerializeField] private bool debugRagdollDisabler;

    public override void OnNetworkSpawn()
    {
        ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        ragdollColliders = ragdollRoot.GetComponentsInChildren<Collider>();
    }

    private void Update()
    {
        if (debugRagdollEnabler)
        {
            debugRagdollEnabler = false;
            DisableRagdoll();
        }

        if (debugRagdollDisabler)
        {
            debugRagdollDisabler = false;
            EnableRagdoll();
        }
    }

    private void HandleOnItemCallbackAction()
    {
        if (IsOwner)
        {
            if (isFallen)
            {
                RequestRagdollDisableServerRpc();
            }
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

        if(closestIndex == -1)
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
        originalHipRotation = hipsTransform.rotation;
        originalRootRotation = rootTransform.rotation;
        ragdollRootRotation = ragdollRoot.rotation;

        verticalOffset = hipsTransform.position.y - rootTransform.position.y;

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = false;
        }

        foreach (Collider ragdollCollider in ragdollColliders)
        {
           ragdollCollider.enabled = true;
        }

        isFallen = true;
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

        if(isFallen == true)
        {
            AlignPositionToHips();
        }

        isFallen = false;
    }

    private void AlignPositionToHips()
    {
        Vector3 newPosition = hipsTransform.position; 
        newPosition.y -= verticalOffset;  // correcting the y axis 

        if (Physics.Raycast(hipsTransform.position, Vector3.down, out RaycastHit hit, 5f)) // check if we hit the ground for not reset in the ground
        {
            float groundY = hit.point.y;

            if (newPosition.y < groundY)
            {
                newPosition.y = groundY;
            }
        }


        // Send all for original rotation, basead on new position
        rootTransform.SetPositionAndRotation(newPosition, originalRootRotation);
        hipsTransform.rotation = originalHipRotation;
        ragdollRoot.rotation = ragdollRootRotation;
    }
}
