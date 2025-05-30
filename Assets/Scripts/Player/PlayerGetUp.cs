using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.Image;

public class PlayerGetUp : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rootTransform;
    [SerializeField] private Transform hipsTransform;

    [Header("Settings")]
    [SerializeField] private Collider[] playerColliders;
    [SerializeField] private LayerMask layersToDetectCollision;

    private NetworkVariable<bool> isFallen = new NetworkVariable<bool>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private const int MAX_ATTEMPTS = 500;
    private const float STEP_SIZE =  0.5f;

    private float verticalOffset;
    private float OriginalRootZ;
    private Quaternion originalRootRotation;
    private Quaternion originalHipsRotation;
    private Vector3 finalPosition;

    private Vector3[] directions =
    {
        Vector3.up,
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right,
        Vector3.forward + Vector3.left,
        Vector3.forward + Vector3.right,
        Vector3.back + Vector3.left,
        Vector3.back + Vector3.right
    };

    public void InitializeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction += HandleOnItemCallbackAction;
    }

    public void TriggerForCacheOriginalPos()
    {
        Debug.Log("STANDUP - Trigger for cache original pos");
        if (!IsOwner) return;
        RequestCacheOriginalPosServerRpc();
        Debug.Log("STANDUP - Trigger for cache original pos work");
    }

    [Rpc(SendTo.Server)]
    private void RequestCacheOriginalPosServerRpc()
    {
        CacheOriginalPosClientRpc();
        Debug.Log("STANDUP - Cache original pos Server rpc");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CacheOriginalPosClientRpc()
    {
        CacheOriginalPos();
        Debug.Log("STANDUP - Cache original pos client rpc");
    }

    private void CacheOriginalPos()
    {
        isFallen.Value = true;
        OriginalRootZ = rootTransform.position.z;
        verticalOffset = hipsTransform.position.y - rootTransform.position.y;
        originalHipsRotation = hipsTransform.rotation;
        originalRootRotation = Quaternion.Euler(0, rootTransform.eulerAngles.y, 0);
        Debug.Log("STANDUP - Cache original pos");
    }

    private void HandleOnItemCallbackAction()
    {
        if(!IsOwner || !isFallen.Value) return;

        CalculatePlayerFreePos();
        Debug.Log("STANDUP - Calculate player free pos");
    }

    private void CalculatePlayerFreePos()
    {
        Vector3 playerRagdollPosition = hipsTransform.position;
        playerRagdollPosition.y -= verticalOffset;
        playerRagdollPosition.z = OriginalRootZ;

        if (Physics.Raycast(hipsTransform.position, Vector3.down, out var hit, 5f, layersToDetectCollision))
            playerRagdollPosition.y = Mathf.Max(playerRagdollPosition.y, hit.point.y);

        Vector3 foundFinalPosition = GetFreePosition(playerRagdollPosition);

        finalPosition = foundFinalPosition;
        PassPlayerFreePosServerRpc(foundFinalPosition, originalRootRotation);
    }

    private Vector3 GetFreePosition(Vector3 startPos)
    {
        foreach (Vector3 direction in directions)
        {
            for (int i = 1; i <= MAX_ATTEMPTS; i++)
            {
                Vector3 testDirection = startPos + direction * (i * STEP_SIZE);
                testDirection.z = OriginalRootZ;

                if (AreAllCollidersFreeAt(testDirection))
                {
                    Debug.Log($"STANDUP - Return calculate correct pos: {testDirection}");
                    return testDirection;
                }
            }
        }
        Debug.Log($"STANDUP - Error on calculate pos: {startPos}");
        return startPos;
    }

    private bool AreAllCollidersFreeAt(Vector3 checkPos)
    {
        foreach (Collider colliders in playerColliders)
        {
            Vector3 localOffset = colliders.transform.position - rootTransform.position;
            Vector3 worldCenter = checkPos + localOffset;

            if (colliders is BoxCollider box)
            {
                Vector3 halfExtents = Vector3.Scale(box.size, colliders.transform.lossyScale) * 0.5f;
                if (Physics.CheckBox(worldCenter, halfExtents, originalRootRotation, layersToDetectCollision))
                {
                    Debug.Log("STANDUP - Checkbox error, is colliding");
                    return false;
                }
                    
            }

            Vector3 groundCheckOrigin = worldCenter + Vector3.up * 0.1f;

            if (!Physics.Raycast(groundCheckOrigin, Vector3.down, out _, 0.5f, layersToDetectCollision))
            {
                Debug.Log("STANDUP - ground error, is not colliding");
                return false;
            }
        }
        return true;
    }


    [Rpc(SendTo.Server)]
    private void PassPlayerFreePosServerRpc(Vector3 finalPos, Quaternion finalRotation)
    {
        PassPlayerFreePosClientRpc(finalPos, finalRotation);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PassPlayerFreePosClientRpc(Vector3 finalPos, Quaternion finalRotation)
    {
        ApplyGetUp(finalPos, finalRotation);
    }

    private void ApplyGetUp(Vector3 finalPos, Quaternion finalRotation)
    {
        isFallen.Value = false;
        rootTransform.SetPositionAndRotation(finalPos, finalRotation);
        hipsTransform.SetPositionAndRotation(finalPos + Vector3.up * verticalOffset, originalHipsRotation);
        Debug.Log("STANDUP - Pass player get up");
    }

    public void UnInitializeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction -= HandleOnItemCallbackAction;
    }
}
