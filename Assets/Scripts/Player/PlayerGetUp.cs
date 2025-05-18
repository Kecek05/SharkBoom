using Unity.Netcode;
using UnityEngine;

public class PlayerGetUp : MonoBehaviour
{

    [SerializeField] private Transform rootTransform;
    [SerializeField] private Transform hipsTransform;
    [SerializeField] private Transform ragdollRoot;

    private float verticalOffset = 0f;

    private Quaternion originalHipRotation;
    private Quaternion originalRootRotation;
    private Quaternion ragdollRootRotation;


    [Rpc(SendTo.Server)]
    private void CacheOriginalPosServerRpc()
    {
        CacheOriginalPosClientRpc();
    }

    [Rpc(SendTo.Owner)]
    private void CacheOriginalPosClientRpc()
    {
        CacheOriginalPos();
    }

    public void CacheOriginalPos()
    {
        //originalHipRotation = hipsTransform.rotation;
        //originalRootRotation = rootTransform.rotation;
        //ragdollRootRotation = ragdollRoot.rotation;

        //verticalOffset = hipsTransform.position.y - rootTransform.position.y;
        Debug.Log("Test Cache Pos");
    }

    public void GetUpPlayer()
    {
        Vector3 newPosition = hipsTransform.position;
        newPosition.y -= verticalOffset;  // correcting the y axis 

        if (Physics.Raycast(hipsTransform.position, Vector3.down, out RaycastHit hit, 5f)) // check if we hit the ground for not reset in the ground
        {
            float groundY = hit.point.y;

            if (newPosition.y < groundY)
            {
                newPosition.y = groundY;
            }
        }

        // Send all for original rotation, basead on new position
        rootTransform.SetPositionAndRotation(newPosition, originalRootRotation);
        hipsTransform.rotation = originalHipRotation;
        ragdollRoot.rotation = ragdollRootRotation;
    }

    
}
