using Sortify;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerRagdollEnabler : NetworkBehaviour
{
    public event Action OnRagdollDisabled;

    [BetterHeader("References")] [SerializeField]
    private Animator animator;

    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private Collider[] playerColliders;
    [SerializeField] private Rigidbody parentRigidbody;
    [SerializeField] private Transform hips;


    [SerializeField] private Rigidbody[] ragdollRbs; //SERIALIZEFIELD ONLY FOR DEBUG
    [SerializeField] private Collider[] ragdollColliders; //SERIALIZEFIELD ONLY FOR DEBUG
    [SerializeField] private Rigidbody[] ragdollRbsToKnockback;
    private Vector3 defaultHipsRotation = new Vector3(-90f, 180f, 0f);
    
    private bool recievedKnockbackData = false;
    private KnockbackData lastKnockbackData;
    private bool alreadyKnockbacked = false;

    [BetterHeader("DEBUG")]
    [SerializeField]private bool debugRagdollEnabler;
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
        DisableRagdoll();
    }

    private IEnumerator WaitKnockbackData(float knockbackStrength, Vector3 hitPoint)
    {
        Debug.Log($"KNOCKBACK - WaitKnockbackData - Owner: {IsOwner}");
        if (IsOwner)
        {
            // RpcDebugServerRpc();
            //Owner of the hit Ragdoll. Means that the Enemy threw the Item
            while (!recievedKnockbackData)
            {
                //Wait to recieve the KnockbackData from the other player that throwed the item. 
                //This shouldnt cause any Jitter because there is an delay between throwing an Item and recieving the data.
                Debug.Log("KNOCKBACK - Waiting for knockback data");
                yield return null;
            }

            Debug.Log(
                $"KNOCKBACK - Knockback data received  - Index: {lastKnockbackData.hitRigidbodyIndex} - Hit Force: {lastKnockbackData.hitForce} - Rb Pos: {lastKnockbackData.hitRagdollPosition} - Rb Rot: {lastKnockbackData.hitRagdollRotation} - {gameObject.transform.parent.name}");
            
            EnableRagdoll();
            //Do knockback based on data
            DoKnockbackOnRagdoll(lastKnockbackData);

            //Reset Knockback data and BonesTransform Data
            UsedLastKnockbackData();

        }
        else
        {
            //Not owner of the hit Ragdoll. Means that this player threw the Item
            EnableRagdoll();
            //Calculate the right Knockback Data
            KnockbackData knockbackData = CalculateKnockbackData(knockbackStrength, hitPoint);
            
            // RPC the Knockback Data and BonesTransform Data
            // RpcDebugServerRpc();
            PassKnockbackDataToServerRpc(knockbackData);
            //Do Knockback based on data
            DoKnockbackOnRagdoll(knockbackData);
        }
    }

    private void UsedLastKnockbackData()
    {
        lastKnockbackData = default;
        recievedKnockbackData = false;
    }
    
    // [Rpc(SendTo.Server, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    // private void RpcDebugServerRpc()
    // {
    //     Debug.Log($"KNOCKBACK - RpcDebugServerRpc - {gameObject.transform.parent.name} - Owner: {IsOwner}");
    //     RpcDebugClientRpc();
    // }
    //
    // [Rpc(SendTo.Owner, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    // private void RpcDebugClientRpc()
    // {
    //     Debug.Log($"KNOCKBACK - RpcDebugClientRpc - {gameObject.transform.parent.name} - Owner: {IsOwner}");
    // }

    [Rpc(SendTo.Server, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void PassKnockbackDataToServerRpc(KnockbackData knockbackData)
    {
        Debug.Log($"KNOCKBACK - Passing Knockback through Server");
        PassKnockbackDataToClientRpc(knockbackData);
    }

    [Rpc(SendTo.Owner, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void PassKnockbackDataToClientRpc(KnockbackData knockbackData)
    {
        Debug.Log($"KNOCKBACK - Knockback Recieved RPC - Owner: {IsOwner} - {gameObject.transform.parent.name}");
        lastKnockbackData = knockbackData;
        recievedKnockbackData = true;
    }

    private KnockbackData CalculateKnockbackData(float knockbackStrength, Vector3 hitPoint)
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
                //Found a closer ragdoll
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
            return default;
        }
        
        Debug.Log(
            $"KNOCKBACK - Knockback data calculated  - Index: {hitRigidbodyIndex} - Hit Pos: {hitPoint} - Rb Pos: {hitRigidbody.position} - Rb Rot: {hitRigidbody.rotation} - Hit Force: {force} - {gameObject.transform.parent.name}");

        return new KnockbackData
        {
            hitForce = force,
            hitRigidbodyIndex = hitRigidbodyIndex,
            hitRagdollPosition = hitRigidbody.position,
            hitRagdollRotation = hitRigidbody.rotation,
        };
    }

    /// <summary>
    /// Called from the knockbackListener
    /// </summary>
    /// <param name="knockbackStrength"> Strength of the Knockback</param>
    /// <param name="hitPoint"> Position of the hit</param>
    public void TriggerKnockback(float knockbackStrength, Vector3 hitPoint)
    {
        Debug.Log($"KNOCKBACK - TRIGGER RAGDOLL - {gameObject.transform.parent.name} - Already Knocked: {alreadyKnockbacked}");
        
        if(alreadyKnockbacked) return;
        
        alreadyKnockbacked = true;
        
        StartCoroutine(WaitKnockbackData(knockbackStrength, hitPoint));
    }
    
    /// <summary>
    /// Called to add force to the bone, the ragdoll must be enabled first. THIS DOES NOT ENABLE THE RAGDOLL!
    /// </summary>
    /// <param name="knockbackData"> Data do Knockback</param>
    /// <param name="boneTransformDatas"> Data to sync the bones pos</param>
    private void DoKnockbackOnRagdoll(KnockbackData knockbackData)
    {
        Rigidbody hitRigidbody = ragdollRbsToKnockback[knockbackData.hitRigidbodyIndex];
        knockbackData.hitForce = new Vector3(knockbackData.hitForce.x, knockbackData.hitForce.y, 0f); //Ensure to not knockback in Z

        //Set Ragdoll position
        hitRigidbody.position = knockbackData.hitRagdollPosition;
        hitRigidbody.rotation = knockbackData.hitRagdollRotation;
        Debug.Log($"KNOCKBACK - Do Knockback - Index: {knockbackData.hitRigidbodyIndex} - Rb Pos: {hitRigidbody.position} - Rb Rot: {hitRigidbody.rotation} - Hit Force: {knockbackData.hitForce} - {gameObject.transform.parent.name}");
        
        hitRigidbody.AddForce(knockbackData.hitForce, ForceMode.Impulse);
        
        // SetBoneLocalTransform(boneTransformDatas);
        // hitRigidbody.AddForceAtPosition(knockbackData.hitForce, knockbackData.hitPosition, ForceMode.Impulse);
        
    }

    /// <summary>
    /// Called to enable ragdoll. Need to be called before getting the KnockbackData and Bone Pos
    /// </summary>
    private void EnableRagdoll()
    {
        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = false;
        }

        foreach (Collider ragdollCollider in ragdollColliders)
        {
            ragdollCollider.enabled = true;
        }

        foreach (Collider playerCollider in playerColliders)
        {
            playerCollider.enabled = false;
        }

        animator.enabled = false;
        parentRigidbody.isKinematic = true;

        // Debug.Log($"RAGDOLL - Animator Enable: {animator.enabled} (false), {gameObject.transform.parent.name}");
    }

    private void DisableRagdoll()
    {

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.isKinematic = true;
        }

        foreach (Collider ragdollCollider in ragdollColliders)
        {
            ragdollCollider.enabled = false;
        }

        foreach (Collider playerCollider in playerColliders)
        {
            playerCollider.enabled = true;
        }

        animator.enabled = true;
        animator.enabled = false;
        animator.enabled = true;

        parentRigidbody.isKinematic = false;

        hips.localRotation = Quaternion.Euler(defaultHipsRotation);
        hips.localPosition = Vector3.zero;
        // Debug.Log($"RAGDOLL - Animator Enable: {animator.enabled} (true), {gameObject.transform.parent.name}");
        OnRagdollDisabled?.Invoke();

        alreadyKnockbacked = false;
    }
}

/// <summary>
/// Data of the Knockback - Hit Rb - Hit Force - Hit Position
/// </summary>
public struct KnockbackData : INetworkSerializable
{
    public int hitRigidbodyIndex;
    public Vector3 hitForce;
    public Vector3 hitRagdollPosition;
    public Quaternion hitRagdollRotation;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref hitRigidbodyIndex);
        serializer.SerializeValue(ref hitForce);
        serializer.SerializeValue(ref hitRagdollPosition);
        serializer.SerializeValue(ref hitRagdollRotation);
    }
} 
