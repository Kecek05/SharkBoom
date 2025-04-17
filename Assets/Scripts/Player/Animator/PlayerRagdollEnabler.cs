using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerRagdollEnabler : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private PlayerHealth playerHealth;

    public Rigidbody[] ragdollRbs;


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

        PlayerHealth.OnPlayerTakeDamage -= PlayerHealth_OnPlayerTakeDamage;
        DisableRagdoll();
    }

}
