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
    
    [SerializeField] private Rigidbody[] ragdollRbs; //SERIALIZEFIELD ONLY FOR DEBUG
    [SerializeField] private Collider[] ragdollColliders;//SERIALIZEFIELD ONLY FOR DEBUG
    [SerializeField] private Rigidbody[] ragdollRbsToKnockback;
    private Vector3 defaultHipsRotation = new Vector3(-90f, 180f, 0f);

    [BetterHeader("DEBUG")]
    [SerializeField] private bool debugRagdollEnabler;
    [SerializeField] private bool debugRagdollDisabler;

    //Debug
    [HideInInspector] public Rigidbody hitedRbDebug;
    
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
        int hitRigidbodyIndex = -1;
        Vector3 force = Vector3.zero;
        Vector3 direction = Vector3.zero;

        foreach (Rigidbody ragdollRb in ragdollRbsToKnockback)
        {
            float currentDistance = Vector3.Distance(ragdollRb.position, hitPoint);
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;

                hitRigidbody = ragdollRb;
                hitRigidbodyIndex = index;

                direction = (hitRigidbody.position - hitPoint).normalized;
                force = direction * knockbackStrength;
            }
            index++;
        }

        if (hitRigidbodyIndex == -1)
        {
            Debug.LogError("No ragdoll rb found");
            return;
        }
        Debug.Log($"Ragdoll - Trigger");
        
        TriggerRagdoll(hitRigidbodyIndex, force, hitPoint, hitRigidbody.position, hitRigidbody.rotation);
    }

    // [Rpc(SendTo.Server)]
    // private void TriggerRagdollServerRpc(int hitRigidbodyIndex, Vector3 force, Vector3 hitPoint, Vector3 hitRigidbodyPosition, Quaternion hitRigidbodyRotation)
    // {
    //     TriggerRagdollClientRpc(hitRigidbodyIndex, force, hitPoint, hitRigidbodyPosition, hitRigidbodyRotation);
    //
    // }

    //[Rpc(SendTo.ClientsAndHost)]
    private void TriggerRagdoll(int hitRigidbodyIndex, Vector3 force, Vector3 hitPoint, Vector3 hitRigidbodyPosition, Quaternion hitRigidbodyRotation)
    {
        EnableRagdoll();
        Debug.Log($"Ragdoll - ParentRb Kinematic: {parentRigidbody.isKinematic}, Animator Enabled: {animator.enabled}");
        Rigidbody hitRigidbody = ragdollRbsToKnockback[hitRigidbodyIndex]; // get the rb we hit
        hitedRbDebug = hitRigidbody;
        
        force = new Vector3(force.x, force.y, 0f); //Ensure to not knockback in Z
        hitRigidbody.position = hitRigidbodyPosition;
        hitRigidbody.rotation = hitRigidbodyRotation;
        Debug.Log($"Ragdoll - Before Force - Velocity: {hitRigidbody.linearVelocity}, Position: {hitRigidbody.position}, Rotation: {hitRigidbody.rotation}, Scale: {hitRigidbody.transform.localScale}");
        hitRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
        Debug.Log($"Ragdoll enabled, hit rb: {hitRigidbody.name}, force: {force}, hitPoint: {hitPoint} - Rb Velocity: {hitRigidbody.linearVelocity}");
    }

    private void EnableRagdoll()
    {
        animator.enabled = false;
        
        foreach (Collider ragdollCollider in ragdollColliders)
        {
            ragdollCollider.enabled = true;
        }

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = false;
            Debug.Log("Enabled Ragdoll");
        }

        foreach (Collider playerCollider in playerColliders)
        {
            playerCollider.enabled = false;
        }

        parentRigidbody.isKinematic = true;
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
        
        animator.enabled = true;
        
        hips.localRotation = Quaternion.Euler(defaultHipsRotation);
        hips.localPosition = Vector3.zero;
        
        OnRagdollDisabled?.Invoke();
    }
}
