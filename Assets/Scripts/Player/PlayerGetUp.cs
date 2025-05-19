using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerGetUp : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rootTransform;
    [SerializeField] private Transform hipsTransform;
    [SerializeField] private Transform ragdollRoot;

    [Header("Settings")]
    [SerializeField] private float capsuleRadius = 0.5f;
    [SerializeField] private float capsuleHeight = 2f;
    [SerializeField] private LayerMask layersToDetectCollision;

    private const int MAX_ATTEMPTS = 10;
    private const int STEP_SIZE = 1;
    private bool isInGoodPosition = false;

    private float verticalOffset;
    private Quaternion originalHipRotation;
    private Quaternion originalRootRotation;
    private Quaternion ragdollRootRotation;

    private Vector3[] directions =
    {
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right
    };

    private Vector3 lastCheckedPosition;

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

    [Rpc(SendTo.Owner)]
    private void CacheOriginalPosClientRpc()
    {
        CacheOriginalPos();
    }

    private void CacheOriginalPos()
    {
        Debug.Log("Hit trigger - Cache original Pos");
        originalHipRotation = hipsTransform.rotation;
        originalRootRotation = rootTransform.rotation;
        ragdollRootRotation = ragdollRoot.rotation;

        verticalOffset = hipsTransform.position.y - rootTransform.position.y;
    }

    private void HandleOnItemCallbackAction()
    {
        if (IsOwner)
        {
            RequestGetUpPlayerServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestGetUpPlayerServerRpc()
    {
        RequestGetUpPlayerClientRpc();
    }

    [Rpc(SendTo.Owner)]
    private void RequestGetUpPlayerClientRpc()
    {
        GetUpPlayer();
    }

    public void GetUpPlayer()
    {
        Vector3 initialPosOfPlayer = hipsTransform.position;
        initialPosOfPlayer.y -= verticalOffset;  // correcting the y axis 

        if (Physics.Raycast(hipsTransform.position, Vector3.down, out RaycastHit hit, 5f)) // check if we hit the ground for not reset in the ground
        {
            initialPosOfPlayer.y = Mathf.Max(initialPosOfPlayer.y, hit.point.y);
        }

        Vector3 finalPos = GetFreePosition(initialPosOfPlayer);

        if(!IsCapsuleFreeAt(finalPos))
        {
            Debug.LogWarning("No free position found to get up player");
            return;
        }
        

        // Send all for original rotation, basead on new position
        rootTransform.SetPositionAndRotation(finalPos, originalRootRotation);
        hipsTransform.rotation = originalHipRotation;
        ragdollRoot.rotation = ragdollRootRotation;
    }

    private Vector3 GetFreePosition(Vector3 startPos)
    {
        foreach (Vector3 direction in directions)
        {
            for (int i = 0; i < MAX_ATTEMPTS; i++)
            {
                Vector3 testPos = startPos + direction * (i * STEP_SIZE);
                lastCheckedPosition = testPos;

                if (IsCapsuleFreeAt(testPos))
                {
                    return testPos;
                }
            }
        }
        return startPos; // Return the original position if no free position is found
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 capsuleBottom = lastCheckedPosition + Vector3.up * capsuleRadius;
        Vector3 capsuleTop = lastCheckedPosition + Vector3.up * (capsuleHeight - capsuleRadius);

        Gizmos.DrawWireSphere(capsuleBottom, capsuleRadius);
        Gizmos.DrawWireSphere(capsuleTop, capsuleRadius);
        Gizmos.DrawLine(capsuleBottom + Vector3.forward * capsuleRadius, capsuleTop + Vector3.forward * capsuleRadius);
        Gizmos.DrawLine(capsuleBottom + Vector3.back * capsuleRadius, capsuleTop + Vector3.back * capsuleRadius);
        Gizmos.DrawLine(capsuleBottom + Vector3.left * capsuleRadius, capsuleTop + Vector3.left * capsuleRadius);
        Gizmos.DrawLine(capsuleBottom + Vector3.right * capsuleRadius, capsuleTop + Vector3.right * capsuleRadius);
    }
}
