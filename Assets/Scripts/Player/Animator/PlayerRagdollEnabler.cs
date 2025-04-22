using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerRagdollEnabler : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private Transform rootTransform;
    [SerializeField] private Transform hipsTransform;

    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Rigidbody[] ragdollRbs;
    [SerializeField] private Collider[] ragdollColliders;

    private float verticalOffset = 0f;

    public void InitializeOwner()
    {
        if (!IsOwner) return;

        CacheRagdollComponents();
        DisableRagdoll();
        // not align and disable ragdoll
    }


    public void HandleOnPlayerTakeDamage(object sender, PlayerHealth.OnPlayerTakeDamageArgs e)
    {
        if (sender != playerHealth) return;

        if (IsOwner)
        {
            // turn on ragdoll
            RequestRagdollServerRpc();
            Debug.Log("Event for enable ragdoll");
        }
    }

    private void HandleItemCallback()
    {

    }

    public void HandleOnPlayerStateChanged(PlayerState state)
    {
        if (state == PlayerState.IdleMyTurn || state == PlayerState.IdleEnemyTurn)
        {
            RequestRagdollDisableServerRpc();
        }
    }

    public void TriggerRagdoll(Vector3 force, Vector3 hitPoint)
    {
        RequestRagdollServerRpc();

        Rigidbody hitRigidbody = ragdollRbs.OrderBy(Rigidbody => Vector3.Distance(Rigidbody.position, hitPoint)).First(); // we order all rbs by distance to hitpoint and take the first one
        hitRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);

        Debug.Log("Trigger Ragdoll");
        // set the state anim for ragdoll (after)
    }


    [Rpc(SendTo.Server)]
    private void RequestRagdollServerRpc()
    {
        EnableRagdollClientRpc();
        Debug.Log("Ragdoll enable SEND TO RPC");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EnableRagdollClientRpc()
    {
        EnableRagdoll();
        Debug.Log("Ragdoll enable RPC");
    }

    private void EnableRagdoll()
    {
        animator.enabled = false;

        CacheRagdollComponents();

        verticalOffset = hipsTransform.position.y - rootTransform.position.y;

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = false;
        }

        foreach (Collider ragdollCollider in ragdollColliders)
        {
            ragdollCollider.enabled = true;
        }

        Debug.Log("Ragdoll enable");
    }


    [Rpc(SendTo.Server)]
    private void RequestRagdollDisableServerRpc()
    {
        DisableRagdollClientRpc();
        Debug.Log("Disable Ragdoll SEND TO RPC");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DisableRagdollClientRpc()
    {
        DisableRagdoll();
        // align to hips
        Debug.Log("Disable Ragdoll RPC");
    }

    private void AlignPositionToHips()
    {
        Vector3 newPosition = hipsTransform.position; // create a new pos basead on actual hips transform position
        newPosition.y -= verticalOffset;  // correcting the y axis to align with the root transform
        rootTransform.position = newPosition;
        rootTransform.rotation = Quaternion.LookRotation(ragdollRoot.forward, Vector3.up);

        // Send gfx for original pos 
        ragdollRoot.localPosition = Vector3.zero;
        ragdollRoot.localRotation = Quaternion.identity;

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

        
        Debug.Log("Ragdoll disable");
    }

    private void CacheRagdollComponents()
    {
        ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        ragdollColliders = ragdollRoot.GetComponentsInChildren<Collider>();
    }

    public void UnInitializeOwner()
    {
        if (!IsOwner) return;

        PlayerHealth.OnPlayerTakeDamage -= HandleOnPlayerTakeDamage;
    }
}
