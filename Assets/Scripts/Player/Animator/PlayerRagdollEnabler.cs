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



    [Header("Seetings")]
    [SerializeField] private float timeToWakeUp = 5f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public void InitializeOwner()
    {
        if (!IsOwner) return;

        PlayerHealth.OnPlayerTakeDamage += PlayerHealth_OnPlayerTakeDamage;
        ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();

        DisableRagdoll();
    }

    // Just for debug on scene Ragdoll
    private void Awake()
    {
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
        EnableRagdoll();

        Rigidbody hitRigidbody = ragdollRbs.OrderBy(Rigidbody => Vector3.Distance(Rigidbody.position, hitPoint)).First(); // we order all rbs by distance to hitpoint and take the first one
        hitRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);

        // set the state anim for ragdoll
    }

    private void RagdollBehaviour()
    {
        timeToWakeUp -= Time.deltaTime;

        if(timeToWakeUp <= 0)
        {
            DisableRagdoll();
        }
    }

    private void AlignPositionToHips()
    {
        Vector3 newPosition = hipsTransform.position; // create a new pos basead on actual hips transform position
        rootTransform.position = newPosition;
        rootTransform.rotation = Quaternion.LookRotation(ragdollRoot.forward, Vector3.up);

        // Send gfx for original pos 
        ragdollRoot.localPosition = Vector3.zero;
        ragdollRoot.localRotation = Quaternion.identity;

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
        AlignPositionToHips();

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
