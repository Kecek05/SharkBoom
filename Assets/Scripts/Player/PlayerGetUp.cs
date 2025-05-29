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

    private const int MAX_ATTEMPTS = 500;
    private const float STEP_SIZE =  0.5f;
    private bool isFallen = false;

    private float verticalOffset;
    private float OriginalRootZ;
    private Quaternion originalRootRotation;
    private Quaternion originalHipsRotation;

    private Vector3 finalPosition;

    private Vector3[] directions =
    {
        Vector2.up, 
        Vector2.down, 
        Vector2.left, 
        Vector2.right,
        (Vector2.up + Vector2.left).normalized, 
        (Vector2.up + Vector2.right).normalized,
        (Vector2.down + Vector2.left).normalized, 
        (Vector2.down + Vector2.right).normalized
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
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CacheOriginalPosClientRpc()
    {
        CacheOriginalPos();
    }

    private void CacheOriginalPos()
    {
        isFallen = true;
        OriginalRootZ = rootTransform.position.z;
        verticalOffset = hipsTransform.position.y - rootTransform.position.y;
        originalRootRotation = rootTransform.rotation;
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
        if (!IsOwner) return;
        if (!isFallen) return;
        Debug.Log($"STANDUP - {IsOwner} + {isFallen}");
        CalculatePlayerFreePos();
        Debug.Log("STANDUP - Request get calculate player free pos");
    }

    private void CalculatePlayerFreePos()
    {
        Debug.Log("STANDUP - Calculate player free pos");
        Vector3 origin = hipsTransform.position;
        origin.y -= verticalOffset;
        origin.z = OriginalRootZ;

        if (Physics.Raycast(hipsTransform.position, Vector3.down, out var hit, 5f, layersToDetectCollision))
            origin.y = Mathf.Max(origin.y, hit.point.y);

        Vector3 found = GetFreePosition(origin);

        finalPosition = found;
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
            Vector3 worldCenter = checkPos + (colliders.bounds.center - rootTransform.position);

            if (colliders is BoxCollider box)
            {
                Vector3 halfExtents = Vector3.Scale(box.size, colliders.transform.lossyScale) * 0.5f;
                if (Physics.CheckBox(worldCenter, halfExtents, colliders.transform.rotation, layersToDetectCollision))
                    return false;
            }

            Vector3 groundCheckOrigin = worldCenter + Vector3.up * 0.1f;
            if (!Physics.Raycast(groundCheckOrigin, Vector3.down, out _, 0.2f + 0.1f, layersToDetectCollision))
                return false;
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
        rootTransform.SetPositionAndRotation(finalPosition, originalRootRotation);
        hipsTransform.SetPositionAndRotation(finalPosition + Vector3.up * verticalOffset, originalHipsRotation);
        isFallen = false;
        Debug.Log("STANDUP - Set player free pos");
    }

    public void UnInitializeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction -= HandleOnItemCallbackAction;
    }
}
