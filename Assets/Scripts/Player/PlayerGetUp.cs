using QFSW.QC;
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

    private bool isFallen = false;
    //private NetworkVariable<bool> isFallen = new NetworkVariable<bool>(false);

    private const int MAX_ATTEMPTS = 50;
    private const float STEP_SIZE =  0.5f;
    private const float ANGLE_STEP = 10f;
    private Vector3 defaultHipsRotation = new Vector3(-90f, 180f, 0f);
    private Vector3 defaultHipsPosition = new Vector3(0f,0f,0f);
    private float defaultZPosition = -14f;

    private float verticalOffset;
    private float originalRootZ;
    private Quaternion originalRootRotation;
    private Quaternion originalHipsRotation;
    private Vector3 finalPosition;

    //private Vector3[] directions =
    //{
    //    Vector3.up,
    //    Vector3.forward,
    //    Vector3.back,
    //    Vector3.left,
    //    Vector3.right,
    //    Vector3.forward + Vector3.left,
    //    Vector3.forward + Vector3.right,
    //    Vector3.back + Vector3.left,
    //    Vector3.back + Vector3.right
    //};

    private List<Vector3> directions;


    //DEBUG
    public GameObject CubeHipDEBUG;
    public GameObject CubeRootDEBUG;
    public GameObject CubeFreePosDEBUG;
    public GameObject CubeTestedPosDEBUG;

    public DateTime lastGetUpTime;

    public float VerticalOffset => verticalOffset;
    public bool IsFallen => isFallen;
    public Vector3 FinalPosition => finalPosition;
    public float OriginalRootZ => originalRootZ;
    public Quaternion OriginalRootRotation => originalRootRotation;
    public Quaternion OriginalHipsRotation => originalHipsRotation;

    public Quaternion recievedFinalRotation;
    public Quaternion recievedOriginalHipsRotation;

    public void CacheOriginalPos()
    {
        if(!IsOwner) return;
        Debug.Log("Getup - CacheOriginalPos");
        //isFallen = true;
        //originalRootZ = rootTransform.position.z;
        //verticalOffset = hipsTransform.position.y - rootTransform.position.y;
        //originalHipsRotation = hipsTransform.rotation;
        //originalRootRotation = Quaternion.Euler(0, rootTransform.eulerAngles.y, 0);
    }

    public void HandleOnRagdollDisabled()
    {
        if (!IsOwner) return;

        //if (!isFallen) return;

        //CalculatePlayerFreePos();
    }

    [Command("getup", MonoTargetType.All)]
    public void GetUpDebug()
    {
        if(!IsOwner) return;
        CalculatePlayerFreePos();
    }

    private void CalculatePlayerFreePos()
    {
        Debug.Log("Getup - CalculatePlayerFreePos");
        Vector3 playerRagdollPosition = hipsTransform.position;
        playerRagdollPosition.y -= verticalOffset;
        playerRagdollPosition.z = defaultZPosition;

        if (Physics.Raycast(hipsTransform.position, Vector3.down, out var hit, 5f, layersToDetectCollision))
            playerRagdollPosition.y = Mathf.Max(playerRagdollPosition.y, hit.point.y);

        Vector3 foundFinalPosition = GetFreePosition(playerRagdollPosition);

        //isFallen = false;
        finalPosition = foundFinalPosition;
        PassPlayerFreePosServerRpc(foundFinalPosition);
        //ApplyGetUp(foundFinalPosition, originalRootRotation);
    }

    private Vector3 GetFreePosition(Vector3 startPos)
    {
        CalculateXYDirections();
        foreach (Vector3 direction in directions)
        {
            for (int i = 1; i <= MAX_ATTEMPTS; i++)
            {
                Vector3 testDirection = startPos + direction * (i * STEP_SIZE);
                testDirection.z = defaultZPosition;
                Instantiate(CubeTestedPosDEBUG, testDirection, Quaternion.identity);
                if (AreAllCollidersFreeAt(testDirection))
                {
                    Instantiate(CubeFreePosDEBUG, testDirection, Quaternion.identity);
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

    private void CalculateXYDirections()
    {
        directions = new List<Vector3>();

        for (float angle = 0f; angle < 360f; angle += ANGLE_STEP)
        {
            float rad = angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad);
            float y = Mathf.Sin(rad);
            directions.Add(new Vector3(x, y, 0f).normalized); // direction on XY plane
            Debug.Log("Direction added: " + new Vector3(x, y, 0).normalized);
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
        //recievedOriginalHipsRotation = originalHipsRotation;
        //recievedFinalRotation = Quaternion.Euler(finalRotation);
        finalPosition = finalPos;
        lastGetUpTime = DateTime.Now;
        //Debug.Log($"GetUp - ApplyGetUp - FinalPos: {finalPos} - FinalRotation: {finalRotation} - OriginalHipsRotation: {originalHipsRotation}"); //DEBUG
        InstantiateCubesForDebug(finalPos);
        OnPlayerGetUp?.Invoke();

        //hipsTransform.SetPositionAndRotation(defaultHipsPosition, Quaternion.Euler(defaultHipsRotation));
        hipsTransform.transform.rotation = Quaternion.Euler(defaultHipsRotation);
        rootTransform.transform.position = finalPos;


        //hipsTransform.SetPositionAndRotation(finalPos + Vector3.up * verticalOffset, originalHipsRotation);
        //rootTransform.SetPositionAndRotation(finalPos, Quaternion.Euler(finalRotation));
    }

    private void InstantiateCubesForDebug(Vector3 finalPos)
    {
        Instantiate(CubeHipDEBUG, finalPos + Vector3.up * verticalOffset, originalHipsRotation);
        Instantiate(CubeRootDEBUG, finalPos, originalHipsRotation);
    }

    public void UnInitializeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction -= HandleOnRagdollDisabled;
    }
}
