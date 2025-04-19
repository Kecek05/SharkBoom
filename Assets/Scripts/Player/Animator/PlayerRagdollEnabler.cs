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
    private float verticalOffset = 0f;

    public void InitializeOwner()
    {
        if (!IsOwner) return;

        PlayerHealth.OnPlayerTakeDamage += PlayerHealth_OnPlayerTakeDamage;
        ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();

        DisableRagdoll(false);
    }

    // Just for debug on scene Ragdoll
    private void Awake()
    {
        ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        DisableRagdoll(false);
    }
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EnableRagdoll();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            DisableRagdoll(true);
        }
    }

    private void PlayerHealth_OnPlayerTakeDamage(object sender, PlayerHealth.OnPlayerTakeDamageArgs e)
    {
        EnableRagdoll(); // when take damage enable ragdoll (check if this have delay)
    }

    public void TriggerRagdoll(Vector3 force, Vector3 hitPoint)
    {
        EnableRagdoll();

        Rigidbody hitRigidbody = ragdollRbs.OrderBy(Rigidbody => Vector3.Distance(Rigidbody.position, hitPoint)).First(); // we order all rbs by distance to hitpoint and take the first one
        hitRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);

        // set the state anim for ragdoll (after)
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


    private void EnableRagdoll()
    {
        verticalOffset = hipsTransform.position.y - rootTransform.position.y;

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = false;
        }

        animator.enabled = false;

    }

    private void DisableRagdoll(bool alignToHips)
    {
        if (alignToHips)
        {
            AlignPositionToHips();
        }

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
        DisableRagdoll(false);
    }

}
