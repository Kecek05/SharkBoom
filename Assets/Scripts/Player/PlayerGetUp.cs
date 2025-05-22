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
    [SerializeField] private float capsuleRadius = 0.5f;
    [SerializeField] private float capsuleHeight = 2f;
    [SerializeField] private LayerMask layersToDetectCollision;

    private const int MAX_ATTEMPTS = 10;
    private const int STEP_SIZE = 1;
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

    private List<Vector2> debugTestPositions = new List<Vector2>();

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
        verticalOffset = hipsTransform.position.y - rootTransform.position.y;
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
        debugTestPositions.Clear();
        Vector3 initialPosOfPlayer = hipsTransform.position;
        initialPosOfPlayer.y -= verticalOffset;  // correcting the y axis 

        if (Physics.Raycast(hipsTransform.position, Vector3.down, out RaycastHit hit, 5f, layersToDetectCollision)) // check if we hit the ground for not reset in the ground
        {
            initialPosOfPlayer.y = Mathf.Max(initialPosOfPlayer.y, hit.point.y);
        }

        Vector2 initialPosOfPlayerXZ = new Vector2(initialPosOfPlayer.x, initialPosOfPlayer.z);

        Vector3 getFinalPos = GetFreePosition(initialPosOfPlayerXZ);
        getFinalPos.z = OriginalRootZ;

        if (!IsCapsuleFreeAt(getFinalPos))
        {
            Debug.LogWarning("No free position found to get up player");
            return;
        }

        finalPosition = getFinalPos;

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

    private Vector2 GetFreePosition(Vector2 startPos)
    {
        foreach (Vector2 direction in directions)
        {
            for (int i = 0; i < MAX_ATTEMPTS; i++)
            {
                Vector2 testPos = startPos + direction * (i * STEP_SIZE);
                debugTestPositions.Add(testPos);
                if (IsCapsuleFreeAt(testPos))
                {
                    return testPos;
                }
            }
        }
        Debug.LogWarning("No free position found to get up player");
        return startPos;
    }

    private bool IsCapsuleFreeAt(Vector3 pos)
    {
        // we use Vector3.up because we are testing the capsule in get up
        Vector3 capsuleBottom = pos + Vector3.up * capsuleRadius;
        Vector3 capsuleTop = pos + Vector3.up * (capsuleHeight - capsuleRadius);

        bool isFree = !Physics.CheckCapsule(capsuleBottom, capsuleTop, capsuleRadius, layersToDetectCollision);
        bool IsOnGround = Physics.Raycast(pos + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 1f, layersToDetectCollision);

        return isFree && IsOnGround;
    }

    public void UnInitializeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction -= HandleOnItemCallbackAction;
    }
}
