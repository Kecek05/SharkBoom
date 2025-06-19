using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerGetUp : NetworkBehaviour
{
    public event Action OnPlayerGetUp;

    [Header("References")]
    [SerializeField] private Transform rootTransform;
    [SerializeField] private Transform hipsTransform;

    [Header("Settings")]
    [SerializeField] private Collider[] playerColliders;
    [SerializeField] private LayerMask layersToDetectCollision;
    
    private const int MAX_ATTEMPTS = 10;
    private const float STEP_SIZE =  0.5f;
    private const float ANGLE_STEP = 10f;
    private float defaultZPosition = -14.5f;

    private float verticalOffset;
    private float originalRootZ;
    private Quaternion originalRootRotation;
    private Quaternion originalHipsRotation;

    private List<Vector3> directions = new List<Vector3>();

    private List<Vector3> foundDirections = new List<Vector3>();

    //DEBUG
    public GameObject CubeFoundGetUpPosDEBUG;
    public GameObject CubeCollidedPosDEBUG;
    public GameObject CubeFloatingPosDEBUG;
    public GameObject CubeStartCalcPosDEBUG;
    public GameObject CubeTestedPosDEBUG;
    public GameObject CubeSelectedPosDEBUG;

    public void HandleOnItemCallbackAction()
    {
        //if (!IsOwner) return;

        //CalculatePlayerFreePos();
    }

    public void SyncPosWithServer()
    {
        CalculatePlayerFreePos(true);
    }

    private void CalculatePlayerFreePos(bool isSyncServer = false)
    {
        Vector3 playerRagdollPosition = hipsTransform.position;
        playerRagdollPosition.y -= verticalOffset;
        playerRagdollPosition.z = defaultZPosition;

        if (Physics.Raycast(hipsTransform.position, Vector3.down, out var hit, 5f, layersToDetectCollision))
            playerRagdollPosition.y = Mathf.Max(playerRagdollPosition.y, hit.point.y);

        //Instantiate(CubeStartCalcPosDEBUG, playerRagdollPosition, Quaternion.identity);
        Vector3 foundFinalPosition = GetFreePosition(playerRagdollPosition);

        Debug.Log($"CALCULATED POS: {foundFinalPosition} - {gameObject.name} - Owner: {IsOwner} - From Server: {isSyncServer}");
        if (isSyncServer)
        {
            //If is Server, will sync based on the server position
            PassPlayerFreePosServerRpc(foundFinalPosition);
        }
        else
        {
            //Not server, sync apply now
            ApplyGetUp(foundFinalPosition);
        }
    }

    private Vector3 GetFreePosition(Vector3 startPos)
    {
        CalculateXYDirections();
        foundDirections.Clear();
        foreach (Vector3 direction in directions)
        {
            for (int i = 1; i <= MAX_ATTEMPTS; i++)
            {
                Vector3 testDirection = startPos + direction * (i * STEP_SIZE);
                testDirection.z = defaultZPosition;
                //Instantiate(CubeTestedPosDEBUG, testDirection, Quaternion.identity);
                if (AreAllCollidersFreeAt(testDirection))
                {
                    foundDirections.Add(testDirection);
                }
            }
        }

        float closestDistance = float.MaxValue;
        Vector3 closestDirection = Vector3.zero;

        foreach (Vector3 foundDirection in foundDirections)
        {
            float distance = Vector3.Distance(foundDirection, startPos);
            if (closestDistance > distance)
            {
                closestDistance = distance;
                closestDirection = foundDirection;
            }
        }

        return closestDirection;
    }

    private bool AreAllCollidersFreeAt(Vector3 checkPos)
    {
        int selfLayer = gameObject.layer;
        int filteredMask = layersToDetectCollision & ~(1 << selfLayer); // remove self layer from the mask
        foreach (Collider colliders in playerColliders)
        {
            Vector3 localOffset = colliders.transform.position - rootTransform.position;
            Vector3 worldCenter = checkPos + localOffset;

            if (colliders is BoxCollider box)
            {
                Vector3 halfExtents = Vector3.Scale(box.size, colliders.transform.lossyScale) * 0.5f;
                if (Physics.CheckBox(worldCenter, halfExtents, Quaternion.Euler(0f, 0f, 0f), filteredMask))
                {
                    //(CubeCollidedPosDEBUG, worldCenter, Quaternion.identity);
                    return false;
                }
            }
        }

        Vector3 groundCheckOrigin = checkPos + Vector3.up * 0.1f;
        float maxDistance = 3f;
        if (!Physics.Raycast(groundCheckOrigin, Vector3.down, out _, maxDistance, filteredMask))
        {
            //Instantiate(CubeFloatingPosDEBUG, groundCheckOrigin, Quaternion.identity);
            return false;
        }

        //Instantiate(CubeFoundGetUpPosDEBUG, checkPos, Quaternion.identity);
        return true;
    }

    private void CalculateXYDirections()
    {
        directions.Clear();

        for (float angle = 0f; angle < 360f; angle += ANGLE_STEP)
        {
            float rad = angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad);
            float y = Mathf.Sin(rad);
            directions.Add(new Vector3(x, y, 0f).normalized); // direction on XY plane
        }
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void PassPlayerFreePosServerRpc(Vector3 finalPos)
    {
        PassPlayerFreePosClientRpc(finalPos);
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void PassPlayerFreePosClientRpc(Vector3 finalPos)
    {
        ApplyGetUp(finalPos);
    }

    private void ApplyGetUp(Vector3 finalPos)
    {
        rootTransform.position = finalPos;

        OnPlayerGetUp?.Invoke();
    }
}
