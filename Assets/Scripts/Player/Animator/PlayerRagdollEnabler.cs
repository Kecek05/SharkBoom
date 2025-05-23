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

    private Quaternion originalHipRotation;
    private Quaternion originalRootRotation;
    private Quaternion ragdollRootRotation;

    private bool isFallen = false;


    public void InitializeOwner()
    {
        if (!IsOwner) return;


        ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        ragdollColliders = ragdollRoot.GetComponentsInChildren<Collider>();
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
        }
    }

    private void HandleItemCallback()
    {

    }

    public void HandleOnPlayerStateChanged(PlayerState state)
    {
        if (state == PlayerState.IdleEnemyTurn || state == PlayerState.IdleMyTurn)
        {
            if(IsOwner)
            {
                if (isFallen)
                {
                    RequestRagdollDisableServerRpc();
                }
            }
        }
    }


    public void TriggerRagdoll(Vector3 force, Vector3 hitPoint)
    {
        RequestRagdollServerRpc();

        Rigidbody hitRigidbody = ragdollRbs.OrderBy(Rigidbody => Vector3.Distance(Rigidbody.position, hitPoint)).First(); // we order all rbs by distance to hitpoint and take the first one
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

        if (ragdollRbs == null)
        {
            ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        }

        if(ragdollColliders == null)
        {
            ragdollColliders = ragdollRoot.GetComponentsInChildren<Collider>();
        }

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

    public void UnInitializeOwner()
    {
        if (!IsOwner) return;

        PlayerHealth.OnPlayerTakeDamage -= HandleOnPlayerTakeDamage;
    }
}
