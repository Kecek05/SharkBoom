using Sortify;
using System;
using System.Collections;
using System.Collections.Generic;
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
    private bool recievedKnockbackData = false;
    private KnockbackData lastKnockbackData;
    
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
        DisableRagdoll();
    }

    private IEnumerator WaitKnockbackData(float knockbackStrength, Vector3 hitPoint)
    {
        if (IsOwner)
        {
            //Owner of the hit Ragdoll. Means that the Enemy threw the Item
            while (!recievedKnockbackData)
            {
                //Wait to recieve the KnockbackData from the other player that throwed the item. 
                //This shouldnt cause any Jitter because there is an delay between throwing an Item and recieving the data.
                Debug.Log("KNOCKBACK - Waiting for knockback data");
                yield return null;
            }
            Debug.Log($"KNOCKBACK - Knockback data received  - Index: {lastKnockbackData.hitRigidbodyIndex} - Rb Pos: {lastKnockbackData.hitRigidbodyPosition} - Rb Rot: {lastKnockbackData.hitRigidbodyRotation} - Hit Pos: {lastKnockbackData.hitPosition} - Hit Force: {lastKnockbackData.hitForce} - {gameObject.transform.parent.name}");
            //Do knockback based on data
            TriggerRagdoll(lastKnockbackData);
            
            //Reset Knockback data
            UsedLastKnockbackData();

        }
        else
        {
            //Not owner of the hit Ragdoll. Means that this player threw the Item
            
            //Calculate the right Knockback Data
            KnockbackData knockbackData = CalculateKnockbackData(knockbackStrength, hitPoint);
            Debug.Log($"KNOCKBACK - Knockback data calculated  - Index: {knockbackData.hitRigidbodyIndex} - Rb Pos: {knockbackData.hitRigidbodyPosition} - Rb Rot: {knockbackData.hitRigidbodyRotation} - Hit Pos: {knockbackData.hitPosition} - Hit Force: {knockbackData.hitForce} - {gameObject.transform.parent.name}");
            // RPC the Knockback Data
            PassKnockbackDataToServerRpc(knockbackData);
            //Do Knockback based on data
            TriggerRagdoll(knockbackData);
        }
    }

    private void UsedLastKnockbackData()
    {
        recievedKnockbackData = false;
        lastKnockbackData = default;
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void PassKnockbackDataToServerRpc(KnockbackData knockbackData)
    {
        PassKnockbackDataToClientRpc(knockbackData);
    }

    [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
    private void PassKnockbackDataToClientRpc(KnockbackData knockbackData)
    {
        recievedKnockbackData = true;
        lastKnockbackData = knockbackData;
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

        return new KnockbackData
        {
            hitForce = force,
            hitPosition = hitPoint,
            hitRigidbodyIndex = hitRigidbodyIndex,
            hitRigidbodyPosition = hitRigidbody.position,
            hitRigidbodyRotation = hitRigidbody.rotation,
        };
    }
    
    public void TriggerRagdoll(float knockbackStrength, Vector3 hitPoint)
    {
        Debug.Log($"KNOCKBACK - TRIGGER RAGDOLL - {gameObject.transform.parent.name}");
        StartCoroutine(WaitKnockbackData(knockbackStrength, hitPoint));
    }
    private void TriggerRagdoll(KnockbackData knockbackData)
    {
        EnableRagdoll();
        Rigidbody hitRigidbody = ragdollRbsToKnockback[knockbackData.hitRigidbodyIndex];
        hitedRbDebug = hitRigidbody;

        knockbackData.hitForce = new Vector3(knockbackData.hitForce.x, knockbackData.hitForce.y, 0f); //Ensure to not knockback in Z
        hitRigidbody.position = knockbackData.hitRigidbodyPosition;
        hitRigidbody.rotation = knockbackData.hitRigidbodyRotation;
        //Debug.Log($"RAGDOLL - Before Force - Velocity: {hitRigidbody.linearVelocity}, Position: {hitRigidbody.position}, Rotation: {hitRigidbody.rotation}, Scale: {hitRigidbody.transform.localScale}");
        hitRigidbody.AddForceAtPosition(knockbackData.hitForce, knockbackData.hitPosition, ForceMode.Impulse);
        //Debug.Log($"RAGDOLL - Ragdoll enabled, hit rb: {hitRigidbody.name}, force: {force}, hitPoint: {hitPoint} - Rb Velocity: {hitRigidbody.linearVelocity}");
    }

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

        Debug.Log($"RAGDOLL - Animator Enable: {animator.enabled} (false), {gameObject.transform.parent.name}");
    }


    //[Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    //private void RequestRagdollDisableServerRpc()
    //{
    //    DisableRagdollClientRpc();
    //}

    //[Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    //private void DisableRagdollClientRpc()
    //{
    //    DisableRagdoll();
    //}

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
        Debug.Log($"RAGDOLL - Animator Enable: {animator.enabled} (true), {gameObject.transform.parent.name}");
        OnRagdollDisabled?.Invoke();
    }
}

public struct KnockbackData : INetworkSerializable, IEquatable<KnockbackData> 
{
    public int hitRigidbodyIndex;
    public Vector3 hitForce;
    public Vector3 hitPosition;
    public Vector3 hitRigidbodyPosition;
    public Quaternion hitRigidbodyRotation;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref hitRigidbodyIndex);
        serializer.SerializeValue(ref hitPosition);
        serializer.SerializeValue(ref hitRigidbodyPosition);
        serializer.SerializeValue(ref hitRigidbodyRotation);
        serializer.SerializeValue(ref hitForce);
    }

    public bool Equals(KnockbackData other)
    {
        return hitRigidbodyIndex == other.hitRigidbodyIndex && hitPosition == other.hitPosition &&
               hitRigidbodyPosition == other.hitRigidbodyPosition && hitRigidbodyRotation == other.hitRigidbodyRotation && hitForce == other.hitForce;
    }
} 
