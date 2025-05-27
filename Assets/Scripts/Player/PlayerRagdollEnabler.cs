using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine; 

public class PlayerRagdollEnabler : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private Collider[] playerColliders;

    [SerializeField] private Rigidbody[] ragdollRbs;
    [SerializeField] private Rigidbody parentRigidbody;

    private Collider[] ragdollColliders;

    [SerializeField] private bool debugRagdollEnabler;
    [SerializeField] private bool debugRagdollDisabler;

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

    //private void Update()
    //{
    //    if (debugRagdollDisabler)
    //    {
    //        debugRagdollDisabler = false;
    //        DisableRagdoll();
    //    }

    //    if (debugRagdollEnabler)
    //    {
    //        debugRagdollEnabler = false;
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

        TriggerRagdollServerRpc(closestIndex, force, hitPoint);
    }

    [Rpc(SendTo.Server)]
    private void TriggerRagdollServerRpc(int hitRigidbodyIndex, Vector3 force, Vector3 hitPoint)
    {
        TriggerRagdollClientRpc(hitRigidbodyIndex, force, hitPoint);
        Debug.Log("RAGDOLL - Call trigger on server");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerRagdollClientRpc(int hitRigidbodyIndex, Vector3 force, Vector3 hitPoint)
    {
        EnableRagdoll();
        Rigidbody hitRigidbody = ragdollRbs[hitRigidbodyIndex]; // get the rb we hit
        hitRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
    }

    private void EnableRagdoll()
    {
        Debug.Log("RAGDOLL ENABLE - Enable ragdoll");

        foreach (Collider ragdollCollider in ragdollColliders)
        {
            ragdollCollider.enabled = true;
            Debug.Log($"RAGDOLL ENABLE - {ragdollCollider.name} + {ragdollCollider.enabled}");
        }

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = false;
            Debug.Log($"RAGDOLL ENABLE - {ragdollRb.name} + isKinematic: {ragdollRb.isKinematic}");
        }

        foreach (Collider playerCollider in playerColliders)
        {
            playerCollider.enabled = false;
            Debug.Log($"RAGDOLL ENABLE - {playerCollider.name} + {playerCollider.enabled}");
        }

        parentRigidbody.isKinematic = true;
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
        Debug.Log("RAGDOLL DISABLE - Disable ragdoll");
        animator.enabled = true;

        foreach (Collider ragdollCollider in ragdollColliders)
        {
            ragdollCollider.enabled = false;
            Debug.Log($"RAGDOLL DISABLE -{ragdollCollider.name} + {ragdollCollider.enabled}");
        }

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = true;
            Debug.Log($"RAGDOLL DISABLE - {ragdollRb.name} + isKinematic: {ragdollRb.isKinematic}");
        }

        foreach (Collider playerCollider in playerColliders)
        {
            playerCollider.enabled = true;
            Debug.Log($"RAGDOLL DISABLE - {playerCollider.name} + {playerCollider.enabled}");
        }

        parentRigidbody.isKinematic = false;
    }

    public void UnInitializeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction += HandleOnItemCallbackAction;
    }
}
