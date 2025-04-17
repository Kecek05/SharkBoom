using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerRagdollEnabler : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private PlayerHealth playerHealth;

    [SerializeField] private Rigidbody[] ragdollRbs;

    public void InitializeOwner()
    {
        if (!IsOwner) return;

        PlayerHealth.OnPlayerTakeDamage += PlayerHealth_OnPlayerTakeDamage;
        ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        DisableRagdoll();
    }

    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EnableRagdoll();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            DisableRagdoll();
        }
    }


    private void PlayerHealth_OnPlayerTakeDamage(object sender, PlayerHealth.OnPlayerTakeDamageArgs e)
    {
        EnableRagdoll();
    }

    public void TriggerRagdoll(Vector3 force, Vector3 hitPoint)
    {
        Rigidbody hitRigidbody = ragdollRbs.OrderBy(rigidbody => Vector3.Distance(rigidbody.position, hitPoint)).First(); // we order all rbs by distance to hitpoint and take the first one
        hitRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);

        // set the state anim for ragdoll
    }


    private void EnableRagdoll()
    {
        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = false;
        }

        animator.enabled = false;

    }

    private void DisableRagdoll()
    {
        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = true;

        }

        animator.enabled = true;
    }

    public void UnInitializeOwner()
    {
        if (!IsOwner) return;

        PlayerHealth.OnPlayerTakeDamage -= PlayerHealth_OnPlayerTakeDamage;
        DisableRagdoll();
    }

}
