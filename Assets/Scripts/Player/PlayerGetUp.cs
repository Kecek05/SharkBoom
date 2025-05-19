using Unity.Netcode;
using UnityEngine;

public class PlayerGetUp : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rootTransform;
    [SerializeField] private Transform hipsTransform;
    [SerializeField] private Transform ragdollRoot;

    [Header("Settings")]
    private float capsuleRadius = 0.5f;
    private float capsuleHeight = 2f;
    private LayerMask[] layersToDetectCollision;

    private const int MAX_ATTEMPTS = 10;
    private const float VERTICAL_STEP = 0.1f;
    private bool isInGoodPosition = false;

    private float verticalOffset;
    private Quaternion originalHipRotation;
    private Quaternion originalRootRotation;
    private Quaternion ragdollRootRotation;


    public void HitRecieveNotify()
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

    public void GetUpPlayer()
    {
        Vector3 targetPos = hipsTransform.position;
        targetPos.y -= verticalOffset;  // correcting the y axis 

        if (Physics.Raycast(hipsTransform.position, Vector3.down, out RaycastHit hit, 5f)) // check if we hit the ground for not reset in the ground
        {
            targetPos.y = Mathf.Max(targetPos.y, hit.point.y);
        }

        Vector3 capsuleBottom = targetPos + Vector3.up * capsuleHeight;
        Vector3 capsuleTop = targetPos + Vector3.up * (capsuleHeight - capsuleRadius);

        for(int i = 0; i < MAX_ATTEMPTS; i++)
        {
            if (!Physics.CheckCapsule(capsuleBottom, capsuleTop, capsuleRadius, LayerMask.GetMask("Default")))
            {
                isInGoodPosition = true;
                break;
            }

            targetPos.y += VERTICAL_STEP;
            capsuleBottom += Vector3.up * capsuleHeight;
            capsuleTop += Vector3.up * VERTICAL_STEP;
        }
        
        if(!isInGoodPosition)
        {
            Debug.LogWarning("No good position found to get up player");
            return;
        }

        // Send all for original rotation, basead on new position
        rootTransform.SetPositionAndRotation(targetPos, originalRootRotation);
        hipsTransform.rotation = originalHipRotation;
        ragdollRoot.rotation = ragdollRootRotation;
    }

    
}
