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

    private Transform[] allBonesTransform;
    [SerializeField] private Rigidbody[] ragdollRbs; //SERIALIZEFIELD ONLY FOR DEBUG
    [SerializeField] private Collider[] ragdollColliders; //SERIALIZEFIELD ONLY FOR DEBUG
    [SerializeField] private Rigidbody[] ragdollRbsToKnockback;
    private Vector3 defaultHipsRotation = new Vector3(-90f, 180f, 0f);
    private bool recievedKnockbackData = false;
    private KnockbackData lastKnockbackData;
    private BoneTransformData[] lastBoneTransformData;

    [BetterHeader("DEBUG")] [SerializeField]
    private bool debugRagdollEnabler;

    [SerializeField] private bool debugRagdollDisabler;


    public override void OnNetworkSpawn()
    {
        allBonesTransform = ragdollRoot.GetComponentsInChildren<Transform>();
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

            Debug.Log(
                $"KNOCKBACK - Knockback data received  - Index: {lastKnockbackData.hitRigidbodyIndex} - Hit Pos: {lastKnockbackData.hitPosition} - Hit Force: {lastKnockbackData.hitForce} - {gameObject.transform.parent.name}");

            foreach (BoneTransformData boneTransformData in lastBoneTransformData)
            {
                Debug.Log($"KNOCKBACK - BONE TRANSFORM DATA RECIEVED - POSITION: {boneTransformData.LocalPosition} - ROTATION: {boneTransformData.LocalRotation}");
            }
            
            EnableRagdoll();
            //Do knockback based on data
            DoKnockbackOnRagdoll(lastKnockbackData, lastBoneTransformData);

            //Reset Knockback data and BonesTransform Data
            UsedLastKnockbackData();

        }
        else
        {
            //Not owner of the hit Ragdoll. Means that this player threw the Item
            EnableRagdoll();
            //Calculate the right Knockback Data
            KnockbackData knockbackData = CalculateKnockbackData(knockbackStrength, hitPoint);

            BoneTransformData[] bonesTransformData = GetAllBonesTransformData();
            
            // RPC the Knockback Data and BonesTransform Data
            PassKnockbackDataToServerRpc(knockbackData, bonesTransformData);
            //Do Knockback based on data
            DoKnockbackOnRagdoll(knockbackData, bonesTransformData);
        }
    }

    private void UsedLastKnockbackData()
    {
        lastKnockbackData = default;
        Array.Clear(lastBoneTransformData, 0, lastBoneTransformData.Length); //clear it from the index 0 to the end
        
        recievedKnockbackData = false;
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void PassKnockbackDataToServerRpc(KnockbackData knockbackData, BoneTransformData[] bonesTrasfrom)
    {
        Debug.Log($"KNOCKBACK - Passing Knockback through Server");
        PassKnockbackDataToClientRpc(knockbackData, bonesTrasfrom);
    }

    [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
    private void PassKnockbackDataToClientRpc(KnockbackData knockbackData, BoneTransformData[] bonesTrasfrom)
    {
        Debug.Log($"KNOCKBACK - Knockback Recieved RPC");
        lastKnockbackData = knockbackData;
        lastBoneTransformData = new BoneTransformData[bonesTrasfrom.Length];
        Array.Copy(bonesTrasfrom, lastBoneTransformData, bonesTrasfrom.Length);
        
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
            $"KNOCKBACK - Knockback data calculated  - Index: {hitRigidbodyIndex} - Hit Pos: {hitPoint} - Hit Force: {force} - {gameObject.transform.parent.name}");

        return new KnockbackData
        {
            hitForce = force,
            hitPosition = hitPoint,
            hitRigidbodyIndex = hitRigidbodyIndex,
        };
    }

    private BoneTransformData[] GetAllBonesTransformData()
    {
        var boneDataList = new List<BoneTransformData>(allBonesTransform.Length);
        foreach (Transform bone in allBonesTransform)
        {
            boneDataList.Add(new BoneTransformData {
                LocalPosition = bone.localPosition,
                LocalRotation = bone.localRotation
            });
            Debug.Log($"KNOCKBACK - GETTING BONES POS: {bone.localPosition} - {bone.localRotation} - {bone.name}");
        }
        BoneTransformData[] boneArray = boneDataList.ToArray();

        return boneArray;
    }
    
    private void SetBoneLocalTransform(BoneTransformData[] bonesTransform)
    {
        for (int i = 0; i < bonesTransform.Length; i++)
        {
            Debug.Log(
                $"KNOCKBACK - SETTING BONE LOCAL POS - CURRENT POS: {allBonesTransform[i].localPosition} - RIGHT POS: {bonesTransform[i].LocalPosition} - CURRENT ROTATION: {allBonesTransform[i].localRotation} - RIGHT ROTATION: {bonesTransform[i].LocalRotation}");
            allBonesTransform[i].localPosition = bonesTransform[i].LocalPosition;
            allBonesTransform[i].localRotation = bonesTransform[i].LocalRotation;
        }
    }

    /// <summary>
    /// Called from the knockbackListener
    /// </summary>
    /// <param name="knockbackStrength"> Strength of the Knockback</param>
    /// <param name="hitPoint"> Position of the hit</param>
    public void TriggerRagdoll(float knockbackStrength, Vector3 hitPoint)
    {
        Debug.Log($"KNOCKBACK - TRIGGER RAGDOLL - {gameObject.transform.parent.name}");
        StartCoroutine(WaitKnockbackData(knockbackStrength, hitPoint));
    }
    
    /// <summary>
    /// Called to add force to the bone, the ragdoll must be enabled first. THIS DOES NOT ENABLE THE RAGDOLL!
    /// </summary>
    /// <param name="knockbackData"> Data do Knockback</param>
    /// <param name="boneTransformDatas"> Data to sync the bones pos</param>
    private void DoKnockbackOnRagdoll(KnockbackData knockbackData, BoneTransformData[] boneTransformDatas)
    {
        Rigidbody hitRigidbody = ragdollRbsToKnockback[knockbackData.hitRigidbodyIndex];
        knockbackData.hitForce = new Vector3(knockbackData.hitForce.x, knockbackData.hitForce.y, 0f); //Ensure to not knockback in Z

        SetBoneLocalTransform(boneTransformDatas);
        
        hitRigidbody.AddForceAtPosition(knockbackData.hitForce, knockbackData.hitPosition, ForceMode.Impulse);
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

/// <summary>
/// Data of the Bone Position and Rotation in Local Space
/// </summary>
public struct BoneTransformData : INetworkSerializable
{
    public Vector3 LocalPosition;
    public Quaternion LocalRotation;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref LocalPosition);
        serializer.SerializeValue(ref LocalRotation);
    }
}

/// <summary>
/// Data of the Knockback - Hit Rb - Hit Force - Hit Position
/// </summary>
public struct KnockbackData : INetworkSerializable, IEquatable<KnockbackData> 
{
    public int hitRigidbodyIndex;
    public Vector3 hitForce;
    public Vector3 hitPosition;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref hitRigidbodyIndex);
        serializer.SerializeValue(ref hitPosition);
        serializer.SerializeValue(ref hitForce);
    }

    public bool Equals(KnockbackData other)
    {
        return hitRigidbodyIndex == other.hitRigidbodyIndex && hitPosition == other.hitPosition && hitForce == other.hitForce;
    }
} 
