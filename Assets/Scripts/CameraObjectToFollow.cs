using Unity.Cinemachine;
using UnityEngine;

public class CameraObjectToFollow : MonoBehaviour
{
    [SerializeField] private CinemachineConfiner3D cinemachineConfiner3D;
    private Vector3 confinedPosition;

    private void Update()
    {
        confinedPosition = cinemachineConfiner3D.BoundingVolume.ClosestPoint(transform.position);
        transform.position = confinedPosition;
    }
}