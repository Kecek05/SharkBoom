using Unity.Netcode;
using UnityEngine;

public class PlayerRagdollEnabler : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform ragdollRoot;

    public Rigidbody[] ragdollRbs;


    public void InitializeOwner()
    {
        if (!IsOwner) return;

        ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        DisableRagdoll();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    EnableRagdoll();
        //}

        //if(Input.GetKeyDown(KeyCode.R))
        //{
        //    DisableRagdoll();
        //}
    }

    private void EnableRagdoll()
    {
        animator.enabled = false;

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = false;
        }

    }

    private void DisableRagdoll()
    {
        animator.enabled = true;

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = true;

        }
    }

    public void UnInitializeOwner()
    {
        if (!IsOwner) return;

        DisableRagdoll();
    }

}
