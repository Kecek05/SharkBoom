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
    [SerializeField] private Collider[] playerColliders;
    [SerializeField] private LayerMask layersToDetectCollision;

    private bool isFallen = false;
    //private NetworkVariable<bool> isFallen = new NetworkVariable<bool>(false);

    private const int MAX_ATTEMPTS = 500;
    private const float STEP_SIZE =  0.5f;

    private float verticalOffset;
    private float originalRootZ;
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


    //DEBUG

    public DateTime lastGetUpTime;

    public float VerticalOffset => verticalOffset;
    public bool IsFallen => isFallen;
    public Vector3 FinalPosition => finalPosition;
    public float OriginalRootZ => originalRootZ;
    public Quaternion OriginalRootRotation => originalRootRotation;
    public Quaternion OriginalHipsRotation => originalHipsRotation;

    public Quaternion recievedFinalRotation;
    public Quaternion recievedOriginalHipsRotation;

    public void InitializeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction += HandleOnItemCallbackAction;
    }

    public void CacheOriginalPos()
    {
        if(!IsOwner) return;

        isFallen = true;
        originalRootZ = rootTransform.position.z;
        verticalOffset = hipsTransform.position.y - rootTransform.position.y;
        originalHipsRotation = hipsTransform.rotation;
        originalRootRotation = Quaternion.Euler(0, rootTransform.eulerAngles.y, 0);
    }

    private void HandleOnItemCallbackAction()
    {
        if(!IsOwner) return;

        if (!isFallen) return;

        CalculatePlayerFreePos();
    }

    private void CalculatePlayerFreePos()
    {
        Vector3 playerRagdollPosition = hipsTransform.position;
        playerRagdollPosition.y -= verticalOffset;
        playerRagdollPosition.z = originalRootZ;

        if (Physics.Raycast(hipsTransform.position, Vector3.down, out var hit, 5f, layersToDetectCollision))
            playerRagdollPosition.y = Mathf.Max(playerRagdollPosition.y, hit.point.y);

        Vector3 foundFinalPosition = GetFreePosition(playerRagdollPosition);

        isFallen = false;
        finalPosition = foundFinalPosition;
        PassPlayerFreePosServerRpc(foundFinalPosition, originalRootRotation, originalHipsRotation);
        //ApplyGetUp(foundFinalPosition, originalRootRotation);
    }

    private Vector3 GetFreePosition(Vector3 startPos)
    {
        foreach (Vector3 direction in directions)
        {
            for (int i = 1; i <= MAX_ATTEMPTS; i++)
            {
                Vector3 testDirection = startPos + direction * (i * STEP_SIZE);
                testDirection.z = originalRootZ;

                if (AreAllCollidersFreeAt(testDirection))
                {
                    return testDirection;
                }
            }
        }
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
                    return false;
                }
            }

            Vector3 groundCheckOrigin = worldCenter + Vector3.up * 0.1f;

            if (!Physics.Raycast(groundCheckOrigin, Vector3.down, out _, 0.5f, layersToDetectCollision))
            {
                return false;
            }
        }
        return true;
    }


    [Rpc(SendTo.Server)]
    private void PassPlayerFreePosServerRpc(Vector3 finalPos, Quaternion finalRotation, Quaternion originalHipsRotation)
    {
        PassPlayerFreePosClientRpc(finalPos, finalRotation, originalHipsRotation);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PassPlayerFreePosClientRpc(Vector3 finalPos, Quaternion finalRotation, Quaternion originalHipsRotation)
    {
        ApplyGetUp(finalPos, finalRotation, originalHipsRotation);
    }

    private void ApplyGetUp(Vector3 finalPos, Quaternion finalRotation, Quaternion originalHipsRotation)
    {
        recievedOriginalHipsRotation = originalHipsRotation;
        recievedFinalRotation = finalRotation;
        finalPosition = finalPos;
        lastGetUpTime = DateTime.Now;
        Debug.Log($"ApplyGetUp - FinalPos: {finalPos} - FinalRotation: {finalRotation} - OriginalHipsRotation: {originalHipsRotation}");
        rootTransform.SetPositionAndRotation(finalPos, finalRotation);
        hipsTransform.SetPositionAndRotation(finalPos + Vector3.up * verticalOffset, originalHipsRotation);
    }

    public void UnInitializeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction -= HandleOnItemCallbackAction;
    }
}
