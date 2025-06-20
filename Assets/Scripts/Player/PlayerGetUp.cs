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

    private Vector3 lastCalculatedPosition;

    private List<Vector3> directions = new List<Vector3>();

    private List<Vector3> foundDirections = new List<Vector3>();

    //DEBUG
    public GameObject CubeFoundGetUpPosDEBUG;
    public GameObject CubeCollidedPosDEBUG;
    public GameObject CubeFloatingPosDEBUG;
    public GameObject CubeStartCalcPosDEBUG;
    public GameObject CubeTestedPosDEBUG;
    public GameObject CubeSelectedPosDEBUG;

    public void HandleOnItemCallbackActionWithOwner(bool isOwner)
    {
        if (isOwner)
        {
            //The item that did the callback was threw by this game
            
            //Both players calculate their positions
            Vector3 foundPos = GetPlayerFreePos();
            ApplyGetUp(foundPos);

            if (IsOwner)
            {
                Debug.Log($"GETUP - Is Owner Of the Item - Owner of the SCRIPT- Will pass to the Not Owner the lastPosition - Found Free Pos: {foundPos} - {gameObject.name}");
                PassOwnerPositionToNotOwnerRpc(foundPos);
            }
            else if (!IsOwner)
            {
                //Im not owner of THIS script, pass the position to the owner 
                //Who could get Hit
                Debug.Log($"GETUP - Is Owner Of the Item - NOT owner of the SCRIPT - Will pass to the Owner the lastPosition - Found Free Pos: {foundPos} - {gameObject.name}");
                PassOwnerPositionToOwnerServerRpc(foundPos);
            }
        }
        else
        {
            //The Item that did the callback wasnt threw by me
            Debug.Log($"GETUP - NOT Is Owner Of the Item - {gameObject.name} - Owner of the Script: {IsOwner}");
            if (IsOwner)
            {
                //Im owner of this SCRIPT and I didnt threw this ITEM
                //Who could get hit
                
                //Use the lastCalculatedPosition Recieved from the other game
                Debug.Log($"GETUP - NOT Is Owner Of the Item - Owner of the Script - Using Last Calculated Pos: {lastCalculatedPosition} - {gameObject.name}");
                ApplyGetUp(lastCalculatedPosition);
            }
            else
            {
                //Im NOT the owner of this SCRIPT and I didnt threw this ITEM
                //The owner of the throw
                Debug.Log($"GETUP - NOT Is Owner Of the Item - NOT Owner of the SCRIPT - Using Last Calculated Pos: {lastCalculatedPosition} - {gameObject.name}");
                ApplyGetUp(lastCalculatedPosition);
            }
            
            // ApplyGetUp(lastCalculatedPosition);
        }
    }
    
    [Rpc(SendTo.Server, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void PassOwnerPositionToNotOwnerServerRpc(Vector3 position)
    {
        PassOwnerPositionToNotOwnerRpc(position);
    }
    
    [Rpc(SendTo.NotOwner, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void PassOwnerPositionToNotOwnerRpc(Vector3 position)
    {
        Debug.Log($"GETUP - NOT Owner Recieved Last Pos: {position} - {gameObject.name}");
        lastCalculatedPosition = position;
    }

    [Rpc(SendTo.Server, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void PassOwnerPositionToOwnerServerRpc(Vector3 position)
    {
        PassOwnerPositionToOwnerRpc(position);
    }
    
    [Rpc(SendTo.Owner, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void PassOwnerPositionToOwnerRpc(Vector3 position)
    {
        Debug.Log($"GETUP - Owner Recieved Last Pos: {position} - {gameObject.name}");
        lastCalculatedPosition = position;
    }
    
    public void HandleOnItemCallbackAction()
    {
        //if (!IsOwner) return;
        // Debug.Log($"GETUP - HandleOnItemCallbackAction Last Pos: {lastCalculatedPosition} - {gameObject.name} - Owner: {IsOwner}");
        // ApplyGetUp(lastCalculatedPosition);
        
        // ApplyGetUp(GetPlayerFreePos());
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    public void SyncPosWithServerRpc()
    {
        //Server Call This
        // SyncOwnerRpc();
        // GetPlayerFreePos(true);
    }
    
    [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
    private void SyncOwnerRpc()
    {
        //Owner Will get own position and pass to others
        PassOwnerPositionToClientsRpc(GetPlayerFreePos());
    }

    [Rpc(SendTo.NotOwner, Delivery = RpcDelivery.Reliable)]
    private void PassOwnerPositionToClientsRpc(Vector3 ownerPos)
    {
        //Not Owner save the position to set it later
        Debug.Log($"GETUP - Recieved Pos {ownerPos} - {gameObject.name} - Owner: {IsOwner}");
        // lastCalculatedPosition = ownerPos;
        // ApplyGetUp(ownerPos);
    }

    private Vector3 GetPlayerFreePos(bool isSyncServer = false)
    {
        Vector3 playerRagdollPosition = hipsTransform.position;
        playerRagdollPosition.y -= verticalOffset;
        playerRagdollPosition.z = defaultZPosition;

        if (Physics.Raycast(hipsTransform.position, Vector3.down, out var hit, 5f, layersToDetectCollision))
            playerRagdollPosition.y = Mathf.Max(playerRagdollPosition.y, hit.point.y);

        //Instantiate(CubeStartCalcPosDEBUG, playerRagdollPosition, Quaternion.identity);
        Vector3 foundFinalPosition = CalculatedFreePosition(playerRagdollPosition);

        Debug.Log($"GETUP POS: {foundFinalPosition} - {gameObject.name} - Owner: {IsOwner} - From Server: {isSyncServer}");
        
        return foundFinalPosition;
        // if (isSyncServer)
        // {
        //     //If is Server, will sync based on the server position
        //     PassPlayerFreePosServerRpc(foundFinalPosition);
        // }
        // else
        // {
        //     //Not server, sync apply now
        //     ApplyGetUp(foundFinalPosition);
        // }
    }

    private Vector3 CalculatedFreePosition(Vector3 startPos)
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
        Debug.Log($"GETUP - APPLY Pos {finalPos} - {gameObject.name} - Owner: {IsOwner}");
        rootTransform.position = finalPos;

        OnPlayerGetUp?.Invoke();
    }
}
