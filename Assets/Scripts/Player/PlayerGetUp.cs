using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerGetUp : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rootTransform;
    [SerializeField] private Transform hipsTransform;

    [Header("Settings")]
    [SerializeField] private Vector3 boxSize;
    [SerializeField] private LayerMask layersToDetectCollision;

    private const int MAX_ATTEMPTS = 100;
    private const float STEP_SIZE =  0.25f;
    private bool isFallen = false;

    private float verticalOffset;
    private float OriginalRootZ;

    private Vector3 finalPosition;

    private Vector2[] directions =
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right,
        (Vector2.up + Vector2.right).normalized,
        (Vector2.up + Vector2.left).normalized,
        (Vector2.down + Vector2.right).normalized,
        (Vector2.down + Vector2.left).normalized,
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
        if (!IsOwner) return;
        CacheOriginalPos();
    }

    private void CacheOriginalPos()
    {
        isFallen = true;
        OriginalRootZ = rootTransform.position.z;
        //verticalOffset = hipsTransform.position.y - rootTransform.position.y;
    }

    private void HandleOnItemCallbackAction()
    {
        if (!IsOwner) return;
        RequestGetUpPlayerServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void RequestGetUpPlayerServerRpc()
    {
        RequestGetUpPlayerClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RequestGetUpPlayerClientRpc()
    {
        if (!IsOwner) return;

        if (!isFallen) return;
        CalculatePlayerFreePos();
    }

    private void CalculatePlayerFreePos()
    {
        Vector3 initialPosOfPlayer = hipsTransform.position;
        initialPosOfPlayer.y -= verticalOffset;
        initialPosOfPlayer.z = OriginalRootZ;

        if (Physics.Raycast(hipsTransform.position, Vector3.down, out RaycastHit hit, 5f, layersToDetectCollision)) // check if we hit the ground for not reset in the ground
        {
            initialPosOfPlayer.y = Mathf.Max(initialPosOfPlayer.y, hit.point.y);
        }

        Vector3 foundPos = GetFreePosition(initialPosOfPlayer);

        if (!IsBoxFreeAt(foundPos))
        {
            Debug.LogWarning("No free position found to get up player");
            return;
        }

        finalPosition = foundPos;

        PassPlayerFreePoosServerRpc();

    }

    [Rpc(SendTo.Server)]
    private void PassPlayerFreePoosServerRpc()
    {
        PassPlayerFreePoosClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PassPlayerFreePoosClientRpc()
    {
        PassPlayerFreePoos();
    }

    private void PassPlayerFreePoos()
    {
        rootTransform.SetPositionAndRotation(finalPosition, Quaternion.identity);
        isFallen = false;
    }

    private Vector3 GetFreePosition(Vector3 startPos)
    {
        foreach (Vector2 direction in directions)
        {
            Vector3 finalDirection = new Vector3(direction.x, 0, direction.y);

            for (int i = 0; i < MAX_ATTEMPTS; i++)
            {
                Vector3 testPos = startPos + finalDirection * (i * STEP_SIZE);
                testPos.z = OriginalRootZ; 

                if (IsBoxFreeAt(testPos))
                {
                    return testPos;
                }
            }
        }
        Debug.LogWarning("No free position found to get up player");
        return startPos;
    }

    private bool IsBoxFreeAt(Vector3 checkPos)
    {
        // we use Vector3.up because we are testing the box in get up
        Vector3 centerOfBox = checkPos + Vector3.up * (boxSize.y * 0.5f);
        Vector3 attempSizeExtends = boxSize * 0.5f;

        bool isFree = !Physics.CheckBox(centerOfBox, attempSizeExtends, Quaternion.identity, layersToDetectCollision);
        bool isOnGround = Physics.Raycast(checkPos + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, attempSizeExtends.y + 0.1f, layersToDetectCollision);

        return isFree && isOnGround;
    }

    public void UnInitializeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction -= HandleOnItemCallbackAction;
    }
}
