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

    private NetworkVariable<bool> isFallen = new NetworkVariable<bool>();

    private const int MAX_ATTEMPTS = 500;
    private const float STEP_SIZE =  0.5f;
    

    private float verticalOffset;
    private float OriginalRootZ;
    private Quaternion originalRootRotation;
    private Quaternion originalHipsRotation;

    private Vector3 finalPosition;

    private Vector3[] directions =
    {
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
        if (!IsOwner) return;
        RequestCacheOriginalPosServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void RequestCacheOriginalPosServerRpc()
    {
        CacheOriginalPosClientRpc();
        Debug.Log("STANDUP - Call request cache original pos on server rpc");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CacheOriginalPosClientRpc()
    {
        CacheOriginalPos();
        Debug.Log("STANDUP - Call request cache original pos on client rpc");
    }

    private void CacheOriginalPos()
    {
        isFallen.Value = true;
        OriginalRootZ = rootTransform.position.z;
        verticalOffset = hipsTransform.position.y - rootTransform.position.y;
        originalHipsRotation = hipsTransform.rotation;
        Debug.Log("STANDUP - Cache original pos");
    }

    private void HandleOnItemCallbackAction()
    {
        if (!IsOwner) return;
        RequestGetUpPlayerServerRpc();
        Debug.Log("STANDUP - Handle item callback");
    }

    [Rpc(SendTo.Server)]
    private void RequestGetUpPlayerServerRpc()
    {
        RequestGetUpPlayerClientRpc();
        Debug.Log("STANDUP - Request get up player on server");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RequestGetUpPlayerClientRpc()
    {
        Debug.Log("STANDUP - Request get up player on Client");
        Debug.Log($"STANDUP - {IsOwner} + {isFallen.Value}");

        if (!IsOwner) return;
        if (!isFallen.Value) return;
        
        CalculatePlayerFreePos();
        Debug.Log("STANDUP - Request get calculate player free pos");
    }

    private void CalculatePlayerFreePos()
    {
        Debug.Log("STANDUP - Calculate player free pos");
        Vector3 playerRagdollPosition = hipsTransform.position;
        playerRagdollPosition.y -= verticalOffset;
        playerRagdollPosition.z = OriginalRootZ;

        if (Physics.Raycast(hipsTransform.position, Vector3.down, out var hit, 5f, layersToDetectCollision))
            playerRagdollPosition.y = Mathf.Max(playerRagdollPosition.y, hit.point.y);

        Vector3 foundFinalPosition = GetFreePosition(playerRagdollPosition);

        finalPosition = foundFinalPosition;
        PassPlayerFreePosServerRpc();

    }

    private Vector3 GetFreePosition(Vector3 startPos)
    {
        foreach (Vector3 direction in directions)
        {
            for (int i = 1; i <= MAX_ATTEMPTS; i++)
            {
                Vector3 testDirection = startPos + direction * (i * STEP_SIZE);
                testDirection.z = OriginalRootZ;
                Debug.Log($"STANDUP - Direction: {direction} + Test Direction: {testDirection}");
                if (AreAllCollidersFreeAt(testDirection))
                    return testDirection;
            }
        }
        return startPos;
    }

    private bool AreAllCollidersFreeAt(Vector3 checkPos)
    {
        Debug.Log("STANDUP - Are all colliders free at");
        foreach (Collider colliders in playerColliders)
        {
            Vector3 localOffset = colliders.transform.position - rootTransform.position;
            Vector3 worldCenter = checkPos + localOffset;

            originalRootRotation = Quaternion.Euler(0, rootTransform.eulerAngles.y, 0);

            if (colliders is BoxCollider box)
            {
                Vector3 halfExtents = Vector3.Scale(box.size, colliders.transform.lossyScale) * 0.5f;
                if (Physics.CheckBox(worldCenter, halfExtents, originalRootRotation, layersToDetectCollision))
                    return false;
            }

            Vector3 groundCheckOrigin = worldCenter + Vector3.up * 0.1f;
            if (!Physics.Raycast(groundCheckOrigin, Vector3.down, out _, 0.5f, layersToDetectCollision))
            {
                Debug.Log("STANDUP - Ground check failed.");
                return false;
            }
        }

        return true;
    }

    [Rpc(SendTo.Server)]
    private void PassPlayerFreePosServerRpc()
    {
        PassPlayerFreePosClientRpc();
        Debug.Log("STANDUP - Player free pos for server");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PassPlayerFreePosClientRpc()
    {
        PassPlayerFreePos();
        Debug.Log("STANDUP - Player free pos for Client");
    }

    private void PassPlayerFreePos()
    {
        hipsTransform.SetPositionAndRotation(finalPosition + Vector3.up * verticalOffset, originalHipsRotation);
        rootTransform.SetPositionAndRotation(finalPosition, originalRootRotation);
        isFallen.Value = false;
        Debug.Log("STANDUP - Set player free pos");
    }

    public void UnInitializeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction -= HandleOnItemCallbackAction;
    }
}
